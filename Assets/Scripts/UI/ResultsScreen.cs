using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class ResultsScreen : MonoBehaviour
    {
        [Header("Single Player UI")]
        [SerializeField] private GameObject soloPanel;
        [SerializeField] private TextMeshProUGUI topTimeText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private Image soloCharacterImage;

        [Header("Multiplayer UI")]
        [SerializeField] private GameObject multiPanel;
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
            if (gameData == null)
            {
                Debug.LogError("GameData is not assigned!");
                return;
            }

            if (gameData.isMultiplayer)
            {
                ShowMultiplayerResults();
            }
            else
            {
                ShowSinglePlayerResults();
            }
        }

        private void ShowSinglePlayerResults()
        {
            // Show single player panel, hide multiplayer
            if (soloPanel != null) soloPanel.SetActive(true);
            if (multiPanel != null) multiPanel.SetActive(false);

            if (player1Data == null)
            {
                Debug.LogError("Player1Data is not assigned!");
                return;
            }

            // Display timing stats
            if (topTimeText != null)
            {
                topTimeText.text = player1Data.lastBestFrameCount.ToString(CultureInfo.CurrentCulture);
            }

            if (bestTimeText != null)
            {
                bestTimeText.text = player1Data.currentBestFrameCount.ToString(CultureInfo.CurrentCulture);
            }
            
            var winningCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == gameData.winningCharacter);
            if (winningCharacter != null)
            {
                soloCharacterImage.sprite = winningCharacter.sprites[0];
            }
        }

        private void ShowMultiplayerResults()
        {
            // Show multiplayer panel, hide single player
            if (soloPanel != null) soloPanel.SetActive(false);
            if (multiPanel != null) multiPanel.SetActive(true);

            if (player1Data == null || player2Data == null)
            {
                Debug.LogError("Player1Data or Player2Data is not assigned!");
                return;
            }
        }
    }
}