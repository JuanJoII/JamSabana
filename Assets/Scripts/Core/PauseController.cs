using UnityEngine;
using UnityEngine.SceneManagement;
using JamSabana.Core;

public class PauseController : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Canvas o panel principal del menú de pausa que se mostrará/ocultará.")]
    public GameObject pauseMenuCanvas;

    [Header("Escenas")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPauseMenuOpen = false;
    public bool IsPauseMenuOpen => isPauseMenuOpen;

    private void Start()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }
        isPauseMenuOpen = false;
    }

    /// <summary>
    /// Método expuesto para abrir o cerrar el menú de pausa.
    /// Listo para ser llamado desde el Input System o desde botones de la UI.
    /// </summary>
    public void TogglePause()
    {
        if (isPauseMenuOpen)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Abre el menú de pausa y pausa la partida mediante el evento global OnGamePaused.
    /// </summary>
    public void PauseGame()
    {
        // Si la partida ya terminó o ya está pausada por otro sistema (ej. el Tutorial inicial), no abrir encima
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState == GameState.GameOver) return;
            if (GameManager.Instance.CurrentState == GameState.Paused && !isPauseMenuOpen) return;
        }

        isPauseMenuOpen = true;

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
        }

        GameEventsB.TriggerGamePaused(true);
    }

    /// <summary>
    /// Cierra el menú de pausa y reanuda la partida. Conectar al botón "Reanudar / Continuar".
    /// </summary>
    public void ResumeGame()
    {
        if (!isPauseMenuOpen) return;

        isPauseMenuOpen = false;

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        GameEventsB.TriggerGamePaused(false);
    }

    /// <summary>
    /// Reinicia la partida actual recargando la escena activa y restableciendo el estado de pausa.
    /// Conectar al botón "Reiniciar / Restart".
    /// </summary>
    public void RestartGame()
    {
        isPauseMenuOpen = false;
        GameEventsB.TriggerGamePaused(false);
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartMatch();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    /// <summary>
    /// Alias de RestartGame para conectar desde la UI.
    /// </summary>
    public void RestartMatch() => RestartGame();

    /// <summary>
    /// Regresa al menú principal asegurando que el tiempo del juego se restablezca.
    /// Conectar al botón "Salir al Menú / Main Menu".
    /// </summary>
    public void ReturnToMainMenu()
    {
        isPauseMenuOpen = false;
        GameEventsB.TriggerGamePaused(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
