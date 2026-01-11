using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

namespace SamuraiStandoff
{
    public class DuelTimerController : BaseGameBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] private float minSignal = 2f;
        [SerializeField] private float maxSignal = 5f;
        
        [Header("UI Elements")]
        [SerializeField] private Slider signalSlider;
        [SerializeField] private TextMeshProUGUI framesText;
        [SerializeField] private TextMeshProUGUI timerText;

        // Public properties
        public bool signal { get; private set; }
        public float signalTime { get; private set; }
        public float enemyReactionTime { get; set; } // Set by EnemyController or multiplayer logic
        public int frames => _frames;

        // Private fields
        private float _timer;
        private int _frames;
        private int _maxFramesForSlider;
        private bool _winnerDeclared;

        private void Awake()
        {
            InitializeTimer();
        }

        private void Update()
        {
            if (_winnerDeclared)
            {
                signalSlider.gameObject.SetActive(false);
            }
            else
            {
                _timer += Time.deltaTime;

                if (_timer >= signalTime && !signal)
                {
                    TriggerSignal();
                }
            }

            UpdateFrameCounter();
        }

        private void InitializeTimer()
        {
            signalTime = Random.Range(minSignal, maxSignal);
            _frames = 0;
            signal = false;
            _winnerDeclared = false;
            
            if (signalSlider != null)
            {
                signalSlider.gameObject.SetActive(false);
                signalSlider.value = 0f;
            }
        }

        private void TriggerSignal()
        {
            AudioManager.instance.PlaySound("Signal");
            signal = true;
            
            if (signalSlider != null)
            {
                signalSlider.gameObject.SetActive(true);
                signalSlider.value = _maxFramesForSlider;
            }
        }

        private void UpdateFrameCounter()
        {
            if (signal && !_winnerDeclared)
            {
                _frames++;
                
                if (signalSlider != null)
                {
                    signalSlider.value = _maxFramesForSlider - _frames;
                }
                
                if (_frames % 3 == 0 && timerText != null)
                {
                    timerText.text = (_maxFramesForSlider - _frames).ToString();
                }
            }
            else if (signal && _winnerDeclared)
            {
                if (framesText != null)
                {
                    framesText.text = _frames.ToString(CultureInfo.CurrentCulture);
                }

                // Log best frame count for result screen
                if (playerData != null && playerData.lastBestFrameCount > _frames)
                {
                    playerData.lastBestFrameCount = _frames;

                    if (playerData.currentBestFrameCount > playerData.lastBestFrameCount)
                    {
                        playerData.currentBestFrameCount = playerData.lastBestFrameCount;
                    }
                }
            }
        }

        /// <summary>
        /// Called by EnemyController or multiplayer setup to set max frames based on reaction time
        /// </summary>
        public void SetMaxFramesForSlider()
        {
            _maxFramesForSlider = Mathf.RoundToInt(enemyReactionTime * 60f);

            if (signalSlider != null)
            {
                signalSlider.maxValue = _maxFramesForSlider;
                signalSlider.minValue = 0f;
            }
        }

        /// <summary>
        /// Notify the timer that a winner has been declared
        /// </summary>
        public void OnWinnerDeclared()
        {
            _winnerDeclared = true;
        }

        /// <summary>
        /// Reset timer for a new round (used after faults)
        /// </summary>
        public void ResetTimer()
        {
            InitializeTimer();
        }
    }
}