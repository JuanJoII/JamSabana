using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu";

    public void OnCredits(InputValue value)
    {
        if (!value.isPressed) return;
        ReturnToMainMenu();
    }
    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}