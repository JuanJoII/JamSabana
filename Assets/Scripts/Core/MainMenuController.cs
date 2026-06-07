// Responsibility: Manage the main menu, credits, and navigation between scenes.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Escenas")]
    public string gameSceneName = "Game";

    [Header("Referencias UI")]
    public GameObject creditsPanel;
    public TextMeshProUGUI pressToStartText;

    [Header("Animación texto")]
    public float pulseSpeed = 2f;
    public float pulseMinAlpha = 0.2f;

    private bool creditsOpen = false;

    private void Start()
    {
        creditsPanel.SetActive(false);
    }

    private void Update()
    {
        PulseText();
    }

    private void PulseText()
    {
        float alpha = Mathf.Lerp(pulseMinAlpha, 1f,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        Color c = pressToStartText.color;
        c.a = alpha;
        pressToStartText.color = c;
    }
    
    public void OnConfirm(InputValue value)
    {
        if (!value.isPressed) return;

        if (creditsOpen)
            CloseCredits();
        else
            StartGame();
    }

    public void OnCredits(InputValue value)
    {
        if (!value.isPressed) return;

        if (creditsOpen)
            CloseCredits();
        else
            OpenCredits();
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OpenCredits()
    {
        creditsOpen = true;
        creditsPanel.SetActive(true);
    }

    private void CloseCredits()
    {
        creditsOpen = false;
        creditsPanel.SetActive(false);
    }
}