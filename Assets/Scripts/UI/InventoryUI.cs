using UnityEngine;
using TMPro;
using JamSabana.Core;

namespace JamSabana.UI
{
    /// <summary>
    /// Controlador de UI que muestra la cantidad de piezas de batería recolectadas por cada jugador.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Configuración del Jugador")]
        [Range(1, 2)]
        [SerializeField] private int playerId = 1;

        [Header("Componentes de UI")]
        [SerializeField] private TextMeshProUGUI partsText;

        private void OnEnable()
        {
            GameEventsB.OnBatteryPartChanged += HandleBatteryPartChanged;
        }

        private void OnDisable()
        {
            GameEventsB.OnBatteryPartChanged -= HandleBatteryPartChanged;
        }

        private void Start()
        {
            UpdatePartsText(0);
        }

        /// <summary>
        /// Filtra y actualiza el texto de partes recolectadas si el evento corresponde a este jugador.
        /// </summary>
        private void HandleBatteryPartChanged(int targetPlayerId, int amount)
        {
            if (targetPlayerId == playerId)
            {
                UpdatePartsText(amount);
            }
        }

        private void UpdatePartsText(int amount)
        {
            if (partsText != null)
            {
                partsText.text = $"x{amount}";
                Debug.Log($"[InventoryUI] Jugador {playerId} - Cantidad de Piezas: {amount}");
            }
        }

        #region Métodos de Depuración (Inspector)

        [ContextMenu("Depuración: Asignar Piezas (0)")]
        private void DebugSetParts0() => HandleBatteryPartChanged(playerId, 0);

        [ContextMenu("Depuración: Asignar Piezas (1)")]
        private void DebugSetParts1() => HandleBatteryPartChanged(playerId, 1);
        [ContextMenu("Depuración: Asignar Piezas (2)")]
        private void DebugSetParts2() => HandleBatteryPartChanged(playerId, 2);

        [ContextMenu("Depuración: Asignar Piezas (3)")]
        private void DebugSetParts3() => HandleBatteryPartChanged(playerId, 3);

        #endregion
    }
}
