using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    public static bool isPaused;

    private PlayerControls controls;

    private void Awake()
    {
        // Create one instance of your input actions
        controls = new PlayerControls();

        // Subscribe once to the Pause action
        controls.Player.Pause.performed += ctx => TogglePause();
    }

    private void OnEnable()
    {
        // Enable action map so it listens for key presses
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        // Disable actions when object is inactive
        controls.Player.Disable();
    }

    private void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // Always unpause before scene switch
        SceneManager.LoadScene(sceneName);
    }
}