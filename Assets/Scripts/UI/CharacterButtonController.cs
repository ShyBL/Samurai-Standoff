using System;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class CharacterButtonController : MonoBehaviour
    {
        public CharacterType characterType;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private GameObject characterPortrait;

        private void Start()
        {
            var unlocked = GameManager.instance.IsCharacterUnlocked(characterType);
            var button = GetComponent<Button>();
            button.interactable = unlocked;
            SetLockedVisual(!unlocked);
        }

        private void SetLockedVisual(bool isLocked)
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
