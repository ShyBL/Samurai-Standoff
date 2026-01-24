using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SamuraiStandoff
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader instance;
        
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string singlePlayerSceneName = "SingleplayerLevel";
        [SerializeField] private string singlePlayerResultsSceneName = "SingleplayerResults";
        [SerializeField] private string multiplayerSceneName = "MultiplayerLevel";
        [SerializeField] private string multiplayerResultsSceneName = "MultiplayerResults";
        [SerializeField] private string tutorialSceneName = "Tutorial";
        
        [SerializeField] private GameObject transitionGameObject;
        [SerializeField] Animator transition;
        [SerializeField] private GameData gameData;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerData player2Data;

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
            FindFirstObjectByType<BackgroundController>().ToggleFXOff();
            // transition.SetTrigger("Clash");
        }
        
        private static void TransitionToGameplayMusic()
        {
            var menuSound = AudioManager.instance.sounds.FirstOrDefault(s => s.name == "Menu");

            if (menuSound == null || !menuSound.source.isPlaying)
            {
                Debug.LogWarning("Menu music is not playing. Loadgame aborted.");
                return;
            }

            Debug.Log("Menu music is playing. Proceeding to load game.");

            AudioManager.instance.StopSound("Menu");
            AudioManager.instance.PlaySound("Fight");
        }        
        
        #region Scene Loading Methods
        
        public void LoadDuel()
        {
            gameManager.CleanUp();

            GameManager.instance.StartCoroutine(LoadScene(singlePlayerSceneName));

            TransitionToGameplayMusic();
        }
        
        public void LoadTutorialDuel()
        {
            gameManager.CleanUp();

            GameManager.instance.StartCoroutine(LoadScene(tutorialSceneName));

            TransitionToGameplayMusic();
        }

        

        public void RestartDuel()
        {
            FindFirstObjectByType<BackgroundController>().ToggleFXOff();
            
            playerData.currentLevel = 1;

            if(gameData.isMultiplayer)
            {
                GameManager.instance.StartCoroutine(LoadScene(multiplayerSceneName));
            }
            else
            {
                GameManager.instance.StartCoroutine(LoadScene(singlePlayerSceneName));
            }
            AudioManager.instance.PlaySound("Click1");

            TransitionToGameplayMusic();
            
        }

        public void LoadMainMenu()
        {
            playerData.currentLevel = 1;
            StartCoroutine(LoadMainMenuScene());
        }

        private IEnumerator LoadMainMenuScene()
        {
            yield return StartCoroutine(LoadScene(mainMenuSceneName));
            gameData.isMultiplayer = false;
        }

        public void LoadMultiplayer()
        {
            gameManager.CleanUp();
            GameManager.instance.StartCoroutine(LoadScene(multiplayerSceneName));

            TransitionToGameplayMusic();

        }

        #endregion
        
        #region Core Scene Loading
        
        private IEnumerator LoadScene(string sceneName)
        {
            transition.SetTrigger("TransitionIn");

            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(sceneName);
        }

        public IEnumerator NextLevel()
        {
            yield return new WaitForSeconds(3f);
            
            playerData.currentLevel++;
            Debug.Log("Next Level");

            if (playerData.currentLevel > GameManager.instance.totalLevels)
            {
                StartCoroutine(LoadResults());
            }
            else
            {
                LoadDuel();
            }
        }

        public IEnumerator LoadResults()
        {
            gameManager.CleanUp();

            yield return new WaitForSeconds(3f);

            string resultsScene = gameData.isMultiplayer 
                ? multiplayerResultsSceneName 
                : singlePlayerResultsSceneName;
            
            StartCoroutine(LoadScene(resultsScene));

            AudioManager.instance.StopSound("Fight");
            AudioManager.instance.PlaySound("Menu");
        }
        #endregion
    }
}