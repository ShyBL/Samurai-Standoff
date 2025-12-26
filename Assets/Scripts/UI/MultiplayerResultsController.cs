using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class MultiplayerResultsController : MonoBehaviour
    {
        [Header("Multiplayer UI")]
        [SerializeField] private Image player1Image;
        [SerializeField] private Image player2Image;
        [SerializeField] private TextMeshProUGUI player1ScoreText;
        [SerializeField] private TextMeshProUGUI player2ScoreText;

        [Header("Data")]
        [SerializeField] private PlayerData player1Data;
        [SerializeField] private PlayerData player2Data;
        [SerializeField] private GameData gameData;

        private void Start()
        {
            if (gameData == null || player1Data == null || player2Data == null)
            {
                Debug.LogError("GameData or PlayerData is not assigned!");
                return;
            }

            ShowMultiplayerResults();
        }

        private void ShowMultiplayerResults()
        {
            // Show player images
            if (player1Image != null && player1Data.selectedCharacter != null)
                player1Image.sprite = player1Data.selectedCharacter.sprites[0];

            if (player2Image != null && player2Data.selectedCharacter != null)
                player2Image.sprite = player2Data.selectedCharacter.sprites[0];

            // Show scores
            if (player1ScoreText != null)
                player1ScoreText.text = $"Faults: {player1Data.faultCounter}";

            if (player2ScoreText != null)
                player2ScoreText.text = $"Faults: {player2Data.faultCounter}";
        }
    }
}