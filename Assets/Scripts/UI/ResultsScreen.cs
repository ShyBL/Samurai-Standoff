using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class ResultsScreen : MonoBehaviour
    {
        [Header("UI")] [SerializeField] private GameObject soloText, multiText;
        [SerializeField] private TextMeshProUGUI topTimeText;
        [SerializeField] private TextMeshProUGUI bestTimeText;

        [SerializeField] private Image characterImage;

        [Header("Data")] [SerializeField] private PlayerData playerData;
        [SerializeField] private GameData gameData;

        private void Start()
        {
            // if (LevelManager.instance.isMultiplayer == false)
            // {
            //     soloText.SetActive(true);
            // }
            // else
            // {
            //     multiText.SetActive(false);
            // }

            if (playerData == null || gameData == null) return;

            var winningCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == gameData.winningCharacter);
            if (winningCharacter != null)
            {
                characterImage.sprite = winningCharacter.sprites[0];
            }

            topTimeText.text = playerData.lastBestFrameCount.ToString(CultureInfo.CurrentCulture);
            bestTimeText.text = playerData.currentBestFrameCount.ToString(CultureInfo.CurrentCulture);

        }
    }
}