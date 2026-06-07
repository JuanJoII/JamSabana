using UnityEngine;
using TMPro;
using JamSabana.Core;

namespace JamSabana.UI
{
    /// <summary>
    /// Controlador de UI que muestra y actualiza en tiempo real la cantidad de piezas de batería
    /// y vendajes recolectados por cada jugador en su respectivo HUD.
    /// </summary>
    [DisallowMultipleComponent]
    public class InventoryUI : MonoBehaviour
    {
        [Header("Configuración del Jugador")]
        [Range(1, 2)]
        [SerializeField] private int playerId = 1;

        [Header("Componentes de UI - Piezas de Batería")]
        [SerializeField] private TextMeshProUGUI partsText;

        [Header("Componentes de UI - Vendajes")]
        [SerializeField] private TextMeshProUGUI bandageText;

        private void OnEnable()
        {
            PlayerInventory.OnBatteryPartChanged += HandleBatteryPartChanged;
            PlayerInventory.OnBandageChanged += HandleBandageChanged;
        }

        private void OnDisable()
        {
            PlayerInventory.OnBatteryPartChanged -= HandleBatteryPartChanged;
            PlayerInventory.OnBandageChanged -= HandleBandageChanged;
        }

        private void Start()
        {
            UpdatePartsText(0);
            UpdateBandageText(0);
        }

        /// <summary>
        /// Comprueba si el bando que disparó el evento corresponde al bando asignado a este jugador físico.
        /// </summary>
        private bool IsMyTeam(PlayerTeam team)
        {
            if (GameManager.Instance != null)
            {
                PlayerTeam myTeam = (playerId == 1) ? GameManager.Instance.Player1Team : GameManager.Instance.Player2Team;
                return team == myTeam;
            }
            // Fallback en caso de que no haya GameManager instanciado en escenas de prueba: Jugador 1 es Dark y Jugador 2 es Cute.
            PlayerTeam fallbackTeam = (playerId == 1) ? PlayerTeam.Dark : PlayerTeam.Cute;
            return team == fallbackTeam;
        }

        /// <summary>
        /// Filtra y actualiza el texto de partes de batería si el evento corresponde al equipo de este jugador.
        /// </summary>
        private void HandleBatteryPartChanged(PlayerTeam team, int amount)
        {
            if (IsMyTeam(team))
            {
                UpdatePartsText(amount);
            }
        }

        /// <summary>
        /// Filtra y actualiza el texto de vendas si el evento corresponde al equipo de este jugador.
        /// </summary>
        private void HandleBandageChanged(PlayerTeam team, int amount)
        {
            if (IsMyTeam(team))
            {
                UpdateBandageText(amount);
            }
        }

        private void UpdatePartsText(int amount)
        {
            if (partsText != null)
            {
                partsText.text = $"x{amount}";
                Debug.Log($"[InventoryUI] Jugador {playerId} - Cantidad de Piezas de Batería: {amount}");
            }
        }

        private void UpdateBandageText(int amount)
        {
            if (bandageText != null)
            {
                bandageText.text = $"x{amount}";
                Debug.Log($"[InventoryUI] Jugador {playerId} - Cantidad de Vendajes: {amount}");
            }
        }

        #region Métodos de Depuración (Inspector)

        [ContextMenu("Depuración: Asignar Piezas (0)")]
        private void DebugSetParts0() => HandleBatteryPartChanged((playerId == 1) ? PlayerTeam.Dark : PlayerTeam.Cute, 0);

        [ContextMenu("Depuración: Asignar Piezas (3)")]
        private void DebugSetParts3() => HandleBatteryPartChanged((playerId == 1) ? PlayerTeam.Dark : PlayerTeam.Cute, 3);

        [ContextMenu("Depuración: Asignar Vendas (0)")]
        private void DebugSetBandages0() => HandleBandageChanged((playerId == 1) ? PlayerTeam.Dark : PlayerTeam.Cute, 0);

        [ContextMenu("Depuración: Asignar Vendas (2)")]
        private void DebugSetBandages2() => HandleBandageChanged((playerId == 1) ? PlayerTeam.Dark : PlayerTeam.Cute, 2);

        #endregion
    }
}
