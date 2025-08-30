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
        
        // Subscribe to signal event
        EventsManager.instance.Subscribe(GameEventType.SignalTriggered, OnSignalTriggered);
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        EventsManager.instance.Unsubscribe(GameEventType.SignalTriggered, OnSignalTriggered);
    }
    
    private void OnSignalTriggered(GameEvent gameEvent)
    {
        // Enemy can now react to the signal
        // This could be used for visual feedback or other logic
    }
    
    public void GetTraits()
    {
        Debug.Log("Called");
        int listLevel = LevelManager.instance.currentLevel - 1;

        reactionTimer = enemyStats.currentDifficulty switch
        {
            EnemyDifficultyType.EasyMode => enemyStats.easy[listLevel],
            EnemyDifficultyType.MediumMode => enemyStats.medium[listLevel],
            EnemyDifficultyType.HardMode => enemyStats.hard[listLevel],
            _ => throw new ArgumentOutOfRangeException()
        };
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
                
                // Publish enemy drawn event
                EventsManager.instance.Publish(GameEventType.EnemyDrawn, this.gameObject, this.gameObject);
            }
        }
    }
}