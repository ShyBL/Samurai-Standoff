using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EnemyDifficultyType
{
    EasyMode, MediumMode, HardMode
}

[CreateAssetMenu(menuName = "Enemy Difficulty") ]
public class EnemyDifficulty : ScriptableObject
{
    [SerializeField] public EnemyDifficultyType currentDifficulty;

    [SerializeField] public float[] easy = new float [4];
    [SerializeField] public float[] medium = new float[4];
    [SerializeField] public float[] hard = new float[5];

    [SerializeField] Sprite[] characters; 
    public List<float> reactionFrames = new List<float>();

    public List<Sprite> enemySprites = new List<Sprite>();

    //Buttons
    public void EasyMode()
    {
        LevelManager.instance.totalLevels = 4;
        currentDifficulty = EnemyDifficultyType.EasyMode;
    }

    public void MediumMode()
    {
        LevelManager.instance.totalLevels = 4;
        currentDifficulty = EnemyDifficultyType.MediumMode;
    }

    public void HardMode()
    {
        LevelManager.instance.totalLevels = 5;
        currentDifficulty = EnemyDifficultyType.HardMode;
    }
}
