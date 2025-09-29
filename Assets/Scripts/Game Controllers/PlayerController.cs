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

    [Header("Player Data")]
    [SerializeField] private Character characterData;

    [Header("Player State")]
    [SerializeField] private bool hasPlayerAttacked;

    #endregion

    #region Public Fields

    public int faultCounter;

    #endregion

    #region Unity Methods

    private void Start()
    {
        hasPlayerAttacked = false;

        // Initialize player visuals
        playerImage.sprite = characterData.sprites[0]; // Idle sprite
        playerNameText.text = characterData?.name;
    }

    private void Update()
    {
        UpdateFaultUI();

        if (Input.GetKeyDown(KeyCode.Space) && !hasPlayerAttacked)
        {
            if (!GameController.instance.winnerDeclared)
            {
                Debug.Log("Player Attacked");
                hasPlayerAttacked = true;

                if (!Timer.instance.signal)
                {
                    RegisterFault();
                }
                else
                {
                    GameController.instance.DeclareWinner(gameObject);
                    playerImage.sprite = characterData.sprites[1]; // Win sprite
                    MovePlayerToAttackPosition();
                    
                }
            }
        }

        if (GameController.instance.winnerDeclared && !hasPlayerAttacked)
        {
            playerImage.sprite = characterData.sprites[2]; // Lose sprite
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
            StartCoroutine(GameController.instance.FaultRestart());
        }
        else
        {
            // Determine which player is at fault and declare the other as winner
            if (GameController.instance.pOne == gameObject)
            {
                GameController.instance.DeclareWinner(GameController.instance.pTwo);
            }
            else if (GameController.instance.pTwo == gameObject)
            {
                GameController.instance.DeclareWinner(GameController.instance.pOne);
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