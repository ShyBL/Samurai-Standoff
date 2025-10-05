using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    [SerializeField] Animator transition;

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
    public void Loadgame() //Enter Game
    {
        GameManager.instance.StartCoroutine(LoadScene(1));

        AudioManager.instance.StopSound("Waterfall");
        AudioManager.instance.PlaySound("Waterfall");
    }

    public void Restartgame()
    {
        GameManager.instance.currentLevel = 1;
        GameManager.instance.StartCoroutine(LoadScene(1));

        AudioManager.instance.StopSound("Waterfall");
        AudioManager.instance.PlaySound("Waterfall");
    }

    public void LoadMainMenu()
    {
        GameManager.instance.currentLevel = 1;
        GameManager.instance.StartCoroutine(LoadScene(0));
    }

    public IEnumerator LoadScene(int levelIndex) //
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(levelIndex);
    }

    public IEnumerator NextLevel()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Next Level");
        transition.SetTrigger("Start");
        GameManager.instance.currentLevel++;

        if (GameManager.instance.currentLevel > GameManager.instance.totalLevels)
        {
            StartCoroutine(LoadResults());
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (TryGetComponent(out PlayerController playerController)) playerController.faultCounter = 0;
            }

            Loadgame();
        }
    }

    public IEnumerator LoadResults()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(LoadScene(2));
        AudioManager.instance.StopSound("Waterfall");
    }
}