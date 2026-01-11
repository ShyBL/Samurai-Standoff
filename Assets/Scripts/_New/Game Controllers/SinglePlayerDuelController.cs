using UnityEngine;
using TMPro;
using System.Collections;

namespace SamuraiStandoff
{
    public class SinglePlayerDuelController : BaseGameBehaviour
    {
        #region Singleton

        public static SinglePlayerDuelController instance;

        [Header("References")]
        [SerializeField] private DuelTimerController timerController;
        [SerializeField] private DuelFaultController faultController;

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
        }

        #endregion

        #region Game State

        public GameObject player;
        public GameObject enemy;
        public bool winnerDeclared { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI resultText;

        private void Start()
        {
            winnerDeclared = false;
            
            if (resultText != null)
            {
                resultText.enabled = false;
            }

            // Find player and enemy references
            player = GameObject.FindGameObjectWithTag("Player");
            enemy = GameObject.FindGameObjectWithTag("Enemy");
        }

        #endregion

        #region Win Logic

        /// <summary>
        /// Declares the winner in single player mode
        /// </summary>
        /// <param name="winner">The GameObject of the winner (player or enemy)</param>
        /// <param name="winByFault">Set to true if the win was caused by the opponent's second fault</param>
        public void DeclareWinner(GameObject winner, bool winByFault = false)
        {
            if (winnerDeclared) return;

            AudioManager.instance.PlaySound("Clash");
            SceneLoader.instance.Clash();

            winnerDeclared = true;
            timerController.OnWinnerDeclared();

            GameObject loser = (winner == player) ? enemy : player;
            var enemyType = enemy.GetComponent<EnemyController>().selectedCharacter.type;

            if (winByFault)
            {
                // This call handles the second, loss-inducing fault. The first is in FaultRestart.
                GameManager.instance.OnEarlyAttack();
            }

            if (winner.TryGetComponent(out SinglePlayerController playerController)) // Player wins
            {
                GameManager.instance.OnDuelWon(timerController.frames, loser.name);
                RecordDuelResult(true, enemyType, timerController.frames);
                CheckForDifficultyCompletionAfterWinningFinalDuel();
            }
            else // AI wins
            {
                GameManager.instance.OnDuelLost();
                RecordDuelResult(false, enemyType, timerController.frames);
            }

            ShowWinner(winner);

            if (winner.TryGetComponent(out SinglePlayerController _))
            {
                playerData.lastBestFrameCount = 10000;
                StartCoroutine(SceneLoader.instance.NextLevel());
            }
            else
            {
                StartCoroutine(SceneLoader.instance.LoadResults());
            }
        }

        private void CheckForDifficultyCompletionAfterWinningFinalDuel()
        {
            if (playerData.currentLevel >= GameManager.instance.totalLevels)
            {
                string difficulty = gameData.currentDifficulty switch
                {
                    EnemyDifficultyType.EasyMode => "easy",
                    EnemyDifficultyType.MediumMode => "medium",
                    EnemyDifficultyType.HardMode => "hard",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(difficulty))
                {
                    GameManager.instance.OnDifficultyCompleted(difficulty);
                }
            }
        }

        private void ShowWinner(GameObject winner)
        {
            if (resultText == null) return;

            resultText.enabled = true;

            if (winner.TryGetComponent(out SinglePlayerController playerController))
            {
                resultText.text = $"{playerController.characterData.name} Wins!";
            }
            else if (winner.TryGetComponent(out EnemyController enemyController))
            {
                resultText.text = $"{enemyController.selectedCharacter.name} Wins!";
            }
        }

        /// <summary>
        /// Record single player duel result to GameData
        /// </summary>
        private void RecordDuelResult(bool playerWon, CharacterType enemyType, int frameCount)
        {
            gameData.lastDuelPlayerWon = playerWon;
            gameData.lastEnemyCharacterType = enemyType;
            gameData.lastDuelFrameCount = frameCount;
            gameData.winningCharacter = playerWon ? playerData.characterType : enemyType;
        }

        #endregion

        #region Public Accessors

        public DuelTimerController GetTimerController() => timerController;
        public DuelFaultController GetFaultController() => faultController;

        #endregion
    }
}