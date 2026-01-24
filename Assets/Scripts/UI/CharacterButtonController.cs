using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamuraiStandoff
{
    public class CharacterButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public CharacterType characterType;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private GameObject characterPortrait;
        [SerializeField] private GameObject hoverObject;

        private void Awake()
        {
            // Ensure the hover object starts inactive
            if (hoverObject != null)
            {
                hoverObject.SetActive(false);
            }
        }
        private void Start()
        {
            var unlocked = GameManager.instance.IsCharacterUnlocked(characterType);
            var button = GetComponent<Button>();
            button.interactable = unlocked;
            SetLockedVisual(!unlocked);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverObject != null)
            {
                hoverObject.SetActive(true);
            }
        }
    
        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverObject != null)
            {
                hoverObject.SetActive(false);
            }
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
