using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class SinglePlayerController : BaseGameBehaviour
    {
        #region Serialized Fields

        [Header("Player UI Elements")]
        [SerializeField] private TextMeshProUGUI faultText;
        [SerializeField] private Image playerImage;
        [SerializeField] private GameObject keyPromptObject;

        [Header("Player State")]
        [SerializeField] private bool hasPlayerAttacked;

        public Character characterData;
        public KeyCode currentKey;
        public List<KeyCode> faultKeys = new List<KeyCode>();

        #endregion

        #region References

        private DuelTimerController timerController;
        private DuelFaultController faultController;

        #endregion

        #region Unity Methods

        private void Start()
        {
            // Get controller references
            timerController = SinglePlayerDuelController.instance.GetTimerController();
            faultController = SinglePlayerDuelController.instance.GetFaultController();

            characterData = playerData.selectedCharacter;
            hasPlayerAttacked = false;

            // Initialize player visuals
            if (playerImage != null)
            {
                playerImage.sprite = characterData.sprites[0]; // Idle sprite
            }

            // Set the key based on difficulty
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
            if (timerController.signal && keyPromptObject != null && !SinglePlayerDuelController.instance.winnerDeclared)
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
            if (SinglePlayerDuelController.instance.winnerDeclared && !hasPlayerAttacked)
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
            if (gameData.attackKeys == null || gameData.attackKeys.Count == 0)
            {
                Debug.LogWarning("No attack keys defined in GameData! Defaulting to A.");
                currentKey = KeyCode.A;
                return;
            }

            switch (gameData.currentDifficulty)
            {
                case EnemyDifficultyType.EasyMode:
                case EnemyDifficultyType.Tutorial:
                    currentKey = gameData.attackKeys[0];
                    break;

                case EnemyDifficultyType.MediumMode:
                case EnemyDifficultyType.HardMode:
                    int randomIndex = UnityEngine.Random.Range(0, gameData.attackKeys.Count);
                    currentKey = gameData.attackKeys[randomIndex];
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Add fault keys (all keys except the current one)
            faultKeys.Clear();
            foreach (KeyCode key in gameData.attackKeys)
            {
                if (key != currentKey)
                {
                    faultKeys.Add(key);
                }
            }

            Debug.Log($"Player must press: {currentKey}");

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
            if (SinglePlayerDuelController.instance.winnerDeclared) return;

            Debug.Log("Player Attacked");
            hasPlayerAttacked = true;

            if (!timerController.signal) // Attacked too early
            {
                RegisterFault();
            }
            else // Valid attack
            {
                SinglePlayerDuelController.instance.DeclareWinner(gameObject);
                
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
                SinglePlayerDuelController.instance.DeclareWinner(SinglePlayerDuelController.instance.enemy, true);
            }
        }

        #endregion

        #region UI Logic

        private void UpdateFaultUI()
        {
            if (faultText != null)
            {
                faultText.enabled = playerData.faultCounter >= 1;
            }
        }

        #endregion

        #region Game Logic

        private void RegisterFault()
        {
            faultController.RegisterSinglePlayerFault();

            if (playerData.faultCounter >= 2)
            {
                SinglePlayerDuelController.instance.DeclareWinner(SinglePlayerDuelController.instance.enemy, true);
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