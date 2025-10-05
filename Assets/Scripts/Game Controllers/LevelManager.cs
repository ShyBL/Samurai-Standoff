using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public EnemyDifficultyType currentDifficulty;
    public int totalLevels;
    public int currentLevel = 1;
    public bool isMultiplayer;
    
    #region Singleton

    public static LevelManager instance;

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
}