//Script created by Anthony Mota
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverScreen;
    public GameObject restartButton;
    public GameObject returnToMainMenuButton;
    public GameObject startGameButton; 
    public string gameSceneName = "Level1.0"; 
    public GameObject exitGameButton;

    // Show Game Over screen and freeze game
    public void ShowGameOverScreen()
    {
        gameOverScreen.SetActive(true);
        restartButton.SetActive(true);
        returnToMainMenuButton.SetActive(true);

        Time.timeScale = 0f; // Freeze the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Restart the current scene and unfreeze game
    public void RestartGame()
    {
        Time.timeScale = 1f; // Unfreeze game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Go to main menu and unfreeze game
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Unfreeze game
        SceneManager.LoadScene("MainMenu");
    }
    
    public void StartGame()
    {
        Time.timeScale = 1f; // Ensure game runs
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ExitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    
}
