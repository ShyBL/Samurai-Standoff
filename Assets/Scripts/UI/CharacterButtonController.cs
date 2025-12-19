using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class CharacterButtonController : MonoBehaviour
    {
        public CharacterType characterType;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private GameObject characterPortrait;

        public void SetLockedVisual(bool isLocked)
        {
            if (lockOverlay != null)
            {
                lockOverlay.SetActive(isLocked);
                
                if (isLocked == false)
                {
                    characterPortrait.GetComponent<Image>().color = new Color(255f, 255f, 255f, 1f);
                }
                
            }
        }

    }
}
