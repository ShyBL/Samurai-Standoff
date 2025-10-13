using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine.UI;

public class DuelController : MonoBehaviour
{
    #region Singleton

    public static DuelController instance;
    [SerializeField] private GameData gameData;

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

        winnerDeclared = false;
        TimerInit();
    }

    #endregion

    #region Timer Logic
    [SerializeField] private float minSignal, maxSignal;
    [SerializeField] private Slider signalSlider; // Radial slider for visual feedback
    [SerializeField] private TextMeshProUGUI framesText;
    
    private float _timer;
    private int _frames;
    private int _maxFramesForSlider; // Calculated based on enemy reaction time
    
    public bool signal;
    public float signalTime;
    public float enemyReactionTime; // Set by EnemyController
    
    private void Update()
    {
        if (winnerDeclared)
        {
            signalSlider.gameObject.SetActive(false);
        }
        else
        {
            _timer += Time.deltaTime;

            if (_timer >= signalTime)
            {
                if (!signal)
                {
                    AudioManager.instance.PlaySound("Signal");
                    signal = true;
                    signalSlider.gameObject.SetActive(true);
                    signalSlider.value = 0f;
                }
            }
        }

        switch (signal)
        {
            case true when !winnerDeclared:
                _frames++;
                // Update slider based on frame count directly
                signalSlider.value = _frames;
                break;
            case true when winnerDeclared:
                framesText.text = _frames.ToString(CultureInfo.CurrentCulture);
                
                // Log best frame count for result screen
                if (playerData.lastBestFrameCount > _frames)
                {
                    playerData.lastBestFrameCount = _frames;
                    
                    if (playerData.currentBestFrameCount > playerData.lastBestFrameCount)
                    {
                        playerData.currentBestFrameCount = playerData.lastBestFrameCount;
                    }
                   
                }
                break;
        }
    }
    
    private void TimerInit()
    {
        signalTime = Random.Range(minSignal, maxSignal);
        // Zero out best time for result screen
        _frames = 0;
        //playerData.lastBestFrameCount = _frames;
    }
    
    // Called by EnemyController to set the max frames based on enemy reaction time
    public void SetMaxFramesForSlider()
    {
        // Convert enemy reaction time (in seconds) to frames (assuming 60 FPS)
        _maxFramesForSlider = Mathf.RoundToInt(enemyReactionTime * 60f);
        
        // Set the slider's max value to match
        signalSlider.maxValue = _maxFramesForSlider;
        signalSlider.minValue = 0f;
    }
    
    #endregion
    
    #region Game State
    [SerializeField] private PlayerData playerData;
    public GameObject pOne, pTwo;
    public bool winnerDeclared;
    public bool playerFault;

    [Header("UI Elements")] [SerializeField]
    private TextMeshProUGUI resultText;
    
    private void Start()
    { 
        signalSlider.gameObject.SetActive(false);
        signalSlider.value = 0f;
        resultText.enabled = false;

        // Assign player references
        var players = GameObject.FindGameObjectsWithTag("Player");
        pOne = players[0];
        pTwo = players[1];
    }

    #endregion

    #region Win Logic

    /// <summary>
    /// Declares the winner, records progression stats, and triggers scene transitions.
    /// A win can be triggered by a fault, which is tracked for stats.
    /// </summary>
    /// <param name="winner">The GameObject of the winner.</param>
    /// <param name="winByFault">Set to true if the win was caused by the opponent's second fault.</param>
    public void DeclareWinner(GameObject winner, bool winByFault = false)
    {
        AudioManager.instance.PlaySound("Clash");
        SceneLoader.instance.Clash();

        if (!winnerDeclared)
        {
            winnerDeclared = true;

            GameObject loser = (winner == pOne) ? pTwo : pOne;

            if (winByFault)
            {
                // This call handles the second, loss-inducing fault. The first is in FaultRestart.
                GameManager.instance.OnEarlyAttack();
            }

            if (!gameData.isMultiplayer) // Single Player Logic
            {
                if (winner.GetComponent<PlayerController>() != null) // Player wins
                {
                    GameManager.instance.OnDuelWon(_frames, loser.name);

                    // Check for difficulty completion after winning the final level
                    if (playerData.currentLevel >= GameManager.instance.totalLevels)
                    {
                        string difficulty = "";
                        switch (gameData.currentDifficulty)
                        {
                            case EnemyDifficultyType.EasyMode:
                                difficulty = "easy";
                                break;
                            case EnemyDifficultyType.MediumMode:
                                difficulty = "medium";
                                break;
                            case EnemyDifficultyType.HardMode:
                                difficulty = "hard";
                                break;
                        }
                        if (!string.IsNullOrEmpty(difficulty))
                        {
                            GameManager.instance.OnDifficultyCompleted(difficulty);
                        }
                    }
                }
                else // AI wins
                {
                    GameManager.instance.OnDuelLost();
                }
            }
            else // Multiplayer Logic
            {
                // Assuming progression is tracked from the perspective of Player 1 (pOne).
                if (winner == pOne)
                {
                    GameManager.instance.OnDuelWon(_frames, loser.name);
                }
                else
                {
                    GameManager.instance.OnDuelLost();
                }
            }

            resultText.enabled = true;
            resultText.text = $"{winner.name} Wins!";
        }

        if (winner.GetComponent<PlayerController>() != null)
        {
            playerData.lastBestFrameCount = 10000;
            StartCoroutine(SceneLoader.instance.NextLevel());
        }
        else
        {
            StartCoroutine(SceneLoader.instance.LoadResults());
        }
    }

    #endregion

    #region Fault Logic

    // Handles fault scenario and restarts round if needed.
    public IEnumerator FaultRestart()
    {
        // Track the first fault as an early attack.
        GameManager.instance.OnEarlyAttack();

        winnerDeclared = true;
        resultText.enabled = true;
        resultText.text = "Fault";

        yield return new WaitForSeconds(3f);
        RestartRound();
    }

    // Reloads the current scene to restart the round.
    private void RestartRound()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion
}