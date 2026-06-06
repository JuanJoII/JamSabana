using UnityEngine;
using JamSabana.Core;

namespace JamSabana.UI
{
    /// <summary>
    /// Controlador de UI que muestra qué elementos rápidos (batería, poder, venda) tiene disponibles cada jugador.
    /// Diseñado para ubicarse en los HUDs individuales de la pantalla dividida instanciando/destruyendo prefabs.
    /// </summary>
    public class PlayerIndicatorsUI : MonoBehaviour
    {
        [Header("Configuración del Jugador")]
        [Range(1, 2)]
        [SerializeField] private int playerId = 1;

        [Header("Prefabs de Indicadores")]
        [SerializeField] private GameObject batteryPrefab;
        [SerializeField] private GameObject bandagePrefab;
        [SerializeField] private GameObject bombPowerPrefab;
        [SerializeField] private GameObject riftPowerPrefab;
        [SerializeField] private GameObject conversionPowerPrefab;

        private GameObject batteryInstance;
        private GameObject bandageInstance;
        private GameObject powerInstance;

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

        /// <summary>
        /// Destruye todas las instancias de los iconos creados para limpiar el HUD.
        /// </summary>
        public void ResetIndicators()
        {
            if (batteryInstance != null) { Destroy(batteryInstance); batteryInstance = null; }
            if (bandageInstance != null) { Destroy(bandageInstance); bandageInstance = null; }
            if (powerInstance != null) { Destroy(powerInstance); powerInstance = null; }
        }

        /// <summary>
        /// Filtra y actualiza la visibilidad de la batería instanciando o destruyendo su prefab.
        /// </summary>
        private void HandleBatteryCompleted(int targetPlayerId, bool hasBattery)
        {
            if (targetPlayerId != playerId) return;

            if (hasBattery)
            {
                if (batteryInstance == null && batteryPrefab != null)
                {
                    batteryInstance = Instantiate(batteryPrefab, transform);
                    Debug.Log($"[PlayerIndicatorsUI] Jugador {playerId} - Batería Instanciada.");
                }
            }
            else
            {
                if (batteryInstance != null)
                {
                    Destroy(batteryInstance);
                    batteryInstance = null;
                    Debug.Log($"[PlayerIndicatorsUI] Jugador {playerId} - Batería Destruida.");
                }
            }
        }

        /// <summary>
        /// Filtra y actualiza la visibilidad de la venda instanciando o destruyendo su prefab.
        /// </summary>
        private void HandleBandageAdded(int targetPlayerId, bool hasBandage)
        {
            if (targetPlayerId != playerId) return;

            if (hasBandage)
            {
                if (bandageInstance == null && bandagePrefab != null)
                {
                    bandageInstance = Instantiate(bandagePrefab, transform);
                    Debug.Log($"[PlayerIndicatorsUI] Jugador {playerId} - Venda Instanciada.");
                }
            }
            else
            {
                if (bandageInstance != null)
                {
                    Destroy(bandageInstance);
                    bandageInstance = null;
                    Debug.Log($"[PlayerIndicatorsUI] Jugador {playerId} - Venda Destruida.");
                }
            }
        }

        /// <summary>
        /// Filtra y actualiza la visibilidad del poder especial según el tipo obtenido, destruyendo el anterior e instanciando el nuevo.
        /// </summary>
        private void HandlePowerObtained(int targetPlayerId, PowerType powerType)
        {
            if (targetPlayerId != playerId) return;

            // Destruir el poder anterior si existía
            if (powerInstance != null)
            {
                Destroy(powerInstance);
                powerInstance = null;
            }

            // Instanciar el nuevo según el tipo
            GameObject prefabToInstantiate = null;
            switch (powerType)
            {
                case PowerType.Bomb:
                    prefabToInstantiate = bombPowerPrefab;
                    break;
                case PowerType.Rift:
                    prefabToInstantiate = riftPowerPrefab;
                    break;
                case PowerType.Conversion:
                    prefabToInstantiate = conversionPowerPrefab;
                    break;
            }

            if (prefabToInstantiate != null)
            {
                powerInstance = Instantiate(prefabToInstantiate, transform);
                Debug.Log($"[PlayerIndicatorsUI] Jugador {playerId} - Poder {powerType} Instanciado.");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Dibuja una caja verde en la vista de escena que delimita el contenedor de la UI (HorizontalLayoutGroup).
        /// </summary>
        private void OnDrawGizmos()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[0]);

            Gizmos.color = Color.yellow;
            Vector3 center = (corners[0] + corners[2]) * 0.5f;
            Gizmos.DrawWireSphere(center, 15f);
        }
#endif

        #region Métodos de Depuración (Inspector)

        [ContextMenu("Depuración: Alternar Batería (Sí)")]
        private void DebugToggleBatteryOn() => HandleBatteryCompleted(playerId, true);

        [ContextMenu("Depuración: Alternar Batería (No)")]
        private void DebugToggleBatteryOff() => HandleBatteryCompleted(playerId, false);

        [ContextMenu("Depuración: Alternar Venda (Sí)")]
        private void DebugToggleBandageOn() => HandleBandageAdded(playerId, true);

        [ContextMenu("Depuración: Alternar Venda (No)")]
        private void DebugToggleBandageOff() => HandleBandageAdded(playerId, false);

        [ContextMenu("Depuración: Asignar Poder (Bomba)")]
        private void DebugSetPowerBomb() => HandlePowerObtained(playerId, PowerType.Bomb);

        [ContextMenu("Depuración: Asignar Poder (Grieta)")]
        private void DebugSetPowerRift() => HandlePowerObtained(playerId, PowerType.Rift);

        [ContextMenu("Depuración: Asignar Poder (Conversión)")]
        private void DebugSetPowerConversion() => HandlePowerObtained(playerId, PowerType.Conversion);

        [ContextMenu("Depuración: Limpiar Poder")]
        private void DebugClearPower() => HandlePowerObtained(playerId, PowerType.None);

        #endregion
    }
}
