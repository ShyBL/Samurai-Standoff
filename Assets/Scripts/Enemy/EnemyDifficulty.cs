using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Enemy Difficulty") ]
public class EnemyDifficulty : ScriptableObject
{
    [SerializeField] public float[] easy = new float [4], 
        medium = new float[4], 
        hard = new float[5];

    [SerializeField] Sprite[] characters; 
    public List<float> reactionFrames = new List<float>();

    public List<Sprite> enemySprites = new List<Sprite>();

    //Buttons
    public void EasyMode()
    {
        reactionFrames.Clear();
        //Sets total levels
        LevelManager.instance.totalLevels = 4;
        //Moves Easy array into reaction frames list
        for (int i = 0; i < easy.Length; i++)
        {
             reactionFrames.Add(easy[i]);
        }
    }

    public void MediumMode()
    {
        reactionFrames.Clear();
        LevelManager.instance.totalLevels = 4;

        for (int i = 0; i <  medium.Length; i++)
        {
            reactionFrames.Add(medium[i]);
        }
    }

    public void HardMode()
    {
        reactionFrames.Clear();
        LevelManager.instance.totalLevels = 5;

        for (int i = 0; i < hard.Length; i++)
        {
            reactionFrames.Add(hard[i]);
        }
    }
    
}
