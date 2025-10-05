using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Samurai Standoff/Player Data")]
public class PlayerData : ScriptableObject
{
    public Character selectedCharacter;
    public CharacterType characterType;
    public float lastBestFrameCount;
    [Range(0.1f, 100f)]
    public float volume;

    [Header("Difficulty Progression")]
    public bool completedEasyMode;
    public bool completedMediumMode;
    public bool completedHardMode;

    [Header("Analytics")]
    public bool startedFirstDuel; // Did player enter their first battle?
    public bool wonFirstDuel;  // Did they win their first duel?
    public bool reachedMediumDifficulty; // Finished all 4 Easy stages
    public bool reachedHardDifficulty; // Finished all 4 Medium stages  
    public bool defeatedFraug; // Finished all 5 Hard stages (full game)
    
    public int m_perfectTimingWins; // Exactly 1 frame after signal
    public int m_totalEarlyAttacks; // Attacked before signal
    public int m_currentWinStreak; // Best streak (resets on loss)
    public int m_bestWinStreak; // Best ever streak
    
    public int m_totalDuels;
    public int m_totalWins;
    public int m_totalLosses;
    public int m_totalDraws;
    public int m_maxWinStreak;
    public int m_earlyAttacks;
    
    [Header("Stage Completion Counts")]
    [Tooltip("How many stages completed in Easy (0-4)")]
    public int easyStagesCompleted;
    [Tooltip("How many stages completed in Medium (0-4)")]
    public int mediumStagesCompleted;
    [Tooltip("How many stages completed in Hard (0-5)")]
    public int hardStagesCompleted;

    private void Awake()
    {
        if (characterType != CharacterType.Monk)
        {
            characterType = CharacterType.Monk;
        }
        
        if (selectedCharacter == null)
        {
            var gameData = Resources.Load("GameData") as GameData;
            selectedCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == characterType);
        }
    }

    /// <summary>
    /// Call this method from your game logic when a difficulty is fully completed.
    /// </summary>
    public void MarkDifficultyCompleted(string difficulty)
    {
        switch (difficulty.ToLower())
        {
            case "easy":
                completedEasyMode = true; // Progression
                reachedMediumDifficulty = true; // Analytics
                break;
            case "medium":
                completedMediumMode = true; // Progression
                reachedHardDifficulty = true; // Analytics
                break;
            case "hard":
                completedHardMode = true; // Progression
                
                break;
        }
    }

    /// <summary>
    /// Resets all progression data to default values. Useful for testing.
    /// </summary>
    public void ResetProgression()
    {
        completedEasyMode = false;
        completedMediumMode = false;
        completedHardMode = false;
        startedFirstDuel = false;
        wonFirstDuel = false;
        reachedMediumDifficulty = false;
        reachedHardDifficulty = false;
        defeatedFraug = false;
        easyStagesCompleted = 0;
        mediumStagesCompleted = 0;
        hardStagesCompleted = 0;
        Debug.Log("Player Progression Data has been reset.");
    }
}