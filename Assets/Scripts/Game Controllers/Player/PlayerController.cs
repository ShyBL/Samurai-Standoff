using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Player UI Elements")] 
    [SerializeField] private TextMeshProUGUI faultText;
    [SerializeField] private Image playerImage;
    [SerializeField] private GameObject keyPromptObject; // The UI object showing which key to press

    [Header("Player State")] 
    [SerializeField] private bool hasPlayerAttacked;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameData gameData;

    private Character _characterData;
    private KeyCode _currentKey; // The key the player needs to press this round
    #endregion

    #region Unity Methods

    private void Awake()
    {
        _characterData = playerData.selectedCharacter;
    }

    private void Start()
    {
        hasPlayerAttacked = false;

        // Initialize player visuals
        playerImage.sprite = _characterData.sprites[0]; // Idle sprite
        
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

        if (Input.GetKeyDown(_currentKey) && !hasPlayerAttacked)
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
                    playerImage.sprite = _characterData.sprites[1]; // Win sprite
                    MovePlayerToAttackPosition();
                    
                    // Hide key prompt after attack
                    if (keyPromptObject != null)
                    {
                        keyPromptObject.SetActive(false);
                    }
                }
            }
        }

        if (DuelController.instance.winnerDeclared && !hasPlayerAttacked)
        {
            playerImage.sprite = _characterData.sprites[2]; // Lose sprite
            MovePlayerToAttackPosition();
            
            // Hide key prompt when round ends
            if (keyPromptObject != null)
            {
                keyPromptObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Key Assignment

    private void AssignKey()
    {
        // Get the list of keys from GameData
        if (gameData.attackKeys == null || gameData.attackKeys.Count == 0)
        {
            Debug.LogWarning("No attack keys defined in GameData! Defaulting to Space.");
            _currentKey = KeyCode.Space;
            return;
        }

        // Easy mode: always use first key
        if (gameData.currentDifficulty == EnemyDifficultyType.EasyMode)
        {
            _currentKey = gameData.attackKeys[0];
        }
        else // Medium and Hard: random key from the list
        {
            int randomIndex = UnityEngine.Random.Range(0, gameData.attackKeys.Count);
            _currentKey = gameData.attackKeys[randomIndex];
        }
        
        Debug.Log($"Player must press: {_currentKey}");
        
        // Update the key prompt text if it has a TextMeshProUGUI component
        if (keyPromptObject != null)
        {
            TextMeshProUGUI promptText = keyPromptObject.GetComponent<TextMeshProUGUI>();
            if (promptText != null)
            {
                promptText.text = _currentKey.ToString();
            }
        }
    }

    #endregion

    #region UI Logic

    private void UpdateFaultUI()
    {
        faultText.enabled = gameData.faultCounter >= 1;
    }

    #endregion

    #region Game Logic

    // Handles fault registration and win condition logic.
    private void RegisterFault()
    {
        gameData.faultCounter++;
        DuelController.instance.playerFault = true;
        
        if (gameData.faultCounter < 2)
        {
            StartCoroutine(DuelController.instance.FaultRestart());
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
        newPosition.x = 600;
        transform.localPosition = newPosition;
    }

    #endregion
}