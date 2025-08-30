using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    [SerializeField] Animator transition;

    private void Awake()
    {
        if(instance == null)
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
     public void Loadgame()//Enter Game
    {
        LevelManager.instance.StartCoroutine(LoadScene(2));

        AudioManager.instance.StopSound("Waterfall");
        AudioManager.instance.PlaySound("Waterfall");
    }
     public void Restartgame()
    {
        LevelManager.instance.currentLevel = 1;
        LevelManager.instance.StartCoroutine(LoadScene(2));

        AudioManager.instance.StopSound("Waterfall");
        AudioManager.instance.PlaySound("Waterfall");
    }
    public void LoadMainMenu(){
        LevelManager.instance.currentLevel = 1;
        LevelManager.instance.StartCoroutine(LoadScene(1));
    }

    public IEnumerator LoadScene(int levelIndex)//
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
        LevelManager.instance.currentLevel++;

        if (LevelManager.instance.currentLevel > LevelManager.instance.totalLevels)
        {
            StartCoroutine(LoadResults());
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            Player.faultCounter = 0;
            Loadgame();
        }
    }
    
    public IEnumerator LoadResults()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(LoadScene(3));
        AudioManager.instance.StopSound("Waterfall");
    }
}
