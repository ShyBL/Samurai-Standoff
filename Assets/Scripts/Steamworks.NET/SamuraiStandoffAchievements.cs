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
    
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;
    
    // Our GameID
    private CGameID m_GameID;

    // Steam stats validation
    private bool m_bRequestedStats;
    private bool m_bStatsValid;
    private bool m_bStoreStats;

    // Reference to the shared progression data
    public PlayerData playerData;

    void OnEnable() {
        if (!SteamManager.Initialized)
            return;

        // Cache the GameID for use in the Callbacks
        m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        // Reset stats validation flags
        m_bRequestedStats = false;
        m_bStatsValid = false;
    }
    
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
    }

    private void CheckAchievements() {
        foreach (Achievement_t achievement in m_Achievements) {
            if (achievement.m_bAchieved)
                continue;

            switch (achievement.m_eAchievementID) {
                case Achievement.ACH_FIRST_VICTORY:
                    if (playerData.m_totalWins >= 1) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_PERFECT_TIMING:
                    if (playerData.m_perfectTimingWins >= 1) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_EARLY_BIRD:
                    if (playerData.m_earlyAttacks >= 10) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_DRAW_MASTER:
                    if (playerData.m_totalDraws >= 5) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_EASY_COMPLETE:
                    if (playerData.reachedMediumDifficulty) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_MEDIUM_COMPLETE:
                    if (playerData.reachedHardDifficulty) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_HARD_COMPLETE:
                    if (playerData.defeatedFraug) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_DEFEAT_FRAUG:
                    if (playerData.defeatedFraug) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_WIN_STREAK_5:
                    if (playerData.m_maxWinStreak >= 5) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_WIN_STREAK_10:
                    if (playerData.m_maxWinStreak >= 10) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_LIGHTNING_FAST:
                    // This will be triggered externally when a fast win occurs
                    break;
                case Achievement.ACH_PRECISION_MASTER:
                    if (playerData.m_perfectTimingWins >= 20) {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_NEVER_GIVE_UP:
                    if (playerData.m_totalLosses >= 50) {
                        UnlockAchievement(achievement);
                    }
                    break;
            }
        }
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

    private void OnAchievementStored(UserAchievementStored_t pCallback) {
        if ((ulong)m_GameID == pCallback.m_nGameID) {
            Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
        }
    }
    
}