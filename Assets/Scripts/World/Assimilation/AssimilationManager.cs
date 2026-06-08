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
            BombObject.OnBombExploded += HandleBombExploded;
            NPCController.OnNPCConverted += HandleNPCConverted;
        }

        private void OnDisable()
        {
            GameEventsB.OnWorldZoneConverted -= HandleWorldZoneConverted;
            BombObject.OnBombExploded -= HandleBombExploded;
            NPCController.OnNPCConverted -= HandleNPCConverted;
        }

        /// <summary>
        /// Aumenta o disminuye la asimilación un 3% (0.03f) a favor del equipo del jugador que convirtió el NPC.
        /// </summary>
        private void HandleNPCConverted(PlayerTeam converterTeam)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            float npcProgressAmount = 0.1f;
            ApplyAssimilationChange(converterTeam, npcProgressAmount);
            Debug.Log($"[AssimilationManager] NPC convertido por {converterTeam}. Balance modificado en 3%. Nuevo Balance: {CurrentBalance}");
        }

        private void Start()
        {
            GameEventsB.TriggerAssimilationChanged(CurrentBalance);
        }

        /// <summary>
        /// Procesa la conversión visual de una zona.
        /// Nota: El balance de asimilación lógica ya no se modifica aquí para evitar doble conteo
        /// con la explosión de la bomba, que ya añade el 10% lógicamente.
        /// </summary>
        private void HandleWorldZoneConverted(PlayerTeam team, int zoneId, float progressAmount)
        {
            Debug.Log($"[AssimilationManager] Zona {zoneId} convertida visualmente a {team}.");
        }

        /// <summary>
        /// Aumenta o disminuye la asimilación un 10% (0.1f) a favor de la facción que detonó la bomba.
        /// </summary>
        private void HandleBombExploded(PlayerTeam attackingTeam, Vector3 position, float radius)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            float bombProgressAmount = 0.3f;
            ApplyAssimilationChange(attackingTeam, bombProgressAmount);
            Debug.Log($"[AssimilationManager] Bomba detonada por {attackingTeam}. Balance modificado en 10%. Nuevo Balance: {CurrentBalance}");
        }

        /// <summary>
        /// Aplica la lógica matemática para actualizar el balance de asimilación e informar del cambio.
        /// </summary>
        private void ApplyAssimilationChange(PlayerTeam team, float progressAmount)
        {
            if (team == PlayerTeam.Cute)
            {
                CurrentBalance = Mathf.Max(0f, CurrentBalance - progressAmount);
            }
            else if (team == PlayerTeam.Dark)
            {
                CurrentBalance = Mathf.Min(1f, CurrentBalance + progressAmount);
            }

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
            ApplyAssimilationChange(PlayerTeam.Cute, 0.1f);
            Debug.Log($"[AssimilationManager] Depuración: Simulado cambio de balance Cute (10%). Nuevo Balance: {CurrentBalance}");
        }

        [ContextMenu("Depuración: Simular Zona Dark (10%)")]
        private void DebugSimulateDarkZone()
        {
            ApplyAssimilationChange(PlayerTeam.Dark, 0.1f);
            Debug.Log($"[AssimilationManager] Depuración: Simulado cambio de balance Dark (10%). Nuevo Balance: {CurrentBalance}");
        }

        [ContextMenu("Depuración: Forzar Victoria Cute")]
        private void DebugForceCuteWin()
        {
            ApplyAssimilationChange(PlayerTeam.Cute, 1.0f);
            Debug.Log($"[AssimilationManager] Depuración: Forzada victoria Cute. Nuevo Balance: {CurrentBalance}");
        }

        [ContextMenu("Depuración: Forzar Victoria Dark")]
        private void DebugForceDarkWin()
        {
            ApplyAssimilationChange(PlayerTeam.Dark, 1.0f);
            Debug.Log($"[AssimilationManager] Depuración: Forzada victoria Dark. Nuevo Balance: {CurrentBalance}");
        }

        [ContextMenu("Depuración: Simular Conversión NPC Cute (3%)")]
        private void DebugNPCConvertedCute()
        {
            HandleNPCConverted(PlayerTeam.Cute);
        }

        [ContextMenu("Depuración: Simular Conversión NPC Dark (3%)")]
        private void DebugNPCConvertedDark()
        {
            HandleNPCConverted(PlayerTeam.Dark);
        }

        #endregion
    }
}
