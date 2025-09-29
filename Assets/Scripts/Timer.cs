using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public static Timer instance;

    public float signalTime, timer;
    [SerializeField] float minSignal, maxSignal;
    [SerializeField] TextMeshProUGUI signalText;
    public bool signal;

    void Awake()
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
        signalTime = Random.Range(minSignal, maxSignal);
    }

    void Start()
    {
        signalText.enabled = false;
    }

    void Update()
    {
        if(GameController.instance.winnerDeclared != true)
        {
            timer += Time.deltaTime;

            if (timer >= signalTime)
            {
                if (signalText.enabled == false)
                {
                    AudioManager.instance.PlaySound("Signal");
                    signal = true;
                    signalText.enabled = true;
                }
            }
        }
        else
        {
            signalText.enabled = false;
        }   
    }
}