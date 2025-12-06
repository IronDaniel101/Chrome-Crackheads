using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mainMenuCanvas;
    public GameObject controlsCanvas;

    [Header("Scene To Load")]
    public string levelSceneName = "LevelScene"; // Change this to your scene name

    // Called by Start Game button
    public void StartGame()
    {
        SceneManager.LoadScene(levelSceneName);
    }

    // Called by Quit Game button
    public void QuitGame()
    {
        Application.Quit();

        // Helps confirm quitting during Editor play mode
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }

    // Show the controls screen
    public void ShowControls()
    {
        mainMenuCanvas.SetActive(false);
        controlsCanvas.SetActive(true);
    }

    // Return back to main menu
    public void BackToMenu()
    {
        controlsCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }
}
