using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class DuelController : MonoBehaviour
{
    #region Singleton

    public static DuelController instance;

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
    }

    #endregion

    #region Game State

    public GameObject pOne, pTwo;
    public bool winnerDeclared;

    [Header("UI Elements")] [SerializeField]
    private TextMeshProUGUI resultText;

    private void Start()
    {
        resultText.enabled = false;

        // Assign player references
        var players = GameObject.FindGameObjectsWithTag("Player");
        pOne = players[0];
        pTwo = players[1];
    }

    #endregion

    #region Win Logic

    // Declares the winner and triggers appropriate scene transitions.
    public void DeclareWinner(GameObject winner)
    {
        AudioManager.instance.PlaySound("Clash");
        SceneLoader.instance.Clash();

        if (!winnerDeclared)
        {
            winnerDeclared = true;
            resultText.enabled = true;
            resultText.text = $"{winner.name} Wins!";
        }

        if (winner.GetComponent<PlayerController>() != null)
        {
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