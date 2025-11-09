using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    [SerializeField] Animator transition;
    [SerializeField] private GameData gameData;
    [SerializeField] private PlayerData playerData; 

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
    }

    public void Clash()
    {
        transition.SetTrigger("Clash");
    }

    //----Scene Transitions-----
    public void LoadDuel() //Enter Game
    {
        GameManager.instance.StartCoroutine(LoadScene(1));
        
        var menuSound = AudioManager.instance.sounds.FirstOrDefault(s => s.name == "Menu");

        if (menuSound == null || !menuSound.source.isPlaying)
        {
            Debug.LogWarning("Menu music is not playing. Loadgame aborted.");
            return;
        }

        Debug.Log("Menu music is playing. Proceeding to load game.");

        AudioManager.instance.StopSound("Menu");
        AudioManager.instance.PlaySound("Fight");


        //AudioManager.instance.StopSound("Waterfall");
        //AudioManager.instance.PlaySound("Waterfall");
    }

    public void RestartDuel()
    {
        playerData.currentLevel = 1;
        StartCoroutine(LoadScene(1));
        
        var menuSound = AudioManager.instance.sounds.FirstOrDefault(s => s.name == "Menu");

        if (menuSound == null || !menuSound.source.isPlaying)
        {
            Debug.LogWarning("Menu music is not playing. Loadgame aborted.");
            return;
        }

        Debug.Log("Menu music is playing. Proceeding to load game.");

        AudioManager.instance.StopSound("Menu");
        AudioManager.instance.PlaySound("Fight");

        // AudioManager.instance.StopSound("Waterfall");
        // AudioManager.instance.PlaySound("Waterfall");
    }

    public void LoadMainMenu()
    { 
        playerData.currentLevel = 1;
        StartCoroutine(LoadMainMenuScene());
    }

    private IEnumerator LoadMainMenuScene()
    {
        yield return StartCoroutine(LoadScene(0));

        // Wait one frame to ensure scene objects are initialized
        yield return null;

        // Find MenuController and update panels
        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            menuController.ShowCharacterSelection();
        }
    }


    private IEnumerator LoadScene(int levelIndex) //
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(levelIndex);
    }

    public IEnumerator NextLevel()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Next Level");
        // transition.SetTrigger("Start");
        playerData.currentLevel++;

        if (playerData.currentLevel > GameManager.instance.totalLevels)
        {
            StartCoroutine(LoadResults());
        }
        else
        {
            // GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            // foreach (GameObject player in players)
            // {
            //     if (TryGetComponent(out PlayerController playerController)) playerController.faultCounter = 0;
            // }
            gameData.faultCounter = 0;

            LoadDuel();
        }
    }

    public IEnumerator LoadResults()
    {
        gameData.faultCounter = 0;
        yield return new WaitForSeconds(3f);
        StartCoroutine(LoadScene(2));
        
        AudioManager.instance.StopSound("Fight");
        AudioManager.instance.PlaySound("Menu");
        // AudioManager.instance.StopSound("Waterfall");
    }
}