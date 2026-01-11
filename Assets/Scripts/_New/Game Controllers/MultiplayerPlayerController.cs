using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class MultiplayerPlayerController : BaseGameBehaviour
    {
        #region Serialized Fields

        [Header("Player UI Elements")]
        [SerializeField] private TextMeshProUGUI faultText;
        [SerializeField] private Image playerImage;
        [SerializeField] private GameObject keyPromptObject;

        [Header("Player State")]
        [SerializeField] private bool hasPlayerAttacked;

        [Header("Player Assignment")]
        [SerializeField] private int playerNumber = 1; // 1 or 2

        public Character characterData;
        public KeyCode currentKey;
        public List<KeyCode> faultKeys = new List<KeyCode>();
        
        // Public property to get the correct PlayerData for this player
        public PlayerData GetPlayerData() => (playerNumber == 1) ? playerData : player2Data;

        #endregion

        #region References

        private DuelTimerController timerController;
        private DuelFaultController faultController;

        #endregion

        #region Unity Methods

        private void Start()
        {
            // Get controller references
            timerController = MultiplayerDuelController.instance.GetTimerController();
            faultController = MultiplayerDuelController.instance.GetFaultController();

            // Get the correct PlayerData based on playerNumber
            PlayerData currentPlayerData = (playerNumber == 1) ? playerData : player2Data;
            
            characterData = currentPlayerData.selectedCharacter;
            hasPlayerAttacked = false;

            // Initialize player visuals
            if (playerImage != null)
            {
                playerImage.sprite = characterData.sprites[0]; // Idle sprite
            }

            // Set the key for this player
            AssignKey();

            // Hide key prompt initially
            if (keyPromptObject != null)
            {
                keyPromptObject.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateFaultUI();

            // Show key prompt when signal appears
            if (timerController.signal && keyPromptObject != null && !MultiplayerDuelController.instance.winnerDeclared)
            {
                keyPromptObject.SetActive(true);
            }

            // Handle correct key press
            if (Input.GetKeyDown(currentKey) && !hasPlayerAttacked)
            {
                HandleCorrectKeyPress();
            }
            // Handle mispress (wrong attack key)
            else if ((Input.GetKeyDown(faultKeys[0]) || Input.GetKeyDown(faultKeys[1])) && !hasPlayerAttacked)
            {
                HandleMispress();
            }

            // Update sprite if player lost
            if (MultiplayerDuelController.instance.winnerDeclared && !hasPlayerAttacked)
            {
                if (timerController.signal && playerImage != null && playerImage.sprite != characterData.sprites[2])
                {
                    playerImage.sprite = characterData.sprites[2]; // Lose sprite
                    MovePlayerToAttackPosition();

                    if (keyPromptObject != null)
                    {
                        keyPromptObject.SetActive(false);
                    }
                }
            }
        }

        #endregion

        #region Key Assignment

        private void AssignKey()
        {
            List<KeyCode> keyPool = (playerNumber == 1) ? gameData.attackKeys : gameData.p2AttackKeys;

            if (keyPool == null || keyPool.Count == 0)
            {
                Debug.LogWarning($"No attack keys defined for Player {playerNumber}!");
                currentKey = (playerNumber == 1) ? KeyCode.A : KeyCode.J;
                return;
            }

            // Random key from the appropriate player's key pool
            int randomIndex = UnityEngine.Random.Range(0, keyPool.Count);
            currentKey = keyPool[randomIndex];

            // Add fault keys (all keys from this player's pool except the current one)
            faultKeys.Clear();
            foreach (KeyCode key in keyPool)
            {
                if (key != currentKey)
                {
                    faultKeys.Add(key);
                }
            }

            Debug.Log($"Player {playerNumber} must press: {currentKey}");

            // Update the key prompt text
            if (keyPromptObject != null)
            {
                TextMeshProUGUI promptText = keyPromptObject.GetComponentInChildren<TextMeshProUGUI>();
                if (promptText != null)
                {
                    promptText.text = currentKey.ToString();
                }
            }
        }

        #endregion

        #region Input Handling

        private void HandleCorrectKeyPress()
        {
            if (MultiplayerDuelController.instance.winnerDeclared) return;

            Debug.Log($"Player {playerNumber} Attacked");
            hasPlayerAttacked = true;

            if (!timerController.signal) // Attacked too early
            {
                RegisterFault();
            }
            else // Valid attack
            {
                MultiplayerDuelController.instance.DeclareWinner(gameObject);
                
                if (playerImage != null)
                {
                    playerImage.sprite = characterData.sprites[1]; // Win sprite
                }
                
                MovePlayerToAttackPosition();

                if (keyPromptObject != null)
                {
                    keyPromptObject.SetActive(false);
                }
            }
        }

        private void HandleMispress()
        {
            if (!timerController.signal) // Mispress before signal
            {
                RegisterFault();
            }
            else // Mispress after signal (instant loss)
            {
                hasPlayerAttacked = true;
                
                // Determine opponent
                GameObject opponent = (gameObject == MultiplayerDuelController.instance.player1)
                    ? MultiplayerDuelController.instance.player2
                    : MultiplayerDuelController.instance.player1;

                MultiplayerDuelController.instance.DeclareWinner(opponent, true);
            }
        }

        #endregion

        #region UI Logic

        private void UpdateFaultUI()
        {
            if (faultText != null)
            {
                PlayerData currentPlayerData = (playerNumber == 1) ? playerData : player2Data;
                faultText.enabled = currentPlayerData.faultCounter >= 1;
            }
        }

        #endregion

        #region Game Logic

        private void RegisterFault()
        {
            faultController.RegisterMultiplayerFault(gameObject);

            if (faultController.HasReachedFaultLimit(gameObject))
            {
                // Determine opponent
                GameObject opponent = (gameObject == MultiplayerDuelController.instance.player1)
                    ? MultiplayerDuelController.instance.player2
                    : MultiplayerDuelController.instance.player1;

                MultiplayerDuelController.instance.DeclareWinner(opponent, true);
            }
        }

        private void MovePlayerToAttackPosition()
        {
            Vector3 newPosition = transform.localPosition;
            newPosition.x = (newPosition.x >= 600) ? -600 : 600;
            transform.localPosition = newPosition;
        }

        #endregion
    }
}