using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerData player2Data;
        [SerializeField] private GameData gameData;

        #region Unity Methods
        
        private void Start()
        {
            AudioManager.instance.PlaySound("Menu");
            UpdateCharacterDisplay();
            UpdateDifficultyButtons();
        }

        #endregion

        #region Buttons

        [Header("Difficulty UI")] [SerializeField]
        private List<Button> difficultyButtons;

        [SerializeField] private List<TextMeshProUGUI> difficultyText;
        [SerializeField] private TextMeshProUGUI selectedCharacterNameText;

        [Header("Difficulty Text Colors")] [SerializeField]
        private Color activeTextColor = new Color32(255, 255, 255, 255);

        [SerializeField] private Color inactiveTextColor = new Color32(255, 255, 255, 125);

        [Header("Character Selection")]
        [SerializeField] private GameObject characterSelectionPanel;
        [SerializeField] private GameObject menuSelectionPanel;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image player1MultiplayerCharacterImage;
        [SerializeField] private Image player2MultiplayerCharacterImage;
        [SerializeField] private TextMeshProUGUI player1MultiplayerCharacterNameText;
        [SerializeField] private TextMeshProUGUI player2MultiplayerCharacterNameText;

        public void SelectCharacterSinglePlayer(int index)
        {
            if (playerData == null || gameData == null) return;

            CharacterType type = (CharacterType)index;
            var selected = gameData.allCharacters.FirstOrDefault(c => c.type == type);

            if (selected != null)
            {
                playerData.selectedCharacter = selected;
                playerData.characterType = selected.type;

                selectedCharacterNameText.text = selected.name;
                characterImage.sprite = selected.sprites[0];

                Debug.Log($"[SinglePlayer] Selected character: {selected.name}");
            }
            else
            {
                Debug.LogWarning($"Character with index {index} not found.");
                return;
            }
        }

        public void SelectCharacterMultiplayer(int index)
        {
            if (playerData == null || player2Data == null || gameData == null) return;

            CharacterType type = (CharacterType)index;
            var selected = gameData.allCharacters.FirstOrDefault(c => c.type == type);

            if (selected != null)
            {
                if (!player2pick)
                {
                    playerData.selectedCharacter = selected;
                    playerData.characterType = selected.type;

                    player1MultiplayerCharacterNameText.text = selected.name;
                    player1MultiplayerCharacterImage.sprite = selected.sprites[0];

                    Debug.Log($"[Multiplayer] Player 1 selected character: {selected.name}");
                }
                else
                {
                    player2Data.selectedCharacter = selected;
                    player2Data.characterType = selected.type;

                    player2MultiplayerCharacterNameText.text = selected.name;
                    player2MultiplayerCharacterImage.sprite = selected.sprites[0];

                    Debug.Log($"[Multiplayer] Player 2 selected character: {selected.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Character with index {index} not found.");
                return;
            }
        }
        
        public void SelectCharacterByIndex(int index)
        {
            if (playerData == null || gameData == null) return;

            CharacterType type = (CharacterType)index;
            Character selected = gameData.allCharacters.FirstOrDefault(c => c.type == type);

            if (selected != null)
            {
                if(player2pick == false)
                {
                    playerData.selectedCharacter = selected;
                    playerData.characterType = selected.type;

                    selectedCharacterNameText.text = selected.name;
                    characterImage.sprite = selected.sprites[0];

                    Debug.Log($"Selected character: {selected.name}");
                }
                else
                {
                    player2Data.selectedCharacter = selected;
                    player2Data.characterType = selected.type;

                    selectedCharacterNameText.text = selected.name;
                    characterImage.sprite = selected.sprites[0];

                    Debug.Log($"Selected character: {selected.name}");
                }
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
                case 3: // Tutorial
                    GameManager.instance.SetTutorialMode();
                    break;

                default:
                    Debug.LogWarning($"Invalid difficulty index: {index}");
                    return;
            }

            DisableDifficultyButtons();
            if (index == 3)
            {
                SceneLoader.instance.LoadTutorialDuel();
            }
            else
            {
                SceneLoader.instance.LoadDuel();
            }
            
        }

        [SerializeField] private TextMeshProUGUI startText;
        [SerializeField] private TextMeshProUGUI playerChoosesText;
        private bool playersReady;
        private bool player2pick;
        
        public void MultiplayerPlayButton()
        {
            if (!player2pick)
            {
                player2pick = true;
                playerChoosesText.text = "Player 2 Chooses";
                startText.text = "Start";
            }
            else if (!playersReady)
            {
                playersReady = true;
                SceneLoader.instance.LoadMultiplayer();
            }
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

        #endregion
        private void UpdateCharacterDisplay()
        {
            if (playerData != null && gameData != null)
            {
                var playerSelectedCharacter = playerData.selectedCharacter =
                    gameData.allCharacters.FirstOrDefault(c => c.type == playerData.characterType);

                if (playerSelectedCharacter != null)
                {
                    characterImage.sprite = playerSelectedCharacter.sprites[0];
                    selectedCharacterNameText.text = playerSelectedCharacter.name;
                }
            }
        }

        public void ShowCharacterSelection()
        {
            menuSelectionPanel.SetActive(false);
            characterSelectionPanel.SetActive(true);
        }

        public void OpenDiscordServer()
        {
            Application.OpenURL("https://discord.gg/Jwt5a9W7Aq");
        }

        public void OpenGoogleForm()
        {
            Application.OpenURL("https://forms.gle/bs9jxMEavH6QVxeh8");
        }
    }
}