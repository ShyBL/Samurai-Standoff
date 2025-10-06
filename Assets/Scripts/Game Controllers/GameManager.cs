using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    public int totalLevels;
    public int currentLevel = 1;
    public bool isMultiplayer;
    public EnemyDifficultyType currentDifficulty;

    #region Singleton

    public static GameManager instance;

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

    #region Game Mode

    public void SetEasyMode()
    {
        totalLevels = 3;
        currentDifficulty = EnemyDifficultyType.EasyMode;
    }

    public void SetMediumMode()
    {
        totalLevels = 3;
        currentDifficulty = EnemyDifficultyType.MediumMode;
    }

    public void SetHardMode()
    {
        totalLevels = 4;
        currentDifficulty = EnemyDifficultyType.HardMode;
    }
    
    // Toggles multiplayer mode on or off.
    public void ToggleMultiplayer()
    {
        isMultiplayer = !isMultiplayer;
    }

    #endregion

    #region Application Control

    public void OnApplicationQuit()
    {
        // Optional cleanup logic
        Application.Quit();
    }

    #endregion
    
    #region Player Data Control // TODO: ADD HERE EVERY VARIABLE ADDED IN PLAYER DATA
    
    /// <summary>
    /// Resets all progression data to default values. Useful for testing.
    /// </summary>
    public void ResetProgression()
    {
        playerData.completedEasyMode = false;
        playerData.completedMediumMode = false;
        playerData.completedHardMode = false;
        playerData.startedFirstDuel = false;
        playerData.wonFirstDuel = false;
        playerData.reachedMediumDifficulty = false;
        playerData.reachedHardDifficulty = false;
        playerData.defeatedFraug = false;
        playerData.easyStagesCompleted = 0;
        playerData.mediumStagesCompleted = 0;
        playerData.hardStagesCompleted = 0;
        Debug.Log("Player Progression Data has been reset.");
    }
    
    #endregion
    
    #region Progression Control
    
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

        if (opponentName.ToLower() == "Fraug")
        {
            playerData.defeatedFraug = true;
        }

      //  SamuraiStandoffStats.instance.m_bStoreStats = true;
    }

    public void OnDuelLost()
    {
        if (playerData == null) return;

        playerData.m_totalDuels++;
        playerData.m_totalLosses++;
        playerData.m_maxWinStreak = 0;
     //   SamuraiStandoffStats.instance.m_bStoreStats = true;
    }

    public void OnDuelDraw()
    {
        if (playerData == null) return;

        playerData.m_totalDuels++;
        playerData.m_totalDraws++;
        playerData.m_maxWinStreak = 0;
      //  SamuraiStandoffStats.instance.m_bStoreStats = true;
    }

    public void OnEarlyAttack()
    {
        if (playerData == null) return;
        
        playerData.m_totalEarlyAttacks++;
       // SamuraiStandoffStats.instance.m_bStoreStats = true;
    }

    // Call this when you complete a difficulty, which will then trigger stats to be saved.
    public void OnDifficultyCompleted(string difficulty)
    {
        if (playerData == null) return;
        
        MarkDifficultyCompleted(difficulty);
     //   SamuraiStandoffStats.instance.m_bStoreStats = true;
    }

    
    /// <summary>
    /// Call this method from your game logic when a difficulty is fully completed.
    /// </summary>
    public void MarkDifficultyCompleted(string difficulty)
    {
        switch (difficulty.ToLower())
        {
            case "easy":
                playerData.completedEasyMode = true; // Progression
                playerData.reachedMediumDifficulty = true; // Analytics
                break;
            case "medium":
                playerData.completedMediumMode = true; // Progression
                playerData.reachedHardDifficulty = true; // Analytics
                break;
            case "hard":
                playerData.completedHardMode = true; // Progression
                break;
        }
    }
    
    #endregion
}