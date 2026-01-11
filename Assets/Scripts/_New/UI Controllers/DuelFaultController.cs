using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SamuraiStandoff
{
    public class DuelFaultController : BaseGameBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI resultText;

        public bool playerFault { get; private set; }

        /// <summary>
        /// Register a fault for single player mode
        /// </summary>
        public void RegisterSinglePlayerFault()
        {
            playerData.faultCounter++;
            playerFault = true;

            if (playerData.faultCounter < 2)
            {
                StartCoroutine(FaultRestart());
            }
            // If faultCounter >= 2, the calling code should handle declaring the enemy as winner
        }

        /// <summary>
        /// Register a fault for a specific player in multiplayer mode
        /// </summary>
        /// <param name="faultingPlayer">The player who committed the fault</param>
        public void RegisterMultiplayerFault(GameObject faultingPlayer)
        {
            PlayerData faultingPlayerData = GetPlayerData(faultingPlayer);
            
            if (faultingPlayerData != null)
            {
                faultingPlayerData.faultCounter++;
                playerFault = true;

                if (faultingPlayerData.faultCounter < 2)
                {
                    StartCoroutine(FaultRestart());
                }
                // If faultCounter >= 2, the calling code should handle declaring the other player as winner
            }
        }

        /// <summary>
        /// Get the appropriate PlayerData based on the GameObject
        /// </summary>
        private PlayerData GetPlayerData(GameObject player)
        {
            var playerController = player.GetComponent<MultiplayerPlayerController>();
            if (playerController != null)
            {
                return playerController.GetPlayerData();
            }
            return null;
        }

        /// <summary>
        /// Handles fault scenario and restarts round after delay
        /// </summary>
        private IEnumerator FaultRestart()
        {
            // Track the first fault as an early attack
            GameManager.instance.OnEarlyAttack();

            if (resultText != null)
            {
                resultText.enabled = true;
                resultText.text = "Fault";
            }

            yield return new WaitForSeconds(3f);
            RestartRound();
        }

        /// <summary>
        /// Reloads the current scene to restart the round
        /// </summary>
        private void RestartRound()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Reset fault state (called when starting a new duel)
        /// </summary>
        public void ResetFaultState()
        {
            playerFault = false;
        }

        /// <summary>
        /// Check if a player has reached the fault limit (2 faults)
        /// </summary>
        public bool HasReachedFaultLimit(GameObject player)
        {
            if (gameData.isMultiplayer)
            {
                PlayerData data = GetPlayerData(player);
                return data != null && data.faultCounter >= 2;
            }
            else
            {
                return playerData.faultCounter >= 2;
            }
        }
    }
}