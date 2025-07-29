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
        reactionTimer = enemyStats.reactionFrames[listLevel];
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
