using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] bool enemyDrawn;
    [SerializeField] float attackTimer;
    public float reactionTimer;
    SpriteRenderer sr;

    [SerializeField] EnemyDifficulty enemyStats;
    void Start()
    {
        GetTraits();
        RestartAttackTimer();
    }
     public void GetTraits()
    {
        Debug.Log("Called");
        int listLevel = LevelManager.instance.currentLevel - 1;

        //sr.sprite = characters[listLevel];

        //Apply To Boss
        reactionTimer = enemyStats.currentDifficulty switch
        {
            EnemyDifficultyType.EasyMode => enemyStats.easy[listLevel],
            EnemyDifficultyType.MediumMode => enemyStats.medium[listLevel],
            EnemyDifficultyType.HardMode => enemyStats.hard[listLevel],
            _ => throw new ArgumentOutOfRangeException()
        };

        //Boss.GetComponent<SpriteRenderer>().sprite = sr.sprite;
    } 

    void RestartAttackTimer()
    {
        attackTimer = Timer.instance.signalTime + reactionTimer;
    }
    
    void Update()
    {
        if(GameController.instance.winnerDeclared != true)
        {
            if (enemyDrawn == false && attackTimer != 0)
            {
                attackTimer -= Time.deltaTime;
            }
            if (enemyDrawn == false && attackTimer <= 0)
            {
                Debug.Log("AI Attacked");
                enemyDrawn = true;
                GameController.instance.DeclareWinner(this.gameObject);
            }
        }
    }
}
