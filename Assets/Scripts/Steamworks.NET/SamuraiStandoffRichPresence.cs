using UnityEngine;
#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SamuraiStandoff
{
    /// <summary>
    /// Manages Steam Rich Presence to show what players are doing
    /// Displays on Steam profile and friend list
    /// </summary>
    public class SteamRichPresenceManager : MonoBehaviour
    {
        #region Singleton
        
        public static SteamRichPresenceManager instance;
        
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
            }
        }
        
        #endregion
        
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GameData gameData;
        
#if STEAMWORKS_NET
        
        #region Rich Presence Updates
        
        /// <summary>
        /// Update presence when in main menu
        /// </summary>
        public void SetInMenu()
        {
            if (!SteamManager.Initialized) return;
            
            SteamFriends.SetRichPresence("steam_display", "#Status_Menu");
            SteamFriends.SetRichPresence("state", "In Menu");
            
            Debug.Log("[Rich Presence] Set to: In Menu");
        }
        
        /// <summary>
        /// Update presence when in single player duel
        /// </summary>
        public void SetInDuel(EnemyDifficultyType difficulty, int level, int totalLevels, CharacterType character)
        {
            if (!SteamManager.Initialized) return;
            
            string difficultyStr = GetDifficultyString(difficulty);
            
            // Set rich presence keys
            SteamFriends.SetRichPresence("steam_display", "#Status_Playing");
            SteamFriends.SetRichPresence("state", "In Duel");
            SteamFriends.SetRichPresence("difficulty", difficultyStr);
            SteamFriends.SetRichPresence("level", $"{level}/{totalLevels}");
            SteamFriends.SetRichPresence("character", character.ToString());
            
            if (playerData != null && playerData.m_maxWinStreak > 0)
            {
                SteamFriends.SetRichPresence("streak", playerData.m_maxWinStreak.ToString());
            }
            
            Debug.Log($"[Rich Presence] Set to: {difficultyStr} Duel - Level {level}/{totalLevels} as {character}");
        }
        
        /// <summary>
        /// Update presence when in multiplayer
        /// </summary>
        public void SetInMultiplayer(CharacterType playerCharacter, CSteamID opponentSteamID)
        {
            if (!SteamManager.Initialized) return;
            
            string opponentName = SteamFriends.GetFriendPersonaName(opponentSteamID);
            
            SteamFriends.SetRichPresence("steam_display", "#Status_Multiplayer");
            SteamFriends.SetRichPresence("state", "Multiplayer Duel");
            SteamFriends.SetRichPresence("character", playerCharacter.ToString());
            SteamFriends.SetRichPresence("opponent", opponentName);
            
            Debug.Log($"[Rich Presence] Set to: Multiplayer vs {opponentName}");
        }
        
        /// <summary>
        /// Update presence when in multiplayer (no opponent info)
        /// </summary>
        public void SetInMultiplayerLocal(CharacterType playerCharacter)
        {
            if (!SteamManager.Initialized) return;
            
            SteamFriends.SetRichPresence("steam_display", "#Status_Multiplayer");
            SteamFriends.SetRichPresence("state", "Multiplayer Duel");
            SteamFriends.SetRichPresence("character", playerCharacter.ToString());
            
            Debug.Log($"[Rich Presence] Set to: Local Multiplayer");
        }
        
        /// <summary>
        /// Update presence when on character select
        /// </summary>
        public void SetSelectingCharacter()
        {
            if (!SteamManager.Initialized) return;
            
            SteamFriends.SetRichPresence("steam_display", "#Status_CharacterSelect");
            SteamFriends.SetRichPresence("state", "Selecting Character");
            
            Debug.Log("[Rich Presence] Set to: Character Select");
        }
        
        /// <summary>
        /// Update presence when viewing leaderboards
        /// </summary>
        public void SetViewingLeaderboards()
        {
            if (!SteamManager.Initialized) return;
            
            SteamFriends.SetRichPresence("steam_display", "#Status_Leaderboards");
            SteamFriends.SetRichPresence("state", "Viewing Leaderboards");
            
            Debug.Log("[Rich Presence] Set to: Viewing Leaderboards");
        }
        
        /// <summary>
        /// Clear all rich presence
        /// </summary>
        public void ClearPresence()
        {
            if (!SteamManager.Initialized) return;
            
            SteamFriends.ClearRichPresence();
            Debug.Log("[Rich Presence] Cleared");
        }
        
        #endregion
        
        #region Auto-Update Based on Game State
        
        /// <summary>
        /// Automatically update rich presence based on current game state
        /// Call this from your scene managers
        /// </summary>
        public void UpdatePresenceFromGameState()
        {
            if (!SteamManager.Initialized) return;
            
            if (gameData == null || playerData == null)
            {
                SetInMenu();
                return;
            }
            
            // Check if in multiplayer
            if (gameData.isMultiplayer)
            {
                SetInMultiplayerLocal(playerData.characterType);
            }
            // Check if in a duel
            else if (playerData.currentLevel > 0)
            {
                SetInDuel(
                    gameData.currentDifficulty,
                    playerData.currentLevel,
                    GameManager.instance.totalLevels,
                    playerData.characterType
                );
            }
            // Default to menu
            else
            {
                SetInMenu();
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private string GetDifficultyString(EnemyDifficultyType difficulty)
        {
            switch (difficulty)
            {
                case EnemyDifficultyType.Tutorial:
                    return "Tutorial";
                case EnemyDifficultyType.EasyMode:
                    return "Easy";
                case EnemyDifficultyType.MediumMode:
                    return "Medium";
                case EnemyDifficultyType.HardMode:
                    return "Hard";
                default:
                    return "Unknown";
            }
        }
        
        #endregion
        
        #region Application Events
        
        private void OnApplicationQuit()
        {
            ClearPresence();
        }
        
        #endregion
        
#else
        private void Start()
        {
            Debug.LogWarning("[Rich Presence] Steamworks not enabled - Rich Presence disabled");
        }
        
        // Stub methods when Steam is disabled
        public void SetInMenu() { }
        public void SetInDuel(EnemyDifficultyType difficulty, int level, int totalLevels, CharacterType character) { }
        public void SetInMultiplayer(CharacterType playerCharacter, CSteamID opponentSteamID) { }
        public void SetInMultiplayerLocal(CharacterType playerCharacter) { }
        public void SetSelectingCharacter() { }
        public void SetViewingLeaderboards() { }
        public void ClearPresence() { }
        public void UpdatePresenceFromGameState() { }
#endif
    }
}

/*
 * STEAMWORKS BACKEND CONFIGURATION
 * =================================
 * 
 * You need to configure Rich Presence localization tokens in your Steamworks app settings.
 * Go to: Steamworks Partner Site > Your App > Community > Rich Presence
 * 
 * Add these tokens:
 * 
 * Token: #Status_Menu
 * English: In Menu
 * 
 * Token: #Status_Playing
 * English: {#state} - {#difficulty} Level {#level} as {#character}
 * Example output: "In Duel - Medium Level 2/3 as Monk"
 * 
 * Token: #Status_Multiplayer
 * English: Multiplayer Duel as {#character}
 * Example output: "Multiplayer Duel as Ichi"
 * 
 * Token: #Status_CharacterSelect
 * English: Selecting Character
 * 
 * Token: #Status_Leaderboards
 * English: Viewing Leaderboards
 * 
 * Optional - if you want to show opponent in multiplayer:
 * English: Multiplayer vs {#opponent} as {#character}
 * 
 * Optional - if you want to show win streak:
 * Add to #Status_Playing: {#state} - {#difficulty} Level {#level} as {#character} (Streak: {#streak})
 */