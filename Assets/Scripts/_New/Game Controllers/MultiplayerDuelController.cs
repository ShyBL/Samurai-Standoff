using UnityEngine;
using TMPro;
using System.Collections;

namespace SamuraiStandoff
{
    public class MultiplayerDuelController : BaseGameBehaviour
    {
        #region Singleton

        public static MultiplayerDuelController instance;

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

        public GameObject player1;
        public GameObject player2;
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

            // Find both player references
            var players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length >= 2)
            {
                player1 = players[0];
                player2 = players[1];
            }
            else
            {
                Debug.LogError("MultiplayerDuelController: Could not find both players!");
            }
        }

        #endregion

        #region Win Logic

        /// <summary>
        /// Declares the winner in multiplayer mode
        /// </summary>
        /// <param name="winner">The GameObject of the winner (player1 or player2)</param>
        /// <param name="winByFault">Set to true if the win was caused by the opponent's second fault</param>
        public void DeclareWinner(GameObject winner, bool winByFault = false)
        {
            if (winnerDeclared) return;

            AudioManager.instance.PlaySound("Clash");
            SceneLoader.instance.Clash();

            winnerDeclared = true;
            timerController.OnWinnerDeclared();

            GameObject loser = (winner == player1) ? player2 : player1;

            if (winByFault)
            {
                // This call handles the second, loss-inducing fault
                GameManager.instance.OnEarlyAttack();
            }

            // Determine if player1 won (for tracking purposes)
            bool player1Won = (winner == player1);

            if (player1Won)
            {
                GameManager.instance.OnDuelWon(timerController.frames, loser.name);
            }
            else
            {
                GameManager.instance.OnDuelLost();
            }

            RecordDuelResult(player1Won, playerData.characterType, player2Data.characterType, timerController.frames);
            ShowWinner(winner);

            // Reset and go to results
            if (winner.TryGetComponent(out MultiplayerPlayerController _))
            {
                playerData.lastBestFrameCount = 10000;
                player2Data.lastBestFrameCount = 10000;
                StartCoroutine(SceneLoader.instance.LoadResults());
            }
        }

        private void ShowWinner(GameObject winner)
        {
            if (resultText == null) return;

            resultText.enabled = true;

            if (winner.TryGetComponent(out MultiplayerPlayerController playerController))
            {
                resultText.text = $"{playerController.characterData.name} Wins!";
            }
        }

        /// <summary>
        /// Record multiplayer duel result to GameData
        /// </summary>
        private void RecordDuelResult(bool player1Won, CharacterType player1Char, CharacterType player2Char, int frameCount)
        {
            gameData.lastDuelPlayer1Won = player1Won;
            gameData.winningCharacter = player1Won ? player1Char : player2Char;
            gameData.lastDuelFrameCount = frameCount;
        }

        #endregion

        #region Public Accessors

        public DuelTimerController GetTimerController() => timerController;
        public DuelFaultController GetFaultController() => faultController;

        #endregion
    }
}