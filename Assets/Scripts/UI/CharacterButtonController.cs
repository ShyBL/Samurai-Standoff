using UnityEngine;

namespace SamuraiStandoff
{
    public class CharacterButtonController : MonoBehaviour
    {
        public CharacterType characterType;
        [SerializeField] private GameObject lockOverlay;

        public void SetLockedVisual(bool isLocked)
        {
            if (lockOverlay != null)
            {
                lockOverlay.SetActive(isLocked);
            }
        }

    }
}
