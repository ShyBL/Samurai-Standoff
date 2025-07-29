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

        //if( GameController.instance.pTwo == this)
        //{
            
        //}
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
                Debug.Log("Player Attacked");
                playerDrawn = true;
                if (Timer.instance.signal == false)
                {
                    Fault();
                }
                else
                {
                    GameController.instance.DeclareWinner(this.gameObject);
                }
            }
        }
    }

    void Fault()
    {
        faultCounter += 1;
        if(faultCounter < 2)
        {
            StartCoroutine(GameController.instance.FaultRestart());
        }

        if (faultCounter >= 2)
        {
            if (GameController.instance.pOne = this.gameObject)
            {
                GameController.instance.DeclareWinner(GameController.instance.pTwo);//change these!
            }
            else if (GameController.instance.pTwo = this.gameObject)
            {
                GameController.instance.DeclareWinner(GameController.instance.pOne);
            }
           
        }
    }
}
