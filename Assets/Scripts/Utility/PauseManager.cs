//Script Created by Anthony Mota
//Ah nvm you guys are using a different kind of input
//if an engineer sees this can just change the getkeydown to whatever input we currently using 
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;  // Assign your pause menu UI GameObject in the Inspector
    private bool isPaused = false;

    private InputManager inputManager;

    void Start()
    {
        inputManager = GameObject.FindAnyObjectByType<InputManager>();
    }

    void Update()
    {
        // Toggle pause when pressing the Escape key
        if (inputManager.PauseInput)
        {
            if (isPaused)
                Resume();
            else
                Pause();
            inputManager.PauseInput = false;
        }
    }

    // Call this to pause the game
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }

    // Call this to resume the game
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Unfreeze the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        isPaused = false;
    }

    // Optional: Call this to quit the game from pause menu
    public void QuitGame()
    {
        Time.timeScale = 1f; // Reset timescale before quitting
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}

