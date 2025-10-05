using Steamworks;
using UnityEngine;

public class SamuraiStandoffStats : MonoBehaviour
{
    // Reference to the shared progression data
    public PlayerData playerData;

    // Steamworks API
    private CGameID m_GameID;
    private bool m_bRequestedStats;
    private bool m_bStatsValid;
    private bool m_bStoreStats; // Flag to trigger storing stats

    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;

    void OnEnable()
    {
        if (!SteamManager.Initialized) return;

        m_GameID = new CGameID(SteamUtils.GetAppID());
        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
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

            bool bSuccess = SteamUserStats.StoreStats();
            // The flag is reset only on success
            m_bStoreStats = !bSuccess;
        }
    }

    #region Public Methods (Called from Game Logic)

    public void OnDuelWon(int framesAfterSignal, string opponentName = "")
    {
        if (playerData == null) return;

        playerData.m_totalDuels++;
        playerData.m_totalWins++;
        playerData.m_maxWinStreak++;

        if (!playerData.wonFirstDuel)
        {
            playerData.wonFirstDuel = true;
        }

        if (playerData.m_maxWinStreak > playerData.m_bestWinStreak)
        {
            playerData.m_bestWinStreak = playerData.m_maxWinStreak;
        }

        if (framesAfterSignal == 1)
        {
            playerData.m_perfectTimingWins++;
        }

        if (opponentName.ToLower() == "fraug")
        {
            playerData.defeatedFraug = true;
        }

        m_bStoreStats = true;
    }

    public void OnDuelLost()
    {
        playerData.m_totalDuels++;
        playerData.m_totalLosses++;
        playerData.m_maxWinStreak = 0;
        m_bStoreStats = true;
    }

    public void OnDuelDraw()
    {
        playerData.m_totalDuels++;
        playerData.m_totalDraws++;
        playerData.m_maxWinStreak = 0;
        m_bStoreStats = true;
    }

    public void OnEarlyAttack()
    {
        playerData.m_totalEarlyAttacks++;
        m_bStoreStats = true;
    }

    // Call this when you complete a difficulty, which will then trigger stats to be saved.
    public void OnDifficultyCompleted(string difficulty)
    {
        if (playerData == null) return;
        playerData.MarkDifficultyCompleted(difficulty);
        m_bStoreStats = true;
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

    #endregion
}