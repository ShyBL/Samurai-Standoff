using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class SettingsController : MonoBehaviour
    {
        #region Audio Settings

        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeValueText;

        [SerializeField] private Slider backgroundVolumeSlider;
        [SerializeField] private TextMeshProUGUI backgroundVolumeValueText;

        private AudioManager _audioManager;

        private void Start()
        {
            _audioManager = AudioManager.instance;

            // Initialize both sliders
            UpdateAudio(masterVolumeSlider, masterVolumeValueText, "MasterVolume", gameData.masterVolume);
            UpdateAudio(backgroundVolumeSlider, backgroundVolumeValueText, "BackgroundVolume", gameData.backgroundVolume);
        }

        private void UpdateAudio(Slider slider, TextMeshProUGUI label, string mixerParam, float savedValue)
        {
            LoadVolumeFromPlayerData(slider, mixerParam, savedValue);
            UpdateVolumeLabel(slider, label);

            slider.onValueChanged.AddListener(value => ApplyVolume(value, slider, label, mixerParam));
        }

        private void LoadVolumeFromPlayerData(Slider slider, string mixerParam, float savedValue)
        {
            var clamped = Mathf.Clamp(savedValue, 1f, 100f);
            slider.value = clamped;

            var normalized = clamped / 100f;
            var curved = Mathf.Pow(normalized, 2f);
            var volumeDb = Mathf.Lerp(-60f, 0f, curved);

            _audioManager.audioMixer.SetFloat(mixerParam, volumeDb);
        }

        private void ApplyVolume(float value, Slider slider, TextMeshProUGUI label, string mixerParam)
        {
            var normalized = value / 100f;
            var curved = Mathf.Pow(normalized, 2f);
            var volumeDb = Mathf.Lerp(-60f, 0f, curved);

            _audioManager.audioMixer.SetFloat(mixerParam, volumeDb);

            // Save to gameData depending on which slider
            if (mixerParam == "MasterVolume")
                gameData.masterVolume = value;
            else if (mixerParam == "BackgroundVolume")
                gameData.backgroundVolume = value;

            Debug.Log($"Applied and saved {mixerParam}: {value} → {volumeDb} dB");
            UpdateVolumeLabel(slider, label);
        }

        private void UpdateVolumeLabel(Slider slider, TextMeshProUGUI label)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(slider.value)}";
            }
        }

        #endregion
        
        #region Key Bindings
        
        [Header("References")] 
        [SerializeField] private GameData gameData;
        [SerializeField] private GameObject keybindPanel;

        [Header("Player One Key Buttons")]
        [SerializeField] private Button p1Key1Button;
        [SerializeField] private Button p1Key2Button;
        [SerializeField] private Button p1Key3Button;
            
        [Header("Player Two Key Buttons")]
        [SerializeField] private Button p2Key1Button;
        [SerializeField] private Button p2Key2Button;
        [SerializeField] private Button p2Key3Button;
        
        private bool waitingForKey = false;
        private int currentKeyIndex = -1;
        private bool isPlayerOne = true;

        public List<KeyCode> playerOneKeys;
        public List<KeyCode> playerTwoKeys;
        
        private void Awake()
        {
            for (var i = 0; i < gameData.attackKeys.Count; i++)
            {
                playerOneKeys[i] = gameData.attackKeys[i];
            }
            
            for (var i = 0; i < gameData.p2AttackKeys.Count; i++)
            {
                playerTwoKeys[i] = gameData.p2AttackKeys[i];
            }

            SetupButtonListeners();
            UpdateUI();
        }

        
        
        private void Update()
        {
            if (waitingForKey)
            {
                // Check for ESC to cancel
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelRebind();
                    return;
                }
                
                foreach (KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(code))
                    {
                        // Ignore mouse buttons and system keys
                        if (IsValidKey(code))
                        {
                            AssignKey(code);
                            break;
                        }
                    }
                }
            }
        }
        
        private bool IsValidKey(KeyCode code)
        {
            // Exclude mouse buttons
            if (code >= KeyCode.Mouse0 && code <= KeyCode.Mouse6)
                return false;
            
            // Exclude joystick buttons
            if (code >= KeyCode.JoystickButton0 && code <= KeyCode.Joystick8Button19)
                return false;
            
            // Exclude some system keys
            if (code == KeyCode.Escape || code == KeyCode.None)
                return false;
            
            return true;
        }

        private void AssignKey(KeyCode newKey)
        {
            var targetArray = isPlayerOne ? playerOneKeys : playerTwoKeys;
            
            // Check if key is already assigned to another slot
            for (int i = 0; i < targetArray.Count; i++)
            {
                if (i != currentKeyIndex && targetArray[i] == newKey)
                {
                    Debug.LogWarning($"Key {newKey} is already assigned to another slot!");
                    return;
                }
            }
            
            // Assign the new key
            targetArray[currentKeyIndex] = newKey;
            
            // Update GameData with player one keys
            if (isPlayerOne && currentKeyIndex < gameData.attackKeys.Count)
            {
                gameData.attackKeys[currentKeyIndex] = newKey;
            }
            
            Debug.Log($"Assigned {newKey} to {(isPlayerOne ? "Player One" : "Player Two")} slot {currentKeyIndex}");
            
            // Reset and update UI
            waitingForKey = false;
            currentKeyIndex = -1;
            
            UpdateUI();
            
            if (keybindPanel != null)
            {
                keybindPanel.SetActive(false);
            }
            
            
        }
        
        private void SetupButtonListeners()
        {
            // Player One buttons
            if (p1Key1Button != null) p1Key1Button.onClick.AddListener(() => RebindKey(0, true));
            if (p1Key2Button != null) p1Key2Button.onClick.AddListener(() => RebindKey(1, true));
            if (p1Key3Button != null) p1Key3Button.onClick.AddListener(() => RebindKey(2, true));
            
            // Player Two buttons (if multiplayer)
            if (p2Key1Button != null) p2Key1Button.onClick.AddListener(() => RebindKey(0, false));
            if (p2Key2Button != null) p2Key2Button.onClick.AddListener(() => RebindKey(1, false));
            if (p2Key3Button != null) p2Key3Button.onClick.AddListener(() => RebindKey(2, false));
        }

        public void RebindKey(int keyIndex, bool playerOne)
        {
            if (!waitingForKey)
            {
                waitingForKey = true;
                currentKeyIndex = keyIndex;
                isPlayerOne = playerOne;
                
                if (keybindPanel != null)
                {
                    keybindPanel.SetActive(true);
                }
            }
        }

        public void ResetToDefaults()
        {
            playerOneKeys[0] = KeyCode.A;
            playerOneKeys[1] = KeyCode.S;
            playerOneKeys[2] = KeyCode.D;
            
            playerTwoKeys[0] = KeyCode.J;
            playerTwoKeys[1] = KeyCode.K;
            playerTwoKeys[2] = KeyCode.L;
            
            // Update GameData
            for (var i = 0; i < gameData.attackKeys.Count; i++)
            {
                gameData.attackKeys[i] = playerOneKeys[i];
            }
            
            for (var i = 0; i < gameData.p2AttackKeys.Count; i++)
            {
                gameData.p2AttackKeys[i] = playerTwoKeys[i];
            }
            
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Update Player One buttons
            UpdateButtonText(p1Key1Button, playerOneKeys[0]);
            UpdateButtonText(p1Key2Button, playerOneKeys[1]);
            UpdateButtonText(p1Key3Button, playerOneKeys[2]);
            
            // Update Player Two buttons (if they exist)
            UpdateButtonText(p2Key1Button, playerTwoKeys[0]);
            UpdateButtonText(p2Key2Button, playerTwoKeys[1]);
            UpdateButtonText(p2Key3Button, playerTwoKeys[2]);
        }

        private void UpdateButtonText(Button button, KeyCode key)
        {
            if (button != null)
            {
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                
                if (buttonText != null)
                {
                    buttonText.text = FormatKeyName(key);
                }
            }
        }
        
        private string FormatKeyName(KeyCode key)
        {
            // Make key names more readable
            var keyName = key.ToString();
            
            // Handle special cases
            switch (key)
            {
                case KeyCode.Space:
                    return "SPACE";
                case KeyCode.LeftShift:
                    return "L-SHIFT";
                case KeyCode.RightShift:
                    return "R-SHIFT";
                case KeyCode.LeftControl:
                    return "L-CTL";
                case KeyCode.RightControl:
                    return "R-CTRL";
                case KeyCode.LeftAlt:
                    return "L-ALT";
                case KeyCode.RightAlt:
                    return "R-ALT";
                case KeyCode.Return:
                    return "ENTER";
                case KeyCode.Semicolon:
                    return ";";
                default:
                    // Remove "Alpha" prefix from number keys
                    return keyName.StartsWith("Alpha") ? keyName.Replace("Alpha", "") : keyName.ToUpper();
            }
        }

        private void CancelRebind()
        {
            waitingForKey = false;
            currentKeyIndex = -1;
            
            if (keybindPanel != null)
            {
                keybindPanel.SetActive(false);
            }
        }
        #endregion
    }
}