using UnityEngine;

public class ResultsScreen : MonoBehaviour
{
    LevelManager levelController;
    [SerializeField]GameObject soloText, multiText;

    [SerializeField] float topTime;
    [SerializeField] int wins;
    void awake(){
        levelController = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelManager>();
    }

    void Start(){
        if (LevelManager.instance.multiplayer == false){
            soloText.SetActive(true);
        }
        else{
            multiText.SetActive(false);
        }


    }
}