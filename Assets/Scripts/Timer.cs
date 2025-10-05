using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class Timer : MonoBehaviour
{
    public static Timer instance;
    [SerializeField] private PlayerData playerData;
    
    [SerializeField] private float minSignal, maxSignal;
    [SerializeField] private TextMeshProUGUI signalText;
    [SerializeField] private TextMeshProUGUI framesText;
    
    private float _timer;
    private float _frames;
    private float _lastFrameCount;
    
    public bool signal;
    public float signalTime;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        signalTime = Random.Range(minSignal, maxSignal);

        // Zero out best time for result screen
        _frames = 0f;
        //playerData.lastBestFrameCount = _frames;
    }

    private void Start()
    {
        signalText.enabled = false;
    }

    private void Update()
    {
        if (GameController.instance.winnerDeclared != true)
        {
            _timer += Time.deltaTime;

            if (_timer >= signalTime)
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

        switch (signal)
        {
            case true when !GameController.instance.winnerDeclared:
                _frames++;
                break;
            case true when GameController.instance.winnerDeclared:
                framesText.text = _frames.ToString(CultureInfo.CurrentCulture);
                
                // Log best frame count for result screen
                if (_lastFrameCount > _frames)
                {
                    _lastFrameCount = _frames;
                    playerData.lastBestFrameCount = _frames;
                }
                break;
        }
    }
}