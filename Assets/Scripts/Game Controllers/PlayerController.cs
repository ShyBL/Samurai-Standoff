using System;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SamuraiStandoff
{
    public class PlayerController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Player UI Elements")] [SerializeField]
        private TextMeshProUGUI faultText;

        [SerializeField] private Image playerImage;
        [SerializeField] private GameObject keyPromptObject; // The UI object showing which key to press

        [Header("Player State")] [SerializeField]
        private bool hasPlayerAttacked;

        [SerializeField] public PlayerData playerData;
        [SerializeField] private GameData gameData;

        public Character characterData;
        public KeyCode currentKey; // The key the player needs to press this round

        public List<KeyCode> faultKeys = new List<KeyCode>();
        
        #endregion

        #region Unity Methods

        private void Awake()
        {
        }

        private void Start()
        {
            characterData = playerData.selectedCharacter;

            hasPlayerAttacked = false;

            // Initialize player visuals
            playerImage.sprite = characterData.sprites[0]; // Idle sprite

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
            if (DuelController.instance.signal && keyPromptObject != null && !DuelController.instance.winnerDeclared)
            {
                keyPromptObject.SetActive(true);
            }

            if (Input.GetKeyDown(currentKey) && !hasPlayerAttacked)
            {
                if (!DuelController.instance.winnerDeclared)
                {
                    Debug.Log("Player Attacked");
                    hasPlayerAttacked = true;

                    if (!DuelController.instance.signal)
                    {
                        RegisterFault();
                    }
                    else
                    {
                        DuelController.instance.DeclareWinner(gameObject);
                        playerImage.sprite = characterData.sprites[1]; // Win sprite
                        MovePlayerToAttackPosition();

                        // Hide key prompt after attack
                        if (keyPromptObject != null)
                        {
                            keyPromptObject.SetActive(false);
                        }
                    }
                }
            }
            //mis-press occurs
            else if(Input.GetKeyDown(faultKeys[0]) || Input.GetKeyDown(faultKeys[1]) && !hasPlayerAttacked)
            {
                if (DuelController.instance.signal)
                {
                    if (DuelController.instance.pOne == gameObject)
                    {
                        // The second parameter 'true' indicates this win was caused by a fault.
                        DuelController.instance.DeclareWinner(DuelController.instance.pTwo, true);
                    }
                    else if (DuelController.instance.pTwo == gameObject)
                    {
                        // The second parameter 'true' indicates this win was caused by a fault.
                        DuelController.instance.DeclareWinner(DuelController.instance.pOne, true);
                    }
                }
                else
                {
                    RegisterFault();
                }
                

            }
        
            if (DuelController.instance.winnerDeclared && !hasPlayerAttacked)
            {
                if(!DuelController.instance.signal)
                {
                    return;
                }
                else
                {
                    if(playerImage.sprite != characterData.sprites[2])
                    {
                        playerImage.sprite = characterData.sprites[2]; // Lose sprite
                        MovePlayerToAttackPosition();

                        // Hide key prompt when round ends
                        if (keyPromptObject != null)
                        {
                            keyPromptObject.SetActive(false);
                        }
                    }
                }
                
            }
        }

        #endregion

        #region Key Assignment

        private void AssignKey()
        {
            // Multiplayer mode - both players get random keys from their respective lists
            if (gameData.isMultiplayer)
            {
                if (playerData.playerNumber == 1)
                {
                    if (gameData.attackKeys == null || gameData.attackKeys.Count == 0)
                    {
                        Debug.LogWarning("No attack keys defined in GameData! Defaulting to A.");
                        currentKey = KeyCode.A;
                        return;
                    }

                    int randomIndex = UnityEngine.Random.Range(0, gameData.attackKeys.Count);
                    currentKey = gameData.attackKeys[randomIndex];

                    // Add fault keys
                    faultKeys.Clear();
                    foreach (KeyCode key in gameData.attackKeys)
                    {
                        if (key != currentKey)
                        {
                            faultKeys.Add(key);
                        }
                    }
                    Debug.Log($"Player 1 (Multiplayer) must press: {currentKey}");
                }
                else if (playerData.playerNumber == 2)
                {
                    if (gameData.p2AttackKeys == null || gameData.p2AttackKeys.Count == 0)
                    {
                        Debug.LogWarning("No P2 attack keys defined in GameData! Defaulting to J.");
                        currentKey = KeyCode.J;
                        return;
                    }

                    int randomIndex = UnityEngine.Random.Range(0, gameData.p2AttackKeys.Count);
                    currentKey = gameData.p2AttackKeys[randomIndex];

                    // Add fault keys
                    faultKeys.Clear();
                    foreach (KeyCode key in gameData.p2AttackKeys)
                    {
                        if (key != currentKey)
                        {
                            faultKeys.Add(key);
                        }
                    }
                    Debug.Log($"Player 2 (Multiplayer) must press: {currentKey}");
                }
            }
            // Single player mode - only Player 1, difficulty-based key assignment
            else
            {
                if (gameData.attackKeys == null || gameData.attackKeys.Count == 0)
                {
                    Debug.LogWarning("No attack keys defined in GameData! Defaulting to A.");
                    currentKey = KeyCode.A;
                    return;
                }

                switch (gameData.currentDifficulty)
                {
                    case EnemyDifficultyType.Tutorial:
                    case EnemyDifficultyType.EasyMode:
                        // Tutorial and Easy: always use first key
                        currentKey = gameData.attackKeys[0];
                        break;

                    case EnemyDifficultyType.MediumMode:
                    case EnemyDifficultyType.HardMode:
                        // Medium and Hard: random key
                        int randomIndex = UnityEngine.Random.Range(0, gameData.attackKeys.Count);
                        currentKey = gameData.attackKeys[randomIndex];
                        break;

                    default:
                        Debug.LogWarning($"Unhandled difficulty: {gameData.currentDifficulty}. Defaulting to first key.");
                        currentKey = gameData.attackKeys[0];
                        break;
                }

                // Add fault keys
                faultKeys.Clear();
                foreach (KeyCode key in gameData.attackKeys)
                {
                    if (key != currentKey)
                    {
                        faultKeys.Add(key);
                    }
                }
                Debug.Log($"Player (Single Player - {gameData.currentDifficulty}) must press: {currentKey}");
            }

            // Update the key prompt text if it has a TextMeshProUGUI component
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

        #region UI Logic

        private void UpdateFaultUI()
        {
            faultText.enabled = playerData.faultCounter >= 1;
        }

        #endregion

        #region Game Logic

        // Handles fault registration and win condition logic.
        private void RegisterFault()
        {
            playerData.faultCounter++;
            DuelController.instance.playerFault = true;

            if (playerData.faultCounter < 2)
            {
                DuelController.instance.FaultRestart();
            }
            else // Determine which player is at fault and declare the other as winner
            {
                if (DuelController.instance.pOne == gameObject)
                {
                    // The second parameter 'true' indicates this win was caused by a fault.
                    DuelController.instance.DeclareWinner(DuelController.instance.pTwo, true);
                }
                else if (DuelController.instance.pTwo == gameObject)
                {
                    // The second parameter 'true' indicates this win was caused by a fault.
                    DuelController.instance.DeclareWinner(DuelController.instance.pOne, true);
                }
            }
        }

        private void MovePlayerToAttackPosition()
        {
            
            Vector3 newPosition = transform.localPosition;
            if (newPosition.x >= 600)
            {
                newPosition.x = -600;
            }
            else
            {
                newPosition.x = 600;
            }
            
            transform.localPosition = newPosition;
        }

        #endregion
    }
}