using UnityEngine;
using System.Collections;
using Steamworks;

public class SamuraiStandoffAchievements : MonoBehaviour {

    private class Achievement_t {
        public Achievement m_eAchievementID;
        public string m_strName;
        public string m_strDescription;
        public bool m_bAchieved;

        public Achievement_t(Achievement achievementID, string name, string desc) {
            m_eAchievementID = achievementID;
            m_strName = name;
            m_strDescription = desc;
            m_bAchieved = false;
        }
    }
    private enum Achievement : int {
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

    private Achievement_t[] m_Achievements = new Achievement_t[] {
        new Achievement_t(Achievement.ACH_FIRST_VICTORY, "First Blood", "Win your first duel"),
        new Achievement_t(Achievement.ACH_PERFECT_TIMING, "Perfect Timing", "Win with exactly 1 frame after signal"),
        new Achievement_t(Achievement.ACH_EARLY_BIRD, "Eager Samurai", "Attack too early 10 times"),
        new Achievement_t(Achievement.ACH_DRAW_MASTER, "Draw Master", "Achieve 5 draws in duels"),
        new Achievement_t(Achievement.ACH_EASY_COMPLETE, "Novice Warrior", "Complete all Easy difficulty stages"),
        new Achievement_t(Achievement.ACH_MEDIUM_COMPLETE, "Skilled Swordsman", "Complete all Medium difficulty stages"),
        new Achievement_t(Achievement.ACH_HARD_COMPLETE, "Master Samurai", "Complete all Hard difficulty stages"),
        new Achievement_t(Achievement.ACH_DEFEAT_FRAUG, "Frog Slayer", "Defeat Fraug, the ultimate opponent"),
        new Achievement_t(Achievement.ACH_WIN_STREAK_5, "Hot Streak", "Win 5 duels in a row"),
        new Achievement_t(Achievement.ACH_WIN_STREAK_10, "Unstoppable", "Win 10 duels in a row"),
        new Achievement_t(Achievement.ACH_LIGHTNING_FAST, "Lightning Fast", "Win a duel within 3 frames of signal"),
        new Achievement_t(Achievement.ACH_PRECISION_MASTER, "Precision Master", "Win 20 duels with perfect timing"),
        new Achievement_t(Achievement.ACH_NEVER_GIVE_UP, "Never Give Up", "Lose 50 duels but keep fighting")
    };

    // Our GameID
    private CGameID m_GameID;

    // Steam stats validation
    private bool m_bRequestedStats;
    private bool m_bStatsValid;
    private bool m_bStoreStats;

    // Game stats tracking
    private int m_totalDuels;
    private int m_totalWins;
    private int m_totalLosses;
    private int m_totalDraws;
    private int m_maxWinStreak;
    private int m_earlyAttacks;
    private bool m_easyCompleted;
    private bool m_mediumCompleted;
    private bool m_hardCompleted;
    private bool m_fraugDefeated;

    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;

    void OnEnable() {
        if (!SteamManager.Initialized)
            return;

        // Cache the GameID for use in the Callbacks
        m_GameID = new CGameID(SteamUtils.GetAppID());

        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        // Reset stats validation flags
        m_bRequestedStats = false;
        m_bStatsValid = false;
    }

    // Progression & Engagement Stats (Analytics-focused)
    private bool m_startedFirstDuel;           // Did player enter their first battle?
    private bool m_wonFirstDuel;               // Did they win their first duel?
    private bool m_completedEasyMode;          // Finished all 4 Easy stages
    private bool m_completedMediumMode;        // Finished all 4 Medium stages  
    private bool m_completedHardMode;          // Finished all 5 Hard stages (full game)
    private bool m_reachedMedium;              // Made it to Medium difficulty
    private bool m_reachedHard;                // Made it to Hard difficulty
    private bool m_defeatedFraug;              // Beat the final boss
    
    // Stage-specific progression (pinpoint where players quit)
    private int m_easyStagesCompleted;         // 0-4 (Rat, Monk, ???, Macaroni)
    private int m_mediumStagesCompleted;       // 0-4 (Rat, Monk, Macaroni, Badger)
    private int m_hardStagesCompleted;         // 0-5 (Rat, Monk, Macaroni, Fox, Fraug)
    
    // Skill & Engagement Metrics
    private int m_perfectTimingWins;           // Exactly 1 frame after signal
    private int m_totalEarlyAttacks;           // Attacked before signal
    private int m_currentWinStreak;            // Current streak (resets on loss)
    private int m_bestWinStreak;               // Best ever streak
    
    private void Update() {
        if (!SteamManager.Initialized)
            return;

        if (!m_bRequestedStats)
        {
            SteamAPICall_t call = SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
            m_bRequestedStats = true;
            Debug.Log("Requesting stats from Steam...");
        }

        if (!m_bStatsValid)
            return;

        // Check achievements
        CheckAchievements();

        // Store stats if needed
        if (m_bStoreStats) {
            SteamUserStats.SetStat("StartedFirstDuel", m_startedFirstDuel ? 1 : 0);
            SteamUserStats.SetStat("WonFirstDuel", m_wonFirstDuel ? 1 : 0);
            SteamUserStats.SetStat("ReachedMedium", m_reachedMedium ? 1 : 0);
            SteamUserStats.SetStat("ReachedHard", m_reachedHard ? 1 : 0);
            SteamUserStats.SetStat("CompletedEasy", m_completedEasyMode ? 1 : 0);
            SteamUserStats.SetStat("CompletedMedium", m_completedMediumMode ? 1 : 0);
            SteamUserStats.SetStat("CompletedGame", m_completedHardMode ? 1 : 0);
            SteamUserStats.SetStat("DefeatedFraug", m_defeatedFraug ? 1 : 0);

            bool bSuccess = SteamUserStats.StoreStats();
            m_bStoreStats = !bSuccess;
        }
    }

    private void CheckAchievements() {
        foreach (Achievement_t achievement in m_Achievements) {
            if (achievement.m_bAchieved)
                continue;

            switch (achievement.m_eAchievementID) {
                case Achievement.ACH_FIRST_VICTORY:
                    if (m_totalWins >= 1) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_PERFECT_TIMING:
                    if (m_perfectTimingWins >= 1) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_EARLY_BIRD:
                    if (m_earlyAttacks >= 10) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_DRAW_MASTER:
                    if (m_totalDraws >= 5) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_EASY_COMPLETE:
                    if (m_easyCompleted) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_MEDIUM_COMPLETE:
                    if (m_mediumCompleted) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_HARD_COMPLETE:
                    if (m_hardCompleted) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_DEFEAT_FRAUG:
                    if (m_fraugDefeated) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_WIN_STREAK_5:
                    if (m_maxWinStreak >= 5) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_WIN_STREAK_10:
                    if (m_maxWinStreak >= 10) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_LIGHTNING_FAST:
                    // This will be triggered externally when a fast win occurs
                    break;
                case Achievement.ACH_PRECISION_MASTER:
                    if (m_perfectTimingWins >= 20) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_NEVER_GIVE_UP:
                    if (m_totalLosses >= 50) {
                        UnlockAchievement(achievement);
                    }
                    break;
            }
        }
    }

    // Public methods to be called from your game logic
    public void OnDuelWon(int framesAfterSignal, string opponentName = "") {
        m_totalDuels++;
        m_totalWins++;
        m_currentWinStreak++;
        
        if (m_currentWinStreak > m_maxWinStreak) {
            m_maxWinStreak = m_currentWinStreak;
        }

        // Check for perfect timing (exactly 1 frame)
        if (framesAfterSignal == 1) {
            m_perfectTimingWins++;
        }

        // Check if Fraug was defeated
        if (opponentName.ToLower() == "fraug") {
            m_fraugDefeated = true;
        }

        m_bStoreStats = true;
    }

    public void OnDuelLost() {
        m_totalDuels++;
        m_totalLosses++;
        m_currentWinStreak = 0;
        m_bStoreStats = true;
    }

    public void OnDuelDraw() {
        m_totalDuels++;
        m_totalDraws++;
        m_currentWinStreak = 0;
        m_bStoreStats = true;
    }

    public void OnEarlyAttack() {
        m_earlyAttacks++;
        m_bStoreStats = true;
    }

    public void OnDifficultyCompleted(string difficulty) {
        switch (difficulty.ToLower()) {
            case "easy":
                m_easyCompleted = true;
                break;
            case "medium":
                m_mediumCompleted = true;
                break;
            case "hard":
                m_hardCompleted = true;
                break;
        }
        m_bStoreStats = true;
    }

    private void TriggerLightningFastAchievement() {
        foreach (Achievement_t achievement in m_Achievements) {
            if (achievement.m_eAchievementID == Achievement.ACH_LIGHTNING_FAST && !achievement.m_bAchieved) {
                UnlockAchievement(achievement);
                break;
            }
        }
    }

    private void UnlockAchievement(Achievement_t achievement) {
        achievement.m_bAchieved = true;
        SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());
        m_bStoreStats = true;
        
        Debug.Log("Achievement Unlocked: " + achievement.m_strName);
    }

    private void OnUserStatsReceived(UserStatsReceived_t pCallback) {
        if (!SteamManager.Initialized)
            return;

        if ((ulong)m_GameID == pCallback.m_nGameID) {
            if (EResult.k_EResultOK == pCallback.m_eResult) {
                Debug.Log("Received stats and achievements from Steam");
                m_bStatsValid = true;

                // Load achievements
                foreach (Achievement_t ach in m_Achievements) {
                    bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                    if (ret) {
                        ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                        ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                    }
                    else {
                        Debug.LogWarning("Failed to get achievement: " + ach.m_eAchievementID);
                    }
                }

                // Load stats
                SteamUserStats.GetStat("TotalDuels", out m_totalDuels);
                SteamUserStats.GetStat("TotalWins", out m_totalWins);
                SteamUserStats.GetStat("TotalLosses", out m_totalLosses);
                SteamUserStats.GetStat("TotalDraws", out m_totalDraws);
                SteamUserStats.GetStat("MaxWinStreak", out m_maxWinStreak);
                SteamUserStats.GetStat("EarlyAttacks", out m_earlyAttacks);
                SteamUserStats.GetStat("PerfectTimingWins", out m_perfectTimingWins);
            }
            else {
                Debug.Log("RequestStats failed: " + pCallback.m_eResult);
            }
        }
    }

    private void OnUserStatsStored(UserStatsStored_t pCallback) {
        if ((ulong)m_GameID == pCallback.m_nGameID) {
            if (EResult.k_EResultOK == pCallback.m_eResult) {
                Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult) {
                Debug.Log("StoreStats - some failed to validate");
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)m_GameID;
                OnUserStatsReceived(callback);
            }
            else {
                Debug.Log("StoreStats failed: " + pCallback.m_eResult);
            }
        }
    }

    private void OnAchievementStored(UserAchievementStored_t pCallback) {
        if ((ulong)m_GameID == pCallback.m_nGameID) {
            Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
        }
    }

    
}