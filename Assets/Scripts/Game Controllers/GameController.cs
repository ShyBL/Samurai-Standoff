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
    }
    public void DeclareWinner(GameObject Winner)
    {
        AudioManager.instance.PlaySound("Clash");
        SceneLoader.instance.Clash();

        if(winnerDeclared == false)
        {
            winnerDeclared = true;
            resultText.enabled = true;
            resultText.text = Winner.name + " Wins!";
        }

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
        yield return new WaitForSeconds(3f);
        Round2();
    }

    public void Round2()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
