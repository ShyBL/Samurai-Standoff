using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SamuraiStandoff
{
    public class SceneLoader : BaseGameBehaviour
    {
        public static SceneLoader instance;
        [SerializeField] private GameObject transitionGameObject;
        [SerializeField] Animator transition;

        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            gameManager = FindObjectOfType<GameManager>();      
        }

        public void Clash()
        {
            transitionGameObject.SetActive(true);
        }

        //----Scene Transitions-----
        
        /// <summary>
        /// Load Single Player Duel scene
        /// </summary>
        public void LoadSinglePlayerDuel()
        {
            gameManager.CleanUp();
            GameManager.instance.StartCoroutine(LoadScene(1)); // Single Player Duel scene index

            StopMenuMusicAndPlayFight();
        }
        
        /// <summary>
        /// Load Tutorial Duel scene
        /// </summary>
        public void LoadTutorialDuel()
        {
            gameManager.CleanUp();
            GameManager.instance.StartCoroutine(LoadScene(5)); // Tutorial scene index

            StopMenuMusicAndPlayFight();
        }

        /// <summary>
        /// Load Multiplayer Duel scene
        /// </summary>
        public void LoadMultiplayerDuel()
        {
            gameManager.CleanUp();
            GameManager.instance.StartCoroutine(LoadScene(3)); // Multiplayer Duel scene index

            StopMenuMusicAndPlayFight();
        }

        /// <summary>
        /// Restart the current duel (handles both single and multiplayer)
        /// </summary>
        public void RestartDuel()
        {
            playerData.currentLevel = 1;

            if (gameData.isMultiplayer)
            {
                GameManager.instance.StartCoroutine(LoadScene(3)); // Multiplayer scene
            }
            else
            {
                GameManager.instance.StartCoroutine(LoadScene(1)); // Single Player scene
            }
            
            StopMenuMusicAndPlayFight();
        }

        public void LoadMainMenu()
        {
            playerData.currentLevel = 1;
            StartCoroutine(LoadMainMenuScene());
        }

        private IEnumerator LoadMainMenuScene()
        {
            yield return StartCoroutine(LoadScene(0));
            gameData.isMultiplayer = false;
            
            // Wait one frame to ensure scene objects are initialized
            yield return null;
            
            // Find MenuController and update panels
            MenuController menuController = FindObjectOfType<MenuController>();
            if (menuController != null)
            {
                menuController.ShowCharacterSelection();
            }
        }

        private IEnumerator LoadScene(int levelIndex)
        {
            transition.SetTrigger("TransitionIn");
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(levelIndex);
        }

        /// <summary>
        /// Progress to next level (single player only)
        /// </summary>
        public IEnumerator NextLevel()
        {
            yield return new WaitForSeconds(3f);
            Debug.Log("Next Level");
            
            playerData.currentLevel++;

            if (playerData.currentLevel > GameManager.instance.totalLevels)
            {
                StartCoroutine(LoadResults());
            }
            else
            {
                LoadSinglePlayerDuel();
            }
        }

        public IEnumerator LoadResults()
        {
            playerData.faultCounter = 0;
            player2Data.faultCounter = 0;

            yield return new WaitForSeconds(3f);

            if (!gameData.isMultiplayer)
            {
                StartCoroutine(LoadScene(2)); // Single Player Results scene   
            }
            else
            {
                StartCoroutine(LoadScene(4)); // Multiplayer Results scene
            }

            AudioManager.instance.StopSound("Fight");
            AudioManager.instance.PlaySound("Menu");
        }

        /// <summary>
        /// Helper method to stop menu music and start fight music
        /// </summary>
        private void StopMenuMusicAndPlayFight()
        {
            var menuSound = AudioManager.instance.sounds.FirstOrDefault(s => s.name == "Menu");

            if (menuSound == null || !menuSound.source.isPlaying)
            {
                Debug.LogWarning("Menu music is not playing. Load game proceeding anyway.");
            }
            else
            {
                Debug.Log("Menu music is playing. Proceeding to load game.");
                AudioManager.instance.StopSound("Menu");
            }

            AudioManager.instance.PlaySound("Fight");
        }
    }
}