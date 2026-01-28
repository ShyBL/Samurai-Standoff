using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SamuraiStandoff
{
    public class DuelController : MonoBehaviour
    {
        #region Singleton

        public static DuelController instance;
        [SerializeField] private GameData gameData;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            winnerDeclared = false;
            TimerInit();
        }

        #endregion

        #region Timer Logic

        [SerializeField] private float minSignal, maxSignal;
        [SerializeField] private Slider signalSlider; // Radial slider for visual feedback
        [SerializeField] private TextMeshProUGUI framesText;
        [SerializeField] private TextMeshProUGUI timerText;

        private float _timer;
        private int _frames;
        private int _maxFramesForSlider; // Calculated based on enemy reaction time

        public bool signal;
        public float signalTime;
        public float enemyReactionTime; // Set by EnemyController
        public bool isPaused;
        
        private void Update()
        {
            if (isPaused) return;
            
            if (winnerDeclared)
            {
                signalSlider.gameObject.SetActive(false);
            }
            else
            {
                _timer += Time.deltaTime;

                if (_timer >= signalTime)
                {
                    if (!signal)
                    {
                        AudioManager.instance.PlaySound("Signal");
                        signal = true;
                        signalSlider.gameObject.SetActive(true);
                        signalSlider.value = _maxFramesForSlider; // Start at full
                    }
                }
            }

            switch (signal)
            {
                case true when !winnerDeclared:
                    _frames++;
                    signalSlider.value = _maxFramesForSlider - _frames;
                    if (_frames % 3 == 0)
                    {
                        timerText.text = (_maxFramesForSlider - _frames).ToString();
                    }

                    break;
                case true when winnerDeclared:
                    framesText.text = _frames.ToString(CultureInfo.CurrentCulture);

                    // Log best frame count for result screen
                    if (playerData.lastBestFrameCount > _frames)
                    {
                        playerData.lastBestFrameCount = _frames;

                        if (playerData.currentBestFrameCount > playerData.lastBestFrameCount)
                        {
                            playerData.currentBestFrameCount = playerData.lastBestFrameCount;
                        }

                    }

                    break;
            }
        }

        private void TimerInit()
        {
            signalTime = Random.Range(minSignal, maxSignal);
            // Zero out best time for result screen
            _frames = 0;
            //playerData.lastBestFrameCount = _frames;
        }

        // Called by EnemyController to set the max frames based on enemy reaction time
        public void SetMaxFramesForSlider()
        {
            // Convert enemy reaction time (in seconds) to frames (assuming 60 FPS)
            _maxFramesForSlider = Mathf.RoundToInt(enemyReactionTime * 60f);

            // Set the slider's max value to match
            signalSlider.maxValue = _maxFramesForSlider;
            signalSlider.minValue = 0f;
        }

        #endregion

        #region Game State

        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerData player2Data;

        public GameObject pOne, pTwo;
        public bool winnerDeclared;
        public bool playerFault;

        [Header("UI Elements")] [SerializeField]
        private TextMeshProUGUI resultText;

        private void Start()
        {
            SteamRichPresenceManager.instance.SetInDuel(
                gameData.currentDifficulty,
                playerData.currentLevel,
                gameData.currentDifficulty switch
                {
                    EnemyDifficultyType.EasyMode    => gameData.easyTotalLevels,
                    EnemyDifficultyType.MediumMode  => gameData.mediumTotalLevels,
                    EnemyDifficultyType.HardMode    => gameData.hardTotalLevels,
                    EnemyDifficultyType.Tutorial    => 1,
                    _ => throw new ArgumentOutOfRangeException()
                },
                playerData.characterType
            );

            
            signalSlider.gameObject.SetActive(false);
            signalSlider.value = 0f;
            resultText.enabled = false;

            // Assign player references
            var players = GameObject.FindGameObjectsWithTag("Player");
            pOne = players[0];
            pTwo = players[1];
        }

        #endregion

        #region Win Logic

        /// <summary>
        /// Declares the winner, records progression stats, and triggers scene transitions.
        /// A win can be triggered by a fault, which is tracked for stats.
        /// </summary>
        /// <param name="winner">The GameObject of the winner.</param>
        /// <param name="winByFault">Set to true if the win was caused by the opponent's second fault.</param>
        public void DeclareWinner(GameObject winner, bool winByFault = false)
        {
            AudioManager.instance.PlaySound("Clash");
            SceneLoader.instance.Clash();
           
                
            if (!winnerDeclared)
            {
                winnerDeclared = true;
                GameObject loser = (winner == pOne) ? pTwo : pOne;

                if(gameData.isMultiplayer == false)
                {             
                    var enemyType = FindFirstObjectByType<EnemyController>().selectedCharacter.type;

                    if (winByFault)// Fault
                    {
                        // This call handles the second, loss-inducing fault. The first is in FaultRestart.
                        GameManager.instance.OnEarlyAttack();
                    }       
            
                    if (winner.TryGetComponent(out PlayerController player)) // Player wins
                    {
                        GameManager.instance.OnDuelWon(_frames, loser.name);                 
                        RecordSinglePlayerDuel(true,enemyType, _frames);

                        CheckForDifficultyCompletionAfterWinningFinalDuel();
                    }
                    else // AI wins
                    {
                        GameManager.instance.OnDuelLost();
                    
                        RecordSinglePlayerDuel(false,enemyType, _frames);
                    }
                }
                else // Multiplayer Logic
                {    
                    // Assuming progression is tracked from the perspective of Player 1 (pOne).
                    if (winner == pOne)
                    {
                        RecordMultiplayerDuel(true,playerData.characterType,player2Data.characterType, _frames);
                    }
                    else
                    {
                        RecordMultiplayerDuel(false,playerData.characterType,player2Data.characterType, _frames);
                    }
                }

                ShowWinner(winner);
        
                if (gameData.isMultiplayer == false && winner.TryGetComponent<PlayerController>(out PlayerController winningPlayer))
                {
                    playerData.lastBestFrameCount = 10000;
                    StartCoroutine(SceneLoader.instance.NextLevel());
                }
                else
                {
                    StartCoroutine(SceneLoader.instance.LoadResults());
                }
            }
        }

        private void CheckForDifficultyCompletionAfterWinningFinalDuel()
        {
            if (playerData.currentLevel >= GameManager.instance.totalLevels)
            {
                string difficulty = "";
                switch (gameData.currentDifficulty)
                {
                    case EnemyDifficultyType.EasyMode:
                        difficulty = "easy";
                        break;
                    case EnemyDifficultyType.MediumMode:
                        difficulty = "medium";
                        break;
                    case EnemyDifficultyType.HardMode:
                        difficulty = "hard";
                        break;
                }

                if (!string.IsNullOrEmpty(difficulty))
                {
                    GameManager.instance.OnDifficultyCompleted(difficulty);
                }
            }
        }

        private void ShowWinner(GameObject winner)
        {
            if (winner.TryGetComponent(out PlayerController player))
            {
                resultText.enabled = true;
                resultText.text = $"{player.characterData.name} Wins!";
            }
            
            if (winner.TryGetComponent(out EnemyController enemy))
            {
                resultText.enabled = true;
                resultText.text = $"{enemy.selectedCharacter.name} Wins!";
            }
            
        }

        #endregion

        #region Fault Logic

        // Handles fault scenario and restarts round if needed.
        public void FaultRestart()
        {
            // Track the first fault as an early attack.
            GameManager.instance.OnEarlyAttack();

            winnerDeclared = true;
            resultText.enabled = true;
            resultText.text = "Fault";

            SceneLoader.instance.RestartDuel();
        }

        #endregion
        
        // Helper method to record single player duel result
        public void RecordSinglePlayerDuel(bool playerWon, CharacterType enemyType, int frameCount)
        {
            gameData.lastEnemyCharacterType = enemyType;
            gameData.lastDuelFrameCount = frameCount;
            gameData.winningCharacter = playerWon ? playerData.characterType : enemyType; // Adjust based on player's character
        }

        // Helper method to record multiplayer duel result
        public void RecordMultiplayerDuel(bool player1Won, CharacterType player1Char, CharacterType player2Char, int frameCount)
        {
            // Update win counters in PlayerData
            if (player1Won)
            {
                playerData.multiplayerWins++;
            }
            else
            {
                player2Data.multiplayerWins++;

            }
    
            Debug.Log($"Multiplayer duel recorded. P1 Wins: {playerData.multiplayerWins}, P2 Wins: {player2Data.multiplayerWins}");
        }
    }
}