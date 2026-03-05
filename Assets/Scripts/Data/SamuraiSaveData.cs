using System.Collections.Generic;

namespace SamuraiStandoff
{
    /// <summary>
    /// Plain C# class that gets serialized to disk via JSON.
    /// Mirrors every field from PlayerData and the persistent
    /// fields from GameData (volumes, keybinds) that must
    /// survive between sessions.
    ///
    /// ScriptableObjects DO NOT persist in builds — this is the
    /// source of truth for anything that needs to be remembered.
    /// </summary>
    [System.Serializable]
    public class SamuraiSaveData
    {
        // Character
        public int characterType;           // Cast to/from CharacterType enum

        // Progression
        public bool completedEasyMode;
        public bool completedMediumMode;
        public bool completedHardMode;

        // Analytics flags
        public bool startedFirstDuel;
        public bool wonFirstDuel;
        public bool reachedMediumDifficulty;
        public bool reachedHardDifficulty;
        public bool defeatedFraug;

        // Combat statistics
        public int perfectTimingWins;
        public int totalEarlyAttacks;
        public int currentWinStreak;
        public int bestWinStreak;
        public int totalDuels;
        public int totalWins;
        public int totalLosses;
        public int totalDraws;
        public int maxWinStreak;

        // Best times
        public int currentBestFrameCount;

        // Multiplayer statistics
        public int multiplayerWins;
        public int multiplayerLosses;
        public int multiplayerBestWinStreak;
        public int multiplayerCurrentWinStreak;

        // Audio settings (from GameData)
        public float masterVolume  = 80f;
        public float backgroundVolume = 100f;

        // Keybindings (from GameData)
        // Stored as ints so they survive serialization (KeyCode is an enum).
        public List<int> attackKeys   = new List<int>();
        public List<int> p2AttackKeys = new List<int>();
    }
}