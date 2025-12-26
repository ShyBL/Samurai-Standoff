using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsUI; 
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
        if (settingsOpen)
        {
            settingsUI.SetActive(false);
            settingsOpen = false;
            return;
        }

        // Otherwise toggle pause
        if (isPaused)
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }
        else
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }
    }
    
    public void SetSettingsOpen(bool open)
    {
        settingsOpen = open;
    }

}