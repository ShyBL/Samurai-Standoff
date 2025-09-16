using System;
using System.Collections;
using UnityEngine;

public class LevelManager: MonoBehaviour
{
    public static LevelManager instance;
    public int totalLevels;
    public int currentLevel = 1;

   

    public bool multiplayer;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }      
    }

    public void ToggleMultiplayer(){
        if(multiplayer != true){
            multiplayer = true;
        }
        else{
            multiplayer = false;
        }
    }
    
    public void ApplicationQuit()
    {
        Application.Quit();
    }
    
    private void OnApplicationQuit()
    {
        
    }
}
