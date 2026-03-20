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

            UpdateAudio(masterVolumeSlider,     masterVolumeValueText,    "MasterVolume",     gameData.masterVolume);
            UpdateAudio(backgroundVolumeSlider, backgroundVolumeValueText,"BackgroundVolume", gameData.backgroundVolume);
        }

        private void UpdateAudio(Slider slider, TextMeshProUGUI label, string mixerParam, float savedValue)
        {
            LoadVolumeFromPlayerData(slider, mixerParam, savedValue);
            UpdateVolumeLabel(slider, label);

            slider.onValueChanged.AddListener(value => ApplyVolume(value, slider, label, mixerParam));
        }

        private void LoadVolumeFromPlayerData(Slider slider, string mixerParam, float savedValue)
        {
            var clamped    = Mathf.Clamp(savedValue, 1f, 100f);
            slider.value   = clamped;

            float dB = Mathf.Clamp(Mathf.Log10(clamped / 100f) * 20f, -60f, 0f);
            _audioManager.audioMixer.SetFloat(mixerParam, dB);
        }

        private void ApplyVolume(float value, Slider slider, TextMeshProUGUI label, string mixerParam)
        {
            float dB = Mathf.Clamp(Mathf.Log10(value / 100f) * 20f, -60f, 0f);
            _audioManager.audioMixer.SetFloat(mixerParam, dB);

            if (mixerParam == "MasterVolume")
                gameData.masterVolume = value;
            else if (mixerParam == "BackgroundVolume")
                gameData.backgroundVolume = value;

            Debug.Log($"Applied and saved {mixerParam}: {value} → {dB} dB");
            UpdateVolumeLabel(slider, label);

            SaveSystem.instance.Save();
        }

        private void UpdateVolumeLabel(Slider slider, TextMeshProUGUI label)
        {
            if (label != null)
                label.text = $"{Mathf.RoundToInt(slider.value)}";
        }

        #endregion

        #region Display Settings

        [Header("Display Settings")]
        [SerializeField] private Button borderlessButton;
        [SerializeField] private Button windowedButton;
        [SerializeField] private TextMeshProUGUI displayModeLabel;

        // Called from the Borderless button in the Inspector
        public void SetBorderless()
        {
            Resolution native = Screen.resolutions[Screen.resolutions.Length - 1];
            Screen.SetResolution(native.width, native.height, FullScreenMode.FullScreenWindow);
            gameData.displayMode = FullScreenMode.FullScreenWindow;
            UpdateDisplayModeLabel();
            SaveSystem.instance.Save();
            Debug.Log("[Display] Set to Borderless Windowed");
        }

        // Called from the Windowed button in the Inspector
        public void SetWindowed()
        {
            Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.Windowed);
            gameData.displayMode = FullScreenMode.Windowed;
            UpdateDisplayModeLabel();
            SaveSystem.instance.Save();
            Debug.Log("[Display] Set to Windowed");
        }

        private void UpdateDisplayModeLabel()
        {
            if (displayModeLabel == null) return;

            displayModeLabel.text = gameData.displayMode switch
            {
                FullScreenMode.Windowed => "Windowed",
                _                       => "Borderless"
            };
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

        private bool _waitingForKey   = false;
        private int  _currentKeyIndex = -1;
        private bool _isPlayerOne     = true;

        public List<KeyCode> playerOneKeys;
        public List<KeyCode> playerTwoKeys;

        private void Awake()
        {
            for (var i = 0; i < gameData.attackKeys.Count; i++)
                playerOneKeys[i] = gameData.attackKeys[i];

            for (var i = 0; i < gameData.p2AttackKeys.Count; i++)
                playerTwoKeys[i] = gameData.p2AttackKeys[i];

            SetupButtonListeners();
            UpdateUI();
        }

        private void Update()
        {
            if (!_waitingForKey) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelRebind();
                return;
            }

            foreach (KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(code) && IsValidKey(code))
                {
                    AssignKey(code);
                    break;
                }
            }
        }

        private bool IsValidKey(KeyCode code)
        {
            if (code >= KeyCode.Mouse0          && code <= KeyCode.Mouse6)            return false;
            if (code >= KeyCode.JoystickButton0 && code <= KeyCode.Joystick8Button19) return false;
            if (code == KeyCode.Escape || code == KeyCode.None)                       return false;
            return true;
        }

        private void AssignKey(KeyCode newKey)
        {
            var targetArray = _isPlayerOne ? playerOneKeys : playerTwoKeys;

            for (int i = 0; i < targetArray.Count; i++)
            {
                if (i != _currentKeyIndex && targetArray[i] == newKey)
                {
                    Debug.LogWarning($"Key {newKey} is already assigned to another slot!");
                    return;
                }
            }

            targetArray[_currentKeyIndex] = newKey;

            if (_isPlayerOne && _currentKeyIndex < gameData.attackKeys.Count)
                gameData.attackKeys[_currentKeyIndex] = newKey;
            else if (!_isPlayerOne && _currentKeyIndex < gameData.p2AttackKeys.Count)
                gameData.p2AttackKeys[_currentKeyIndex] = newKey;

            Debug.Log($"Assigned {newKey} to {(_isPlayerOne ? "Player One" : "Player Two")} slot {_currentKeyIndex}");

            _waitingForKey   = false;
            _currentKeyIndex = -1;

            UpdateUI();

            if (keybindPanel != null)
                keybindPanel.SetActive(false);

            SaveSystem.instance.Save();
        }

        private void SetupButtonListeners()
        {
            if (p1Key1Button != null) p1Key1Button.onClick.AddListener(() => RebindKey(0, true));
            if (p1Key2Button != null) p1Key2Button.onClick.AddListener(() => RebindKey(1, true));
            if (p1Key3Button != null) p1Key3Button.onClick.AddListener(() => RebindKey(2, true));

            if (p2Key1Button != null) p2Key1Button.onClick.AddListener(() => RebindKey(0, false));
            if (p2Key2Button != null) p2Key2Button.onClick.AddListener(() => RebindKey(1, false));
            if (p2Key3Button != null) p2Key3Button.onClick.AddListener(() => RebindKey(2, false));
        }

        public void RebindKey(int keyIndex, bool playerOne)
        {
            if (_waitingForKey) return;

            _waitingForKey   = true;
            _currentKeyIndex = keyIndex;
            _isPlayerOne     = playerOne;

            if (keybindPanel != null)
                keybindPanel.SetActive(true);
        }

        public void ResetToDefaults()
        {
            playerOneKeys[0] = KeyCode.A;
            playerOneKeys[1] = KeyCode.S;
            playerOneKeys[2] = KeyCode.D;

            playerTwoKeys[0] = KeyCode.J;
            playerTwoKeys[1] = KeyCode.K;
            playerTwoKeys[2] = KeyCode.L;

            for (var i = 0; i < gameData.attackKeys.Count; i++)
                gameData.attackKeys[i] = playerOneKeys[i];

            for (var i = 0; i < gameData.p2AttackKeys.Count; i++)
                gameData.p2AttackKeys[i] = playerTwoKeys[i];

            UpdateUI();

            SaveSystem.instance.Save();
        }

        private void UpdateUI()
        {
            UpdateButtonText(p1Key1Button, playerOneKeys[0]);
            UpdateButtonText(p1Key2Button, playerOneKeys[1]);
            UpdateButtonText(p1Key3Button, playerOneKeys[2]);

            UpdateButtonText(p2Key1Button, playerTwoKeys[0]);
            UpdateButtonText(p2Key2Button, playerTwoKeys[1]);
            UpdateButtonText(p2Key3Button, playerTwoKeys[2]);
        }

        private void UpdateButtonText(Button button, KeyCode key)
        {
            if (button == null) return;

            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = FormatKeyName(key);
        }

        private string FormatKeyName(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Space:        return "SPACE";
                case KeyCode.LeftShift:    return "L-SHIFT";
                case KeyCode.RightShift:   return "R-SHIFT";
                case KeyCode.LeftControl:  return "L-CTL";
                case KeyCode.RightControl: return "R-CTRL";
                case KeyCode.LeftAlt:      return "L-ALT";
                case KeyCode.RightAlt:     return "R-ALT";
                case KeyCode.Return:       return "ENTER";
                case KeyCode.Semicolon:    return ";";
                default:
                    var keyName = key.ToString();
                    return keyName.StartsWith("Alpha") ? keyName.Replace("Alpha", "") : keyName.ToUpper();
            }
        }

        private void CancelRebind()
        {
            _waitingForKey   = false;
            _currentKeyIndex = -1;

            if (keybindPanel != null)
                keybindPanel.SetActive(false);
        }

        #endregion
    }
}