using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Difficulty UI")] 
    [SerializeField] private List<Button> difficultyButtons;
    [SerializeField] private List<TextMeshProUGUI> difficultyText;
    [SerializeField] private TextMeshProUGUI selectedCharacterNameTest;

    [Header("Difficulty Text Colors")]
    [SerializeField] private Color activeTextColor = new Color32(255, 255, 255, 255);
    [SerializeField] private Color inactiveTextColor = new Color32(255, 255, 255, 125);
    
    [Header("Character Selection")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameData gameData;
    [SerializeField] private Image characterImage;
    
    [Header("Settings")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeValueText;

    private AudioManager _audioManager;
    private void Start()
    {
        if (playerData != null || gameData != null)
        {
            var playerSelectedCharacter = playerData.selectedCharacter = gameData.allCharacters.FirstOrDefault(c => c.type == playerData.characterType);
        
            if (playerSelectedCharacter != null)
            {
                characterImage.sprite = playerSelectedCharacter.sprites[0];
                selectedCharacterNameTest.text = playerSelectedCharacter.name;
            }
        }
        
        UpdateDifficultyButtons();
        
        _audioManager = AudioManager.instance;
        LoadVolumeFromPlayerData();
        UpdateVolumeLabel();
    }

    public void SelectCharacterByIndex(int index)
    {
        if (playerData == null || gameData == null) return;

        CharacterType type = (CharacterType)index;
        var selected = gameData.allCharacters.FirstOrDefault(c => c.type == type);

        if (selected != null)
        {
            playerData.selectedCharacter = selected;
            playerData.characterType = selected.type;
            
            selectedCharacterNameTest.text = selected.name;
            characterImage.sprite = selected.sprites[0];
            
            Debug.Log($"Selected character: {selected.name}");
        }
        else
        {
            Debug.LogWarning($"Character with index {index} not found.");
        }
    }

    public void SetDifficultyByIndex(int index)
    {
        if (playerData == null) return;

        switch (index)
        {
            case 0: // Easy
                GameManager.instance.SetEasyMode();
                break;

            case 1: // Medium
                if (!playerData.completedEasyMode) return;
                GameManager.instance.SetMediumMode();
                break;

            case 2: // Hard
                if (!playerData.completedEasyMode) return;
                GameManager.instance.SetHardMode();
                break;

            default:
                Debug.LogWarning($"Invalid difficulty index: {index}");
                return;
        }

        DisableDifficultyButtons();
        SceneLoader.instance.Loadgame();
    }

    public void ApplicationQuit()
    {
        GameManager.instance.OnApplicationQuit();
    }
    
    private void DisableDifficultyButtons()
    {
        foreach (var button in difficultyButtons)
        {
            button.interactable = false;
        }
    }

    private void UpdateDifficultyButtons()
    {
        // Assume buttons are ordered: Easy (0), Medium (1), Hard (2)
        if (difficultyButtons == null || difficultyButtons.Count < 3) return;
        if (difficultyText == null || difficultyText.Count < 3) return;

        difficultyButtons[0].interactable = true;
        difficultyText[0].color = new Color32(255, 255, 255, 255); // fully visible

        bool mediumUnlocked = playerData.completedEasyMode;
        difficultyButtons[1].interactable = mediumUnlocked;
        difficultyText[1].color = mediumUnlocked ? activeTextColor : inactiveTextColor;

        bool hardUnlocked = playerData.completedEasyMode;
        difficultyButtons[2].interactable = hardUnlocked;
        difficultyText[2].color = hardUnlocked ? activeTextColor : inactiveTextColor;
    }

    private void LoadVolumeFromPlayerData()
    {
        var savedVolume = Mathf.Clamp(gameData.volume, 0.1f, 100f);
        volumeSlider.value = savedVolume;

        var normalized = savedVolume / 100f;
        var curved = Mathf.Pow(normalized, 2f);
        var volumeDb = Mathf.Lerp(-80f, 20f, curved);

        _audioManager.audioMixer.SetFloat("Volume", volumeDb);
    }

    public void ApplyVolume()
    {
        var sliderValue = Mathf.Clamp(volumeSlider.value, 0.0001f, 100f);
    
        var normalized = sliderValue / 100f;
    
        var curved = Mathf.Pow(normalized, 2f);
    
        // Map to dB range (-80 to +20)
        var volumeDb = Mathf.Lerp(-80f, 20f, curved);
    
        _audioManager.audioMixer.SetFloat("Volume", volumeDb);
        gameData.volume = sliderValue;
    
        Debug.Log($"Applied and saved volume: {sliderValue} → {volumeDb} dB");
    }

    public void CancelApplyVolume()
    {
        LoadVolumeFromPlayerData();
    }

    public void UpdateVolumeLabel()
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = $"{Mathf.RoundToInt(volumeSlider.value)}";

        }
    }
    
}