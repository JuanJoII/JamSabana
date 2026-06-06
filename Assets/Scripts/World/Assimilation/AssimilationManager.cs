using UnityEngine;

namespace JamSabana.Core
{
    /// <summary>
    /// Controlador central que sirve como la única fuente de verdad sobre el progreso de asimilación.
    /// Utiliza un balance de "tira y afloja" donde 0.0f representa victoria Cute y 1.0f victoria Dark.
    /// </summary>
    [DisallowMultipleComponent]
    public class AssimilationManager : MonoBehaviour
    {
        public static AssimilationManager Instance { get; private set; }

        [Header("Configuración de Balance")]
        [Range(0f, 1f)]
        [SerializeField] private float initialBalance = 0.5f;

        public float CurrentBalance { get; private set; } = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentBalance = initialBalance;
        }

        private void OnEnable()
        {
            GameEventsB.OnWorldZoneConverted += HandleWorldZoneConverted;
        }

        private void OnDisable()
        {
            GameEventsB.OnWorldZoneConverted -= HandleWorldZoneConverted;
        }

        private void Start()
        {
            GameEventsB.TriggerAssimilationChanged(CurrentBalance);
        }

        /// <summary>
        /// Procesa la conversión de una zona y actualiza el balance de la partida.
        /// Si la zona es Cute, baja el balance hacia 0.0f. Si es Dark, lo sube hacia 1.0f.
        /// </summary>
        private void HandleWorldZoneConverted(PlayerTeam team, int zoneId, float progressAmount)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (team == PlayerTeam.Cute)
            {
                CurrentBalance = Mathf.Max(0f, CurrentBalance - progressAmount);
            }
            else if (team == PlayerTeam.Dark)
            {
                CurrentBalance = Mathf.Min(1f, CurrentBalance + progressAmount);
            }

            Debug.Log($"[AssimilationManager] Zona {zoneId} convertida a {team}. Nuevo Balance: {CurrentBalance}");

            GameEventsB.TriggerAssimilationChanged(CurrentBalance);
            CheckWinConditions();
        }

        /// <summary>
        /// Evalúa si el balance alcanzó el límite de asimilación completa de algún bando (0.0f o 1.0f).
        /// </summary>
        private void CheckWinConditions()
        {
            if (CurrentBalance <= 0f)
            {
                Debug.Log("[AssimilationManager] ¡Mundo Oscuro completamente asimilado por Cute!");
                GameEventsB.TriggerWorldFullyAssimilated(PlayerTeam.Dark);
            }
            else if (CurrentBalance >= 1f)
            {
                Debug.Log("[AssimilationManager] ¡Mundo Colorido completamente asimilado por Dark!");
                GameEventsB.TriggerWorldFullyAssimilated(PlayerTeam.Cute);
            }
        }
        
        [ContextMenu("Depuración: Resetear Balance")]
        public void ResetBalance()
        {
            CurrentBalance = initialBalance;
            GameEventsB.TriggerAssimilationChanged(CurrentBalance);
            Debug.Log($"[AssimilationManager] Balance restablecido a {CurrentBalance}");
        }

        #region Métodos de Depuración (Inspector)

        [ContextMenu("Depuración: Simular Zona Cute (10%)")]
        private void DebugSimulateCuteZone()
        {
            HandleWorldZoneConverted(PlayerTeam.Cute, 99, 0.1f);
        }

        [ContextMenu("Depuración: Simular Zona Dark (10%)")]
        private void DebugSimulateDarkZone()
        {
            HandleWorldZoneConverted(PlayerTeam.Dark, 99, 0.1f);
        }

        [ContextMenu("Depuración: Forzar Victoria Cute")]
        private void DebugForceCuteWin()
        {
            HandleWorldZoneConverted(PlayerTeam.Cute, 99, 1.0f);
        }

        [ContextMenu("Depuración: Forzar Victoria Dark")]
        private void DebugForceDarkWin()
        {
            HandleWorldZoneConverted(PlayerTeam.Dark, 99, 1.0f);
        }

        #endregion
    }
}
