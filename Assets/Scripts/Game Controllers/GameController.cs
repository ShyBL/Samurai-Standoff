using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public GameObject pOne, pTwo;

    public bool winnerDeclared;
    [SerializeField] TextMeshProUGUI resultText;

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
        winnerDeclared = false;
    }

    private void Start()
    {
        resultText.enabled = false;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        pOne = players[0];
        pTwo = players[1];
        
        // Subscribe to events
        EventsManager.instance.Subscribe(GameEventType.PlayerDrawn, OnPlayerDrawn);
        EventsManager.instance.Subscribe(GameEventType.EnemyDrawn, OnEnemyDrawn);
        EventsManager.instance.Subscribe(GameEventType.PlayerFault, OnPlayerFault);
        
        // Publish round started event
        EventsManager.instance.Publish(GameEventType.RoundStarted);
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        EventsManager.instance.Unsubscribe(GameEventType.PlayerDrawn, OnPlayerDrawn);
        EventsManager.instance.Unsubscribe(GameEventType.EnemyDrawn, OnEnemyDrawn);
        EventsManager.instance.Unsubscribe(GameEventType.PlayerFault, OnPlayerFault);
    }
    
    private void OnPlayerDrawn(GameEvent gameEvent)
    {
        // Player drew after signal - they win!
        DeclareWinner(gameEvent.sender);
    }
    
    private void OnEnemyDrawn(GameEvent gameEvent)
    {
        // Enemy drew after signal - they win!
        DeclareWinner(gameEvent.sender);
    }
    
    private void OnPlayerFault(GameEvent gameEvent)
    {
        // Handle player fault logic if needed
        int faultCount = (int)gameEvent.data;
        Debug.Log($"Player faulted! Fault count: {faultCount}");
    }

    public void DeclareWinner(GameObject Winner)
    {
        winnerDeclared = true;
        
        // Publish winner declared event
        EventsManager.instance.Publish(GameEventType.WinnerDeclared, Winner, Winner);
        
        AudioManager.instance.PlaySound("Clash");
        SceneLoader.instance.Clash();

        resultText.enabled = true;
        resultText.text = Winner.name + " Wins!";

        if (Winner.GetComponent<Player>() != null)
        {
            StartCoroutine(SceneLoader.instance.NextLevel());
        }
        else
        {
            StartCoroutine(SceneLoader.instance.LoadResults());
        }
    }

    public IEnumerator FaultRestart()
    {
        winnerDeclared = true;

        resultText.enabled = true;
        resultText.text = "Fault";
        
        // Publish fault restart event
        EventsManager.instance.Publish(GameEventType.FaultRestart);
        
        yield return new WaitForSeconds(3f);
        Round2();
    }

    public void Round2()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}