// Responsibility: Manage the main menu, credits, and navigation between scenes via UI buttons.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Escenas")]
    public string gameSceneName = "Level1";

    [Header("Referencias UI")]
    public GameObject creditsPanel;

    [Header("Botones (Opcional si se conectan por OnClick)")]
    public Button playButton;
    public Button creditsButton;
    public Button exitButton;
    public Button closeCreditsButton;

    private bool creditsOpen = false;

    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(ToggleCredits);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (closeCreditsButton != null)
            closeCreditsButton.onClick.AddListener(CloseCredits);
    }

    private void Start()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        creditsOpen = false;
    }

    /// <summary>
    /// Inicia la partida cargando la escena configurada. Conectar al botón Play.
    /// </summary>
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Alias de PlayGame por compatibilidad en el Inspector.
    /// </summary>
    public void StartGame() => PlayGame();

    /// <summary>
    /// Alterna el panel de créditos (abre si está cerrado, cierra si está abierto). Conectar al botón Credits.
    /// </summary>
    public void ToggleCredits()
    {
        if (creditsOpen)
            CloseCredits();
        else
            OpenCredits();
    }

    /// <summary>
    /// Abre el panel de créditos.
    /// </summary>
    public void OpenCredits()
    {
        creditsOpen = true;
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Cierra el panel de créditos. Conectar al botón de cerrar dentro de PanelCreditos.
    /// </summary>
    public void CloseCredits()
    {
        creditsOpen = false;
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Cierra la aplicación. Conectar al botón Exit.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
