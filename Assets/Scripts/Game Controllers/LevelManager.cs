using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager: MonoBehaviour
{
    [SerializeField] private List<Button> buttons;
    public EnemyDifficultyType currentDifficulty;
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

    // Buttons
    public void EasyMode()
    {
        totalLevels = 4;
        currentDifficulty = EnemyDifficultyType.EasyMode;
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
    }

    public void MediumMode()
    {
        totalLevels = 4;
        currentDifficulty = EnemyDifficultyType.MediumMode;
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
    }

    public void HardMode()
    {
        totalLevels = 5;
        currentDifficulty = EnemyDifficultyType.HardMode;
        foreach (Button button in buttons)
        {
            button.interactable = false;
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