using UnityEngine;
using TMPro;

public class FrameSign : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI SignGUI;
    float frames;

    private void Update()
    {
        if(Timer.instance.signal == true && GameController.instance.winnerDeclared == false)
        {
            frames++;
        }
        else
        {
            SignGUI.text = frames.ToString();
        }
    }
}
