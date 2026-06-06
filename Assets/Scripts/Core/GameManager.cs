using JamSabana.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamSabana.Core
{
    /// <summary>
    /// Administrador general del flujo y ciclo de vida de la partida.
    /// Controla la inicialización de la partida, la asignación aleatoria de equipos (Cute/Dark),
    /// la detección del ganador mediante eventos y el reinicio de la escena.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        public GameState CurrentState { get; private set; } = GameState.Setup;

        public PlayerTeam Player1Team { get; private set; }
        public PlayerTeam Player2Team { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            AssignRandomTeams();
        }

        private void Start()
        {
            CurrentState = GameState.Playing;
            Debug.Log("[GameManager] Partida Iniciada.");
        }

        private void AssignRandomTeams()
        {
            if (Random.Range(0, 2) == 0)
            {
                Player1Team = PlayerTeam.Cute;
                Player2Team = PlayerTeam.Dark;
            }
            else
            {
                Player1Team = PlayerTeam.Dark;
                Player2Team = PlayerTeam.Cute;
            }

            Debug.Log($"[GameManager] Roles Asignados -> P1: {Player1Team} | P2: {Player2Team}");
        }

        private void OnEnable()
        {
            GameEventsB.OnWorldFullyAssimilated += HandleWorldFullyAssimilated;
        }

        private void OnDisable()
        {
            GameEventsB.OnWorldFullyAssimilated -= HandleWorldFullyAssimilated;
        }

        /// <summary>
        /// Procesa el fin de la partida cuando el mundo de un jugador es totalmente asimilado.
        /// Determina qué jugador físico (1 o 2) ganó y dispara el evento de finalización.
        /// </summary>
        private void HandleWorldFullyAssimilated(PlayerTeam losingTeam)
        {
            if (CurrentState != GameState.Playing) return;

            CurrentState = GameState.GameOver;

            PlayerTeam winningTeam = (losingTeam == PlayerTeam.Cute) ? PlayerTeam.Dark : PlayerTeam.Cute;
            int winningPlayer = (Player1Team == winningTeam) ? 1 : 2;

            Debug.Log($"[GameManager] ¡Fin de la partida! El mundo de {losingTeam} ha sido destruido. Ganador: Jugador {winningPlayer}");

            GameEventsB.TriggerMatchEnded(winningPlayer);
        }

        public void RestartMatch()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}