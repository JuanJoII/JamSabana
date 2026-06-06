using UnityEngine;
using JamSabana.Core;

namespace JamSabana.UI
{
    /// <summary>
    /// Controlador de UI que muestra qué elementos rápidos (batería, poder, venda) tiene disponibles cada jugador.
    /// Diseñado para ubicarse en los HUDs individuales de la pantalla dividida.
    /// </summary>
    public class PlayerIndicatorsUI : MonoBehaviour
    {
        [Header("Configuración del Jugador")]
        [Range(1, 2)]
        [SerializeField] private int playerId = 1;

        [Header("Iconos Indicadores (GameObjects)")]
        [SerializeField] private GameObject batteryIcon;
        [SerializeField] private GameObject powerIcon;
        [SerializeField] private GameObject bandageIcon;

        private void OnEnable()
        {
            GameEventsB.OnBatteryCompleted += HandleBatteryCompleted;
            GameEventsB.OnPowerObtained += HandlePowerObtained;
            GameEventsB.OnBandageAdded += HandleBandageAdded;
        }

        private void OnDisable()
        {
            GameEventsB.OnBatteryCompleted -= HandleBatteryCompleted;
            GameEventsB.OnPowerObtained -= HandlePowerObtained;
            GameEventsB.OnBandageAdded -= HandleBandageAdded;
        }

        private void Start()
        {
            ResetIndicators();
        }

        public void ResetIndicators()
        {
            if (batteryIcon != null) batteryIcon.SetActive(false);
            if (powerIcon != null) powerIcon.SetActive(false);
            if (bandageIcon != null) bandageIcon.SetActive(false);
        }

        /// <summary>
        /// Filtra y actualiza la visibilidad del icono de batería según el ID del jugador de este HUD.
        /// </summary>
        private void HandleBatteryCompleted(int targetPlayerId, bool hasBattery)
        {
            if (targetPlayerId == playerId && batteryIcon != null)
            {
                batteryIcon.SetActive(hasBattery);
                Debug.Log($"[PlayerIndicatorsUI] Jugador {playerId} - Batería: {hasBattery}");
            }
        }

        /// <summary>
        /// Filtra y actualiza la visibilidad del icono de poder especial según el ID del jugador de este HUD.
        /// </summary>
        private void HandlePowerObtained(int targetPlayerId, bool hasPower)
        {
            if (targetPlayerId == playerId && powerIcon != null)
            {
                powerIcon.SetActive(hasPower);
                Debug.Log($"[PlayerIndicatorsUI] Jugador {playerId} - Poder de Ataque: {hasPower}");
            }
        }

        /// <summary>
        /// Filtra y actualiza la visibilidad del icono de venda de reparación según el ID del jugador de este HUD.
        /// </summary>
        private void HandleBandageAdded(int targetPlayerId, bool hasBandage)
        {
            if (targetPlayerId == playerId && bandageIcon != null)
            {
                bandageIcon.SetActive(hasBandage);
                Debug.Log($"[PlayerIndicatorsUI] Jugador {playerId} - Venda de Reparación: {hasBandage}");
            }
        }

        #region Métodos de Depuración (Inspector)

        [ContextMenu("Depuración: Alternar Batería (Sí)")]
        private void DebugToggleBatteryOn() => HandleBatteryCompleted(playerId, true);

        [ContextMenu("Depuración: Alternar Batería (No)")]
        private void DebugToggleBatteryOff() => HandleBatteryCompleted(playerId, false);

        [ContextMenu("Depuración: Alternar Poder (Sí)")]
        private void DebugTogglePowerOn() => HandlePowerObtained(playerId, true);

        [ContextMenu("Depuración: Alternar Poder (No)")]
        private void DebugTogglePowerOff() => HandlePowerObtained(playerId, false);

        [ContextMenu("Depuración: Alternar Venda (Sí)")]
        private void DebugToggleBandageOn() => HandleBandageAdded(playerId, true);

        [ContextMenu("Depuración: Alternar Venda (No)")]
        private void DebugToggleBandageOff() => HandleBandageAdded(playerId, false);

        #endregion
    }
}
