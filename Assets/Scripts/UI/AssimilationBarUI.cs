using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JamSabana.Core;

namespace JamSabana.UI
{
    /// <summary>
    /// Controlador de UI que maneja la visualización de la barra de asimilación central.
    /// Sincroniza el Slider, los textos de porcentaje de cada bando y activa la pantalla de fin de partida.
    /// </summary>
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
            GameEventsB.OnMatchEnded += ShowVictoryScreen;
        }

        private void OnDisable()
        {
            GameEventsB.OnAssimilationChanged -= UpdateAssimilationUI;
            GameEventsB.OnMatchEnded -= ShowVictoryScreen;
        }

        private void Start()
        {
            if (gameoverPanel != null)
            {
                gameoverPanel.SetActive(false);
            }

            if (AssimilationManager.Instance != null)
            {
                UpdateAssimilationUI(AssimilationManager.Instance.CurrentBalance);
            }
            else
            {
                UpdateAssimilationUI(0.5f);
            }
        }

        /// <summary>
        /// Sincroniza el valor del Slider con el balance general y calcula los porcentajes de Cute y Dark.
        /// </summary>
        private void UpdateAssimilationUI(float balance)
        {
            if (assimilationSlider != null)
            {
                assimilationSlider.value = balance;
            }

            float cuteProgress = (1f - balance) * 100f;
            float darkProgress = balance * 100f;

            if (cutePercentageText != null)
            {
                cutePercentageText.text = $"{Mathf.RoundToInt(cuteProgress)}%";
            }

            if (darkPercentageText != null)
            {
                darkPercentageText.text = $"{Mathf.RoundToInt(darkProgress)}%";
            }
        }

        /// <summary>
        /// Activa la pantalla de fin de partida, mostrando al jugador ganador y la facción que representa.
        /// </summary>
        private void ShowVictoryScreen(int winnerPlayer)
        {
            if (gameoverPanel != null)
            {
                gameoverPanel.SetActive(true);
            }

            if (gameoverMessageText != null)
            {
                string teamName = "Desconocido";
                if (GameManager.Instance != null)
                {
                    PlayerTeam winnerTeam = (winnerPlayer == 1) ? GameManager.Instance.Player1Team : GameManager.Instance.Player2Team;
                    teamName = winnerTeam == PlayerTeam.Cute ? "Colorido (Cute)" : "Oscuro (Dark)";
                }

                gameoverMessageText.text = $"¡Jugador {winnerPlayer} Gana!\nMundo asimilado por la facción {teamName}";
            }
        }
    }
}
