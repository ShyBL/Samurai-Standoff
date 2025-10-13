using System;
using System.Collections;
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
    [SerializeField] private GameObject characterSelectionPanel;
    [SerializeField] private GameObject menuSelectionPanel;

    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameData gameData;
    [SerializeField] private Image characterImage;
    
    [Header("Settings")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeValueText;

    private AudioManager _audioManager;
    private bool _introFinished;
    
    private IEnumerator WaitForIntroToFinish(Sound introSound)
    {
        _introFinished = true;

        float clipLength = introSound.source.clip.length;
        Debug.Log($"Waiting for {clipLength} seconds (Intro clip length).");

        yield return new WaitForSeconds(5.5f);

        Debug.Log("Intro clip duration elapsed. Stopping Intro sound.");
        _audioManager.StopSound("Intro");

        yield return null; // Wait one frame

        if (!introSound.source.isPlaying)
        {
            Debug.Log("Intro sound has stopped. Playing Menu sound.");
            _audioManager.PlaySound("Menu");
            _introFinished = false;
        }
        else
        {
            Debug.LogWarning("Intro sound is still playing after StopSound call.");
        }
    }
    
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
        
        var introSound = _audioManager.sounds.FirstOrDefault(s => s.name == "Intro");

        if (introSound != null && introSound.source.isPlaying)
        {
            Debug.Log("Intro is currently playing. Starting coroutine.");
            StartCoroutine(WaitForIntroToFinish(introSound));
        }
        else
        {
            Debug.Log("Intro is not playing. Skipping coroutine.");
        }
        
        LoadVolumeFromPlayerData();
        UpdateVolumeLabel();

        volumeSlider.onValueChanged.AddListener(ApplyVolume);
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
        SceneLoader.instance.LoadDuel();
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

        bool hardUnlocked = playerData.completedMediumMode;
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

    public void ApplyVolume(float arg0)
    {
        var normalized = arg0 / 100f;
    
        var curved = Mathf.Pow(normalized, 2f);
    
        // Map to dB range (-80 to +20)
        var volumeDb = Mathf.Lerp(-80f, 20f, curved);
    
        _audioManager.audioMixer.SetFloat("Volume", volumeDb);
        gameData.volume = arg0;
    
        Debug.Log($"Applied and saved volume: {arg0} → {volumeDb} dB");
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
    
    public void ShowCharacterSelection()
    {
        menuSelectionPanel.SetActive(false);
        characterSelectionPanel.SetActive(true);
    }

    
}