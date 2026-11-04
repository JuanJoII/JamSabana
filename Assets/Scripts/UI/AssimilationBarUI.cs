using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JamSabana.Core;
using System.Collections; 

namespace JamSabana.UI
{
    public class AssimilationBarUI : MonoBehaviour
    {
        [Header("Componentes de Asimilación")]
        [SerializeField] private Slider assimilationSlider;
        [SerializeField] private TextMeshProUGUI cutePercentageText;
        [SerializeField] private TextMeshProUGUI darkPercentageText;

        [Header("Panel Gameover")]
        [SerializeField] private GameObject gameoverPanel;
        [SerializeField] private TextMeshProUGUI gameoverMessageText;

        private void OnEnable()
        {
            GameEventsB.OnAssimilationChanged += UpdateAssimilationUI;
            GameEventsB.OnMatchEnded += ShowVictoryScreenDelayed; 
        }

        private void OnDisable()
        {
            GameEventsB.OnAssimilationChanged -= UpdateAssimilationUI;
            GameEventsB.OnMatchEnded -= ShowVictoryScreenDelayed; 
        }

        private void Start()
        {
            if (gameoverPanel != null)
                gameoverPanel.SetActive(false);

            if (AssimilationManager.Instance != null)
                UpdateAssimilationUI(AssimilationManager.Instance.CurrentBalance);
            else
                UpdateAssimilationUI(0.5f);
        }

        private void UpdateAssimilationUI(float balance)
        {
            if (assimilationSlider != null)
                assimilationSlider.value = balance;

            float cuteProgress = (1f - balance) * 100f;
            float darkProgress = balance * 100f;

            if (cutePercentageText != null)
                cutePercentageText.text = $"{Mathf.RoundToInt(cuteProgress)}%";

            if (darkPercentageText != null)
                darkPercentageText.text = $"{Mathf.RoundToInt(darkProgress)}%";
        }
        
        private void ShowVictoryScreenDelayed(int winnerPlayer)
        {
            StartCoroutine(VictoryScreenCoroutine(winnerPlayer));
        }

        private IEnumerator VictoryScreenCoroutine(int winnerPlayer)
        {
            yield return new WaitForSecondsRealtime(2f); 
            ShowVictoryScreen(winnerPlayer);
        }

        private void ShowVictoryScreen(int winnerPlayer)
        {
            if (gameoverPanel != null)
                gameoverPanel.SetActive(true);

            if (gameoverMessageText != null)
            {
                string teamName = "Desconocido";
                if (GameManager.Instance != null)
                {
                    PlayerTeam winnerTeam = (winnerPlayer == 1) ? GameManager.Instance.Player1Team : GameManager.Instance.Player2Team;
                    teamName = winnerTeam == PlayerTeam.Cute ? "Fluffy" : "Sour Pop";
                }

                gameoverMessageText.text = $"¡Player {winnerPlayer} wins!\nWorld assimilated by the faction {teamName}";
            }

            Time.timeScale = 0f;
        }
    }
}