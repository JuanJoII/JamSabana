using JamSabana.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamSabana.Core
{
    /// <summary>
    /// Administrador general del flujo y ciclo de vida de la partida.
    /// Controla la inicialización de la partida, la asignación de equipos (Cute/Dark),
    /// el estado de pausa global (Tutorial / Menú de Pausa), la detección del ganador y el reinicio.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        public GameState CurrentState { get; private set; } = GameState.Setup;

        [Header("Referencias de Jugadores")]
        [SerializeField] private PlayerController player1;
        [SerializeField] private PlayerController player2;

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

            AssignFixedTeams();
        }

        private void Start()
        {
            if (CurrentState == GameState.Setup)
            {
                CurrentState = GameState.Playing;
            }
            Debug.Log($"[GameManager] Partida Iniciada (Estado: {CurrentState}).");
        }

        /// <summary>
        /// Asigna y sincroniza los bando fijos: Jugador 1 es Dark y Jugador 2 es Cute.
        /// Si no están asignados en el Inspector, los busca automáticamente por sus nombres en la escena.
        /// </summary>
        private void AssignFixedTeams()
        {
            Player1Team = PlayerTeam.Dark;
            Player2Team = PlayerTeam.Cute;

            if (player1 == null)
            {
                GameObject obj = GameObject.Find("PlayerDark");
                if (obj != null)
                {
                    player1 = obj.GetComponent<PlayerController>();
                }
            }

            if (player2 == null)
            {
                GameObject obj = GameObject.Find("PlayerCute");
                if (obj != null)
                {
                    player2 = obj.GetComponent<PlayerController>();
                }
            }

            if (player1 != null) player1.team = Player1Team;
            if (player2 != null) player2.team = Player2Team;

            Debug.Log($"[GameManager] Equipos Asignados -> P1: {Player1Team} (PlayerDark) | P2: {Player2Team} (PlayerCute)");
        }

        private void OnEnable()
        {
            GameEventsB.OnWorldFullyAssimilated += HandleWorldFullyAssimilated;
            GameEventsB.OnGamePaused += HandleGamePaused;
        }

        private void OnDisable()
        {
            GameEventsB.OnWorldFullyAssimilated -= HandleWorldFullyAssimilated;
            GameEventsB.OnGamePaused -= HandleGamePaused;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// Gestiona de forma centralizada la pausa y reanudación del juego (invocada por Tutorial o PauseMenu).
        /// </summary>
        private void HandleGamePaused(bool isPaused)
        {
            if (CurrentState == GameState.GameOver) return;

            CurrentState = isPaused ? GameState.Paused : GameState.Playing;
            Time.timeScale = isPaused ? 0f : 1f;

            if (player1 != null) player1.SetInputEnabled(!isPaused);
            if (player2 != null) player2.SetInputEnabled(!isPaused);
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
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
