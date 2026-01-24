using UnityEngine;

namespace SamuraiStandoff
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenuUI;
        private bool isPaused;
        private bool settingsOpen;

        private void Update()
        {
            HandlePauseInput();
        }

        private void HandlePauseInput()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            // If settings are open, close them first
            // if (settingsOpen)
            // {
            //     settingsUI.SetActive(false);
            //     settingsOpen = false;
            //     return;
            // }

            // Otherwise toggle pause
            if (isPaused)
            {
                pauseMenuUI.SetActive(false);
                DuelController.instance.isPaused = false;
                isPaused = false;
            }
            else
            {
                pauseMenuUI.SetActive(true);
                DuelController.instance.isPaused = true;
                isPaused = true;
            }
        }

        public void SetSettingsOpen(bool open)
        {
            settingsOpen = open;
        }

    }
}