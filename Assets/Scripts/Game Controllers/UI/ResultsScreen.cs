using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScreen : MonoBehaviour
{
    [Header("UI")] 
    [SerializeField] private GameObject soloText, multiText;
    [SerializeField] private TextMeshProUGUI topTimeText;
    [SerializeField] private TextMeshProUGUI bestTimeText;

    [SerializeField] private Image characterImage;
    
    [Header("Data")] 
    [SerializeField] private PlayerData playerData;
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
        
        var playerSelectedCharacter = playerData.selectedCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == playerData.characterType);
        if (playerSelectedCharacter != null)
        {
            characterImage.sprite = playerSelectedCharacter.sprites[0];
        }
        
        topTimeText.text = playerData.lastBestFrameCount.ToString(CultureInfo.CurrentCulture);
        bestTimeText.text = playerData.currentBestFrameCount.ToString(CultureInfo.CurrentCulture);

    }
}