using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace SamuraiStandoff
{
    /// <summary>
    /// Handles all disk I/O for Samurai Standoff, with Steam Cloud mirroring.
    ///
    /// WHY THIS EXISTS
    /// ───────────────
    /// ScriptableObjects are assets — their runtime changes exist only
    /// in memory and are discarded when the build exits.  This system
    /// serializes the fields that must persist (progression, stats,
    /// settings, keybinds) to a JSON file on disk, then pushes the
    /// loaded values back into the ScriptableObjects on startup so
    /// that every other script can keep reading from them unchanged.
    ///
    /// STEAM CLOUD
    /// ───────────
    /// When Steamworks is available and cloud sync is enabled for the app,
    /// every Save() also mirrors the same JSON bytes to ISteamRemoteStorage.
    /// On Load(), if a cloud file exists and is newer than the local file,
    /// the cloud version wins and is also written back to local disk so
    /// both copies stay in sync.  Timestamp-wins resolves conflicts without
    /// requiring any UI — the same strategy used by the majority of shipped
    /// Steam titles.
    ///
    /// USAGE
    /// ─────
    ///  • Call SaveSystem.instance.Save() anywhere a durable change
    ///    occurs: end of duel, settings changed, difficulty completed.
    ///  • GameManager.Awake() calls Load() automatically.
    ///  • SOs (PlayerData / GameData) remain the live in-memory source
    ///    of truth during a session — this class is only I/O.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem instance;

        [SerializeField] private PlayerData playerData;
        [SerializeField] private GameData   gameData;

        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, "SamuraiStandoff.save");

        // Steam Remote Storage uses a flat filename with no path separators.
        private const string CloudFileName = "SamuraiStandoff.save";

        
        #region Unity

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        #endregion
        
        #region Public API

        /// <summary>
        /// Reads the save file and pushes values into the ScriptableObjects.
        /// Call once on startup (GameManager.Awake).
        ///
        /// Resolution order:
        ///   1. Try Steam Cloud — if available, enabled, and newer than local → use it
        ///      and write it back to local disk to keep both copies in sync.
        ///   2. Fall back to local disk file.
        ///   3. If neither exists, SOs keep their default inspector values.
        /// </summary>
        public void Load()
        {
            try
            {
                // 1. Attempt Steam Cloud load
                string cloudJson = CloudLoad();

                if (cloudJson != null)
                {
                    // Cloud is authoritative — deserialize and also sync to disk.
                    SamuraiSaveData cloudData = JsonConvert.DeserializeObject<SamuraiSaveData>(cloudJson);
                    if (cloudData != null)
                    {
                        ApplyToPlayerData(cloudData);
                        ApplyToGameData(cloudData);
                        File.WriteAllText(SavePath, cloudJson); // keep local in sync
                        Debug.Log("[SaveSystem] Loaded from Steam Cloud (newer than local).");
                        return;
                    }
                    Debug.LogWarning("[SaveSystem] Steam Cloud file was malformed — falling back to local.");
                }

                // 2. Fall back to local disk
                if (!File.Exists(SavePath))
                {
                    Debug.Log("[SaveSystem] No save file found — using defaults.");
                    return;
                }

                string localJson = File.ReadAllText(SavePath);
                SamuraiSaveData localData = JsonConvert.DeserializeObject<SamuraiSaveData>(localJson);

                if (localData == null)
                {
                    Debug.LogWarning("[SaveSystem] Local save file was empty or malformed.");
                    return;
                }

                ApplyToPlayerData(localData);
                ApplyToGameData(localData);
                Debug.Log("[SaveSystem] Loaded from local disk.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load save file: {e.Message}");
            }
        }

        /// <summary>
        /// Reads current values from the ScriptableObjects and writes them to
        /// local disk, then mirrors the same bytes to Steam Cloud if available.
        /// Safe to call frequently — JSON serialization is fast for this payload.
        /// </summary>
        public void Save()
        {
            try
            {
                SamuraiSaveData data = BuildFromScriptableObjects();
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);

                // Local disk (always) 
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] Saved to local disk: {SavePath}");

                // Steam Cloud (when available) 
                CloudSave(Encoding.UTF8.GetBytes(json));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save: {e.Message}");
            }
        }

        /// <summary>
        /// Deletes the save file and resets both SOs to their default SO values.
        /// Useful for the "Reset Progression" developer button.
        /// </summary>
        public void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] Save file deleted.");
            }
            else
            {
                Debug.Log("[SaveSystem] No save file to delete.");
            }
        }

        #endregion
        
        #region Private — SO → Save

        private SamuraiSaveData BuildFromScriptableObjects()
        {
            var d = new SamuraiSaveData();

            // Character
            d.characterType = (int)playerData.characterType;

            // Progression
            d.completedEasyMode   = playerData.completedEasyMode;
            d.completedMediumMode = playerData.completedMediumMode;
            d.completedHardMode   = playerData.completedHardMode;

            // Analytics flags
            d.startedFirstDuel        = playerData.startedFirstDuel;
            d.wonFirstDuel            = playerData.wonFirstDuel;
            d.reachedMediumDifficulty = playerData.reachedMediumDifficulty;
            d.reachedHardDifficulty   = playerData.reachedHardDifficulty;
            d.defeatedFraug           = playerData.defeatedFraug;

            // Combat stats
            d.perfectTimingWins = playerData.m_perfectTimingWins;
            d.totalEarlyAttacks = playerData.m_totalEarlyAttacks;
            d.currentWinStreak  = playerData.m_currentWinStreak;
            d.bestWinStreak     = playerData.m_bestWinStreak;
            d.totalDuels        = playerData.m_totalDuels;
            d.totalWins         = playerData.m_totalWins;
            d.totalLosses       = playerData.m_totalLosses;
            d.totalDraws        = playerData.m_totalDraws;
            d.maxWinStreak      = playerData.m_maxWinStreak;

            // Best time
            d.currentBestFrameCount = playerData.currentBestFrameCount;

            // Multiplayer stats
            d.multiplayerWins              = playerData.multiplayerWins;
            d.multiplayerLosses            = playerData.multiplayerLosses;
            d.multiplayerBestWinStreak     = playerData.multiplayerBestWinStreak;
            d.multiplayerCurrentWinStreak  = playerData.multiplayerCurrentWinStreak;

            // Audio (GameData)
            d.masterVolume     = gameData.masterVolume;
            d.backgroundVolume = gameData.backgroundVolume;

            // Keybindings (GameData) — store as int list
            d.attackKeys   = KeyCodesToInts(gameData.attackKeys);
            d.p2AttackKeys = KeyCodesToInts(gameData.p2AttackKeys);

            return d;
        }

        #endregion
        
        #region Private — Save → SO

        private void ApplyToPlayerData(SamuraiSaveData d)
        {
            playerData.characterType = (CharacterType)d.characterType;

            playerData.completedEasyMode   = d.completedEasyMode;
            playerData.completedMediumMode = d.completedMediumMode;
            playerData.completedHardMode   = d.completedHardMode;

            playerData.startedFirstDuel        = d.startedFirstDuel;
            playerData.wonFirstDuel            = d.wonFirstDuel;
            playerData.reachedMediumDifficulty = d.reachedMediumDifficulty;
            playerData.reachedHardDifficulty   = d.reachedHardDifficulty;
            playerData.defeatedFraug           = d.defeatedFraug;

            playerData.m_perfectTimingWins = d.perfectTimingWins;
            playerData.m_totalEarlyAttacks = d.totalEarlyAttacks;
            playerData.m_currentWinStreak  = d.currentWinStreak;
            playerData.m_bestWinStreak     = d.bestWinStreak;
            playerData.m_totalDuels        = d.totalDuels;
            playerData.m_totalWins         = d.totalWins;
            playerData.m_totalLosses       = d.totalLosses;
            playerData.m_totalDraws        = d.totalDraws;
            playerData.m_maxWinStreak      = d.maxWinStreak;

            playerData.currentBestFrameCount = d.currentBestFrameCount;

            playerData.multiplayerWins             = d.multiplayerWins;
            playerData.multiplayerLosses           = d.multiplayerLosses;
            playerData.multiplayerBestWinStreak    = d.multiplayerBestWinStreak;
            playerData.multiplayerCurrentWinStreak = d.multiplayerCurrentWinStreak;
        }

        private void ApplyToGameData(SamuraiSaveData d)
        {
            gameData.masterVolume     = d.masterVolume;
            gameData.backgroundVolume = d.backgroundVolume;

            // Only overwrite keybinds if the save actually has them
            if (d.attackKeys != null && d.attackKeys.Count > 0)
                gameData.attackKeys = IntsToKeyCodes(d.attackKeys);

            if (d.p2AttackKeys != null && d.p2AttackKeys.Count > 0)
                gameData.p2AttackKeys = IntsToKeyCodes(d.p2AttackKeys);
        }

        #endregion
        
        #region Helpers

        private static List<int> KeyCodesToInts(List<KeyCode> keys)
        {
            var result = new List<int>(keys.Count);
            foreach (var k in keys) result.Add((int)k);
            return result;
        }

        private static List<KeyCode> IntsToKeyCodes(List<int> ints)
        {
            var result = new List<KeyCode>(ints.Count);
            foreach (var i in ints) result.Add((KeyCode)i);
            return result;
        }

        #endregion

        
        #region Steam Cloud

        /// <summary>
        /// Returns true when Steamworks is running AND the user has cloud
        /// sync enabled for this app in their Steam client settings.
        /// All cloud operations must be gated behind this check.
        /// </summary>
        private static bool IsCloudAvailable()
        {
#if !DISABLESTEAMWORKS
            return SteamManager.Initialized
                && SteamRemoteStorage.IsCloudEnabledForApp();
#else
            return false;
#endif
        }

        /// <summary>
        /// Writes <paramref name="bytes"/> to Steam Remote Storage.
        /// Uses BeginFileWriteBatch / EndFileWriteBatch for atomicity —
        /// good practice even for a single file, required for multi-file saves.
        /// </summary>
        private static void CloudSave(byte[] bytes)
        {
#if !DISABLESTEAMWORKS
            if (!IsCloudAvailable()) return;

            try
            {
                SteamRemoteStorage.BeginFileWriteBatch();
                bool ok = SteamRemoteStorage.FileWrite(CloudFileName, bytes, bytes.Length);
                SteamRemoteStorage.EndFileWriteBatch();

                if (ok)
                    Debug.Log("[SaveSystem] Mirrored to Steam Cloud.");
                else
                    Debug.LogWarning("[SaveSystem] SteamRemoteStorage.FileWrite returned false — cloud save may have failed.");
            }
            catch (Exception e)
            {
                // Never let a cloud failure crash the save path — local disk is already written.
                Debug.LogError($"[SaveSystem] Steam Cloud save error: {e.Message}");
            }
#endif
        }

        /// <summary>
        /// Attempts to load from Steam Remote Storage.
        ///
        /// Returns the JSON string from the cloud file if:
        ///   • Cloud is available and the file exists in Remote Storage, AND
        ///   • The cloud timestamp is strictly newer than the local file's
        ///     last-write time (or no local file exists at all).
        ///
        /// Returns null in all other cases — caller falls back to local disk.
        /// </summary>
        private static string CloudLoad()
        {
#if !DISABLESTEAMWORKS
            if (!IsCloudAvailable())                                    return null;
            if (!SteamRemoteStorage.FileExists(CloudFileName))          return null;

            try
            {
                // Timestamp comparison
                long cloudUnixTimestamp = SteamRemoteStorage.GetFileTimestamp(CloudFileName);

                if (File.Exists(SavePath))
                {
                    long localUnixTimestamp =
                        (long)(File.GetLastWriteTimeUtc(SavePath) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                        .TotalSeconds;

                    if (localUnixTimestamp >= cloudUnixTimestamp)
                    {
                        Debug.Log("[SaveSystem] Local file is same age or newer than Steam Cloud — using local.");
                        return null; // local wins
                    }
                }

                //  Cloud is newer (or no local file) — read it 
                int fileSize = SteamRemoteStorage.GetFileSize(CloudFileName);
                if (fileSize <= 0)
                {
                    Debug.LogWarning("[SaveSystem] Steam Cloud file reported size 0 — skipping.");
                    return null;
                }

                byte[] buffer = new byte[fileSize];
                int bytesRead = SteamRemoteStorage.FileRead(CloudFileName, buffer, fileSize);

                if (bytesRead <= 0)
                {
                    Debug.LogWarning("[SaveSystem] SteamRemoteStorage.FileRead returned 0 bytes.");
                    return null;
                }

                Debug.Log($"[SaveSystem] Read {bytesRead} bytes from Steam Cloud.");
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Steam Cloud load error: {e.Message}");
                return null;
            }
#else
            return null;
#endif
        }

        #endregion
    }
}