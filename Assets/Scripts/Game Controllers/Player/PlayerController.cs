using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Player UI Elements")] 
    [SerializeField] private TextMeshProUGUI faultText;

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerImage;

    [Header("Player State")] 
    [SerializeField] private bool hasPlayerAttacked;
    [SerializeField] private PlayerData playerData;

    private Character _characterData;
    #endregion

    #region Public Fields

    public int faultCounter;

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
        playerNameText.text = _characterData?.name;
    }

    private void Update()
    {
        UpdateFaultUI();

        if (Input.GetKeyDown(KeyCode.Space) && !hasPlayerAttacked)
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
                }
            }
        }

        if (DuelController.instance.winnerDeclared && !hasPlayerAttacked)
        {
            playerImage.sprite = _characterData.sprites[2]; // Lose sprite
            MovePlayerToAttackPosition();
        }
    }

    #endregion

    #region UI Logic

    private void UpdateFaultUI()
    {
        faultText.enabled = faultCounter >= 1;
    }

    #endregion

    #region Game Logic

    // Handles fault registration and win condition logic.
    private void RegisterFault()
    {
        faultCounter++;

        if (faultCounter < 2)
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