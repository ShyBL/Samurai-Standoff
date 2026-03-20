using Steamworks;
using UnityEngine;

namespace SamuraiStandoff
{
    public class SamuraiStandoffStats : MonoBehaviour
    {
        public bool testing;
        
        // Reference to the shared progression data
        public PlayerData playerData;

        // Steamworks API
        private CGameID m_GameID;
        private bool m_bRequestedStats;
        private bool m_bStatsValid;
        public bool m_bStoreStats; // Flag to trigger storing stats

        protected Callback<UserStatsReceived_t> m_UserStatsReceived;
        protected Callback<UserStatsStored_t> m_UserStatsStored;
        protected Callback<UserAchievementStored_t> m_UserAchievementStored;


        #region Singleton

        public static SamuraiStandoffStats instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                if (!testing) return;
                
                ClearAchievements();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            if (!SteamManager.Initialized) return;

            m_GameID = new CGameID(SteamUtils.GetAppID());
            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
        }

        private void Update()
        {
            if (!SteamManager.Initialized) return;

            if (!m_bRequestedStats)
            {
                var success = SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
                m_bRequestedStats = true;
                Debug.Log("Requesting stats from Steam...");
            }

            if (!m_bStatsValid) return;

            // If a change has occurred, store the stats
            if (m_bStoreStats)
            {
                // Set aggregate stats
                SteamUserStats.SetStat("TotalDuels", playerData.m_totalDuels);
                SteamUserStats.SetStat("TotalWins", playerData.m_totalWins);
                SteamUserStats.SetStat("TotalLosses", playerData.m_totalLosses);
                SteamUserStats.SetStat("TotalDraws", playerData.m_totalDraws);
                SteamUserStats.SetStat("BestWinStreak", playerData.m_bestWinStreak);
                SteamUserStats.SetStat("TotalEarlyAttacks", playerData.m_totalEarlyAttacks);
                SteamUserStats.SetStat("PerfectTimingWins", playerData.m_perfectTimingWins);

                // Set progression stats by reading from PlayerData
                SteamUserStats.SetStat("StartedFirstDuel", playerData.startedFirstDuel ? 1 : 0);
                SteamUserStats.SetStat("WonFirstDuel", playerData.wonFirstDuel ? 1 : 0);
                SteamUserStats.SetStat("CompletedEasy", playerData.completedEasyMode ? 1 : 0);
                SteamUserStats.SetStat("CompletedMedium", playerData.completedMediumMode ? 1 : 0);
                SteamUserStats.SetStat("CompletedGame", playerData.completedHardMode ? 1 : 0);
                SteamUserStats.SetStat("DefeatedFraug", playerData.defeatedFraug ? 1 : 0);

                // Check achievements
                CheckAchievements();
                
                bool bSuccess = SteamUserStats.StoreStats();
                // The flag is reset only on success
                m_bStoreStats = !bSuccess;
            }
        }
        
        #endregion
        
        #region Steam Callbacks

        private void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            if ((ulong)m_GameID != pCallback.m_nGameID || pCallback.m_eResult != EResult.k_EResultOK) return;

            Debug.Log("Received stats from Steam.");
            m_bStatsValid = true;

            // Load stats from Steam
            SteamUserStats.GetStat("TotalDuels", out playerData.m_totalDuels);
            SteamUserStats.GetStat("TotalWins", out playerData.m_totalWins);
            SteamUserStats.GetStat("TotalLosses", out playerData.m_totalLosses);
            SteamUserStats.GetStat("TotalDraws", out playerData.m_totalDraws);
            SteamUserStats.GetStat("BestWinStreak", out playerData.m_bestWinStreak);
            SteamUserStats.GetStat("TotalEarlyAttacks", out playerData.m_totalEarlyAttacks);
            SteamUserStats.GetStat("PerfectTimingWins", out playerData.m_perfectTimingWins);

            // Load achievement state from Steam so we don't re-unlock
            // already-unlocked achievements and so CheckAchievements() can skip them.
            foreach (Achievement_t ach in m_Achievements)
            {
                SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
            }
        }

        private void OnUserStatsStored(UserStatsStored_t pCallback)
        {
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (pCallback.m_eResult == EResult.k_EResultOK)
                {
                    Debug.Log("Successfully stored stats to Steam.");
                }
                else
                {
                    Debug.LogError("Failed to store stats: " + pCallback.m_eResult);
                }
            }
        }

        private void OnAchievementStored(UserAchievementStored_t pCallback)
        {
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
        }
        
        #endregion
        
        #region Achievements
        
        private class Achievement_t
        {
            public Achievement m_eAchievementID;
            public string m_strName;
            public string m_strDescription;
            public bool m_bAchieved;

            public Achievement_t(Achievement achievementID, string name, string desc)
            {
                m_eAchievementID = achievementID;
                m_strName = name;
                m_strDescription = desc;
                m_bAchieved = false;
            }
        }

        private enum Achievement : int
        {
            ACH_FIRST_VICTORY,
            ACH_PERFECT_TIMING,
            ACH_EARLY_BIRD,
            ACH_DRAW_MASTER,
            ACH_EASY_COMPLETE,
            ACH_MEDIUM_COMPLETE,
            ACH_HARD_COMPLETE,
            ACH_DEFEAT_FRAUG,
            ACH_WIN_STREAK_5,
            ACH_WIN_STREAK_10,
            ACH_LIGHTNING_FAST,
            ACH_PRECISION_MASTER,
            ACH_NEVER_GIVE_UP
        };

        private Achievement_t[] m_Achievements = new Achievement_t[]
        {
            new Achievement_t(Achievement.ACH_FIRST_VICTORY,    "First Blood",        "Win your first duel"),
            new Achievement_t(Achievement.ACH_PERFECT_TIMING,   "Perfect Timing",     "Win with exactly 1 frame after signal"),
            new Achievement_t(Achievement.ACH_EARLY_BIRD,       "Eager Samurai",      "Attack too early 10 times"),
            new Achievement_t(Achievement.ACH_DRAW_MASTER,      "Draw Master",        "Achieve 5 draws in duels"),
            new Achievement_t(Achievement.ACH_EASY_COMPLETE,    "Novice Warrior",     "Complete all Easy difficulty stages"),
            new Achievement_t(Achievement.ACH_MEDIUM_COMPLETE,  "Skilled Swordsman",  "Complete all Medium difficulty stages"),
            new Achievement_t(Achievement.ACH_HARD_COMPLETE,    "Master Samurai",     "Complete all Hard difficulty stages"),
            new Achievement_t(Achievement.ACH_DEFEAT_FRAUG,     "Frog Slayer",        "Defeat Fraug, the ultimate opponent"),
            new Achievement_t(Achievement.ACH_WIN_STREAK_5,     "Hot Streak",         "Win 5 duels in a row"),
            new Achievement_t(Achievement.ACH_WIN_STREAK_10,    "Unstoppable",        "Win 10 duels in a row"),
            new Achievement_t(Achievement.ACH_LIGHTNING_FAST,   "Lightning Fast",     "Win a duel within 3 frames of signal"),
            new Achievement_t(Achievement.ACH_PRECISION_MASTER, "Precision Master",   "Win 20 duels with perfect timing"),
            new Achievement_t(Achievement.ACH_NEVER_GIVE_UP,    "Never Give Up",      "Lose 50 duels but keep fighting")
        };

        private void CheckAchievements()
        {
            foreach (Achievement_t achievement in m_Achievements)
            {
                if (achievement.m_bAchieved) continue;

                switch (achievement.m_eAchievementID)
                {
                    case Achievement.ACH_FIRST_VICTORY:
                        if (playerData.m_totalWins >= 1)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_PERFECT_TIMING:
                        if (playerData.m_perfectTimingWins >= 1)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_EARLY_BIRD:
                        if (playerData.m_totalEarlyAttacks >= 10)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_DRAW_MASTER:
                        if (playerData.m_totalDraws >= 5)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_EASY_COMPLETE:
                        if (playerData.completedEasyMode)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_MEDIUM_COMPLETE:
                        if (playerData.reachedHardDifficulty)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_HARD_COMPLETE:
                        if (playerData.completedHardMode)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_DEFEAT_FRAUG:
                        if (playerData.defeatedFraug)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_WIN_STREAK_5:
                        if (playerData.m_maxWinStreak >= 5)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_WIN_STREAK_10:
                        if (playerData.m_maxWinStreak >= 10)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_LIGHTNING_FAST:
                        // Triggered externally via TriggerLightningFastAchievement()
                        // called from GameManager.OnDuelWon when framesAfterSignal <= 3.
                        break;

                    case Achievement.ACH_PRECISION_MASTER:
                        if (playerData.m_perfectTimingWins >= 20)
                            UnlockAchievement(achievement);
                        break;

                    case Achievement.ACH_NEVER_GIVE_UP:
                        if (playerData.m_totalLosses >= 50)
                            UnlockAchievement(achievement);
                        break;
                }
            }
        }

        public void TriggerLightningFastAchievement()
        {
            foreach (Achievement_t achievement in m_Achievements)
            {
                if (achievement.m_eAchievementID == Achievement.ACH_LIGHTNING_FAST && !achievement.m_bAchieved)
                {
                    UnlockAchievement(achievement);
                    break;
                }
            }
        }

        private void UnlockAchievement(Achievement_t achievement)
        {
            achievement.m_bAchieved = true;
            SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());
            m_bStoreStats = true;
            Debug.Log("Achievement Unlocked: " + achievement.m_strName);
        }
        
        private void ClearAchievements()
        {
            foreach (Achievement_t achievement in m_Achievements)
            {
                SteamUserStats.ClearAchievement(achievement.m_eAchievementID.ToString());
            }
        }
        
        #endregion
    }
}