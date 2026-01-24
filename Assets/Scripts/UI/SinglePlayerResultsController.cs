using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class SinglePlayerResultsController : MonoBehaviour
    {
        [Header("Single Player UI")]
        [SerializeField] private TextMeshProUGUI topTimeText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private Image soloCharacterImage;

        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        [SerializeField] private GameData gameData;

        private void Start()
        {
            if (gameData == null || playerData == null)
            {
                Debug.LogError("GameData or PlayerData is not assigned!");
                return;
            }

            ShowSinglePlayerResults();
        }

        private void ShowSinglePlayerResults()
        {
            // Display timing stats
            if (topTimeText != null)
                topTimeText.text = playerData.lastBestFrameCount.ToString(CultureInfo.CurrentCulture);

            if (bestTimeText != null)
                bestTimeText.text = playerData.currentBestFrameCount.ToString(CultureInfo.CurrentCulture);

            // Show winning character sprite
            var winningCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == gameData.winningCharacter);
            if (winningCharacter != null && soloCharacterImage != null)
                soloCharacterImage.sprite = winningCharacter.sprites[0];
        }
        
        public void BackToMenu()
        {
            // Set state to SinglePlayerMenu so when main menu loads, 
            // it shows the single player panel
            gameData.currentMainMenuState = MainMenuState.BackFromSinglePlayer;
            
            // Load main menu scene
            SceneLoader.instance.LoadMainMenu();
        }
    }
}