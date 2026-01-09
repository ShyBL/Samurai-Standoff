using System;
using UnityEngine;

namespace SamuraiStandoff
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerData player2Data;
        [SerializeField] private GameData gameData;
        public static bool isTestMode = false;
        public bool IsMultiplayer;

        #region Singleton

        public static GameManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                Application.targetFrameRate = 30;
                QualitySettings.vSyncCount = 0;

                DontDestroyOnLoad(gameObject);
                
                if (!isTestMode)
                {
                    ValidateCharacterUnlocks();
                }

            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {

        }

        #endregion

        #region Game Mode

        public int totalLevels;
    
        public void SetTutorialMode()
        {
            totalLevels = gameData.tutorialTotalLevels; 
            gameData.currentDifficulty = EnemyDifficultyType.Tutorial;
        }
        
        public void SetEasyMode()
        {
            totalLevels = gameData.easyTotalLevels; 
            gameData.currentDifficulty = EnemyDifficultyType.EasyMode;
        }

        public void SetMediumMode()
        {
            totalLevels = gameData.mediumTotalLevels; 
            gameData.currentDifficulty = EnemyDifficultyType.MediumMode;
        }

        public void SetHardMode()
        {
            totalLevels = gameData.hardTotalLevels; 
            gameData.currentDifficulty = EnemyDifficultyType.HardMode;
        }

        public void ToggleMultiplayer(bool isMultiplayer)
        {
            gameData.isMultiplayer = isMultiplayer;
            IsMultiplayer =  gameData.isMultiplayer;
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
            playerData.m_perfectTimingWins = 0;
            playerData.m_totalEarlyAttacks = 0;
            playerData.m_currentWinStreak = 0;
            playerData.m_bestWinStreak = 0;
            playerData.m_totalDuels = 0;
            playerData.m_totalWins = 0;
            playerData.m_totalLosses = 0;
            playerData.m_totalDraws = 0;
            playerData.m_maxWinStreak = 0;
            Debug.Log("Player Progression Data has been reset.");
        }

        public void CleanUp()//Resets values for new game
        {
            playerData.faultCounter = 0;
            
            if(gameData.isMultiplayer)
            {
                player2Data.faultCounter = 0;
            }
        }
        #endregion
    
        #region Progression Control
    
        private void ValidateCharacterUnlocks()
        {
            // Ensure characters are unlocked based on actual progression
            if (playerData.completedEasyMode)
                UnlockCharacter(CharacterType.Ichi);
    
            if (playerData.completedMediumMode)
                UnlockCharacter(CharacterType.Bluetail);
    
            if (playerData.completedHardMode)
                UnlockCharacter(CharacterType.Fraug);
    
            if (playerData.m_totalLosses >= 10)
                UnlockCharacter(CharacterType.Macaroni);
    
            if (playerData.m_bestWinStreak >= 10)
                UnlockCharacter(CharacterType.Chaolin);
        }
    
        public bool IsCharacterUnlocked(CharacterType type)
        {
            return playerData.Characters[type];
        }

        public void UnlockCharacter(CharacterType type)
        {
            if (playerData.Characters.ContainsKey(type))
                playerData.Characters[type] = true;
            else
                playerData.Characters.Add(type, true);
        }
    
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

            if (playerData.m_totalLosses == 10)
            {
                UnlockCharacter(CharacterType.Macaroni);
            }
        
            if (playerData.m_bestWinStreak == 10)
            {
                UnlockCharacter(CharacterType.Chaolin);
            }
            
            // Update Steam Achievements Flag
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
        private void MarkDifficultyCompleted(string difficulty)
        {
            switch (difficulty.ToLower())
            {
                case "easy":
                    UnlockCharacter(CharacterType.Ichi); // Character Progression
                    playerData.completedEasyMode = true; // Stage Progression
                
                    playerData.reachedMediumDifficulty = true; // Analytics
                    break;
                case "medium":
                    UnlockCharacter(CharacterType.Bluetail); // Character Progression
                    playerData.completedMediumMode = true; // Stage Progression
                
                    playerData.reachedHardDifficulty = true; // Analytics
                    break;
                case "hard":
                    UnlockCharacter(CharacterType.Fraug); // Character Progression
                    playerData.completedHardMode = true; // Stage Progression
                    break;
            }
        }
    
        #endregion
        

    }
    
}