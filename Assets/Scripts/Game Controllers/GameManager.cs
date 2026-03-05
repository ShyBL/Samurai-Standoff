using System;
using System.Collections.Generic;
using System.Linq;
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

                // Load save file first so all SO fields are populated before ValidateCharacterUnlocks reads them
                SaveSystem.instance.Load();

                ValidateCharacterUnlocks();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            playerData.currentLevel = 1;
            CleanUp();
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
            IsMultiplayer = gameData.isMultiplayer;
        }

        #endregion

        #region Application Control

        public void OnApplicationQuit()
        {
            Application.Quit();
        }

        // Unity calls this automatically when the app closes.
        // Belt-and-suspenders save in case we missed a spot.
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveSystem.instance.Save();
        }

        #endregion

        #region Player Data Control

        /// <summary>
        /// Resets all progression data to default values and deletes the save file.
        /// </summary>
        public void ResetProgression()
        {
            playerData.completedEasyMode      = false;
            playerData.completedMediumMode    = false;
            playerData.completedHardMode      = false;
            playerData.startedFirstDuel       = false;
            playerData.wonFirstDuel           = false;
            playerData.reachedMediumDifficulty = false;
            playerData.reachedHardDifficulty  = false;
            playerData.defeatedFraug          = false;
            playerData.m_perfectTimingWins    = 0;
            playerData.m_totalEarlyAttacks    = 0;
            playerData.m_currentWinStreak     = 0;
            playerData.m_bestWinStreak        = 0;
            playerData.m_totalDuels           = 0;
            playerData.m_totalWins            = 0;
            playerData.m_totalLosses          = 0;
            playerData.m_totalDraws           = 0;
            playerData.m_maxWinStreak         = 0;

            SaveSystem.instance.DeleteSave();
            Debug.Log("Player Progression Data has been reset.");
        }

        public void CleanUp() // Resets transient values for a new duel
        {
            playerData.faultCounter = 0;

            if (gameData.isMultiplayer)
            {
                player2Data.faultCounter = 0;
            }
        }

        #endregion

        #region Progression Control

        private void ValidateCharacterUnlocks()
        {
            if (playerData.characterType != CharacterType.Monk)
            {
                playerData.characterType = CharacterType.Monk;
            }

            if (playerData.selectedCharacter.sprites.Count == 0)
            {
                if (gameData != null && gameData.allCharacters.Count != 0)
                {
                    playerData.selectedCharacter =
                        gameData.allCharacters.FirstOrDefault(c => c.type == playerData.characterType);
                }
            }

            if (gameData.isDemo)
            {
                playerData.Characters = new Dictionary<CharacterType, bool>()
                {
                    { CharacterType.Monk,     true },
                    { CharacterType.Ichi,     true },
                    { CharacterType.Bluetail, true },
                    { CharacterType.Macaroni, true },
                    { CharacterType.Chaolin,  true },
                    { CharacterType.Fraug,    true }
                };
            }
            else
            {
                playerData.Characters = new Dictionary<CharacterType, bool>()
                {
                    { CharacterType.Monk,     true  },
                    { CharacterType.Ichi,     false },
                    { CharacterType.Bluetail, false },
                    { CharacterType.Macaroni, false },
                    { CharacterType.Chaolin,  false },
                    { CharacterType.Fraug,    false }
                };
            }

            // Re-apply unlocks based on loaded progression
            if (playerData.completedEasyMode)   UnlockCharacter(CharacterType.Ichi);
            if (playerData.completedMediumMode) UnlockCharacter(CharacterType.Bluetail);
            if (playerData.completedHardMode)   UnlockCharacter(CharacterType.Fraug);
            if (playerData.m_totalLosses >= 10) UnlockCharacter(CharacterType.Macaroni);
            if (playerData.m_bestWinStreak >= 10) UnlockCharacter(CharacterType.Chaolin);
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
                playerData.wonFirstDuel = true;

            if (playerData.m_maxWinStreak > playerData.m_bestWinStreak)
                playerData.m_bestWinStreak = playerData.m_maxWinStreak;

            if (framesAfterSignal == 1)
                playerData.m_perfectTimingWins++;

            if (opponentName.ToLower() == "fraug")
                playerData.defeatedFraug = true;

            if (playerData.m_totalLosses == 10)
                UnlockCharacter(CharacterType.Macaroni);

            if (playerData.m_bestWinStreak == 10)
                UnlockCharacter(CharacterType.Chaolin);

            SamuraiStandoffStats.instance.m_bStoreStats = true;

            SaveSystem.instance.Save();
        }

        public void OnDuelLost()
        {
            if (playerData == null) return;

            playerData.m_totalDuels++;
            playerData.m_totalLosses++;
            playerData.m_maxWinStreak = 0;

            SamuraiStandoffStats.instance.m_bStoreStats = true;

            SaveSystem.instance.Save();
        }

        public void OnDuelDraw()
        {
            if (playerData == null) return;

            playerData.m_totalDuels++;
            playerData.m_totalDraws++;
            playerData.m_maxWinStreak = 0;

            SamuraiStandoffStats.instance.m_bStoreStats = true;

            SaveSystem.instance.Save();
        }

        public void OnEarlyAttack()
        {
            if (playerData == null) return;

            playerData.m_totalEarlyAttacks++;

            SamuraiStandoffStats.instance.m_bStoreStats = true;
            
            SaveSystem.instance.Save();
        }

        public void OnDifficultyCompleted(string difficulty)
        {
            if (playerData == null) return;

            MarkDifficultyCompleted(difficulty);

            SamuraiStandoffStats.instance.m_bStoreStats = true;

            SaveSystem.instance.Save();
        }

        private void MarkDifficultyCompleted(string difficulty)
        {
            switch (difficulty.ToLower())
            {
                case "easy":
                    UnlockCharacter(CharacterType.Ichi);
                    playerData.completedEasyMode      = true;
                    playerData.reachedMediumDifficulty = true;
                    break;
                case "medium":
                    UnlockCharacter(CharacterType.Bluetail);
                    playerData.completedMediumMode   = true;
                    playerData.reachedHardDifficulty = true;
                    break;
                case "hard":
                    UnlockCharacter(CharacterType.Fraug);
                    playerData.completedHardMode = true;
                    break;
            }
        }

        #endregion
    }
}