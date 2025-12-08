using UnityEngine;
using UnityEngine.SceneManagement;

public class LossPopupUI : MonoBehaviour
{
    // Called by Restart button
    public void RestartGame()
    {
        Time.timeScale = 1f;
        PoliceHandler.ResetGameState();

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    // Called by Return to Menu button
    public void ReturnToMenu(string menuSceneName)
    {
        Time.timeScale = 1f;
        PoliceHandler.ResetGameState();

        SceneManager.LoadScene(menuSceneName);
    }

    private void OnEnable()
    {
        // If you want gameplay to freeze when popup appears:
        Time.timeScale = 0f;
    }
}