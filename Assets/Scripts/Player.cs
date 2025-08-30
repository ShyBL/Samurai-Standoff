using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField] bool playerDrawn;

    public static int faultCounter;
    [SerializeField] int faults;
    [SerializeField] TextMeshProUGUI faultUI;

    SpriteRenderer sr;

    void Start()
    {
        playerDrawn = false;
        
        // Subscribe to signal event
        EventsManager.instance.Subscribe(GameEventType.SignalTriggered, OnSignalTriggered);
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        EventsManager.instance.Unsubscribe(GameEventType.SignalTriggered, OnSignalTriggered);
    }

    void Update()
    {
        if (faultUI.enabled == true)
        {
            faults = faultCounter;
            if (faultCounter >= 1)
            {
                faultUI.enabled = true;
            }
            else
            {
                faultUI.enabled = false;
            }
        }

        if (Input.GetKeyDown("space") && playerDrawn == false)
        {
            if(GameController.instance.winnerDeclared != true)
            {
                playerDrawn = true;
                
                if (Timer.instance.signal == false)
                {
                    Fault();
                }
                else
                {
                    // Publish player drawn event
                    EventsManager.instance.Publish(GameEventType.PlayerDrawn, this.gameObject, this.gameObject);
                }
            }
        }
    }
    
    private void OnSignalTriggered(GameEvent gameEvent)
    {
        // Player can now react to the signal
        // This could be used for visual feedback or other logic
    }

    void Fault()
    {
        faultCounter += 1;
        
        // Publish player fault event
        EventsManager.instance.Publish(GameEventType.PlayerFault, faultCounter, this.gameObject);
        
        if(faultCounter < 2)
        {
            StartCoroutine(GameController.instance.FaultRestart());
        }

        if (faultCounter >= 2)
        {
            if (GameController.instance.pOne == this.gameObject)
            {
                GameController.instance.DeclareWinner(GameController.instance.pTwo);
            }
            else if (GameController.instance.pTwo == this.gameObject)
            {
                GameController.instance.DeclareWinner(GameController.instance.pOne);
            }
        }
    }
}