using System.Globalization;
using UnityEngine;
using TMPro;

public class FrameSign : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI signGUI;
    private float _frames;

    private void Update()
    {
        if(Timer.instance.signal == true && GameController.instance.winnerDeclared == false)
        {
            _frames++;
        }
        else
        {
            signGUI.text = _frames.ToString(CultureInfo.CurrentCulture);
        }
    }
}