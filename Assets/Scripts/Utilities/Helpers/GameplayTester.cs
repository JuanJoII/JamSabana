using UnityEngine;
using JamSabana.Core;

namespace JamSabana.Utilities
{
    /// <summary>
    /// Componente centralizado de depuración que invoca los eventos desacoplados del juego
    /// para simular y probar las explosiones de bomba y la asignación de poderes.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameplayTester : MonoBehaviour
    {
        [Header("Configuración de Bomba")]
        [SerializeField] private PlayerTeam bombTeam = PlayerTeam.Dark;
        [SerializeField] private Vector3 bombPosition = Vector3.zero;
        [SerializeField] private float bombRadius = 4f;

        #region Prueba de Bomba

        /// <summary>
        /// Simula la explosión de una bomba disparando su evento global de forma desacoplada.
        /// </summary>
        [ContextMenu("Prueba Bomba: Simular Explosión")]
        public void SimulateBombExplosion()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[GameplayTester] La simulación de bomba solo funciona en modo Play.");
                return;
            }

            // Invocar el trigger estático de la bomba para que WorldZone y AssimilationManager reaccionen
            BombObject.TriggerBombExploded(bombTeam, bombPosition, bombRadius);
            Debug.Log($"[GameplayTester] Evento OnBombExploded simulado para {bombTeam} en {bombPosition} con radio {bombRadius}.");
        }

        #endregion

        #region Pruebas de Poderes (Disparo y Grieta)

        [ContextMenu("Prueba Poderes: Otorgar Conversión a PlayerDark")]
        public void GrantConversionDark() => GrantPowerToTeam(PlayerTeam.Dark, PowerType.Conversion);

        [ContextMenu("Prueba Poderes: Otorgar Conversión a PlayerCute")]
        public void GrantConversionCute() => GrantPowerToTeam(PlayerTeam.Cute, PowerType.Conversion);

        [ContextMenu("Prueba Poderes: Otorgar Rift a PlayerDark")]
        public void GrantRiftDark() => GrantPowerToTeam(PlayerTeam.Dark, PowerType.Rift);

        [ContextMenu("Prueba Poderes: Otorgar Rift a PlayerCute")]
        public void GrantRiftCute() => GrantPowerToTeam(PlayerTeam.Cute, PowerType.Rift);

        [ContextMenu("Prueba Poderes: Otorgar Bomba a PlayerDark")]
        public void GrantBombDark() => GrantPowerToTeam(PlayerTeam.Dark, PowerType.Bomb);

        [ContextMenu("Prueba Poderes: Otorgar Bomba a PlayerCute")]
        public void GrantBombCute() => GrantPowerToTeam(PlayerTeam.Cute, PowerType.Bomb);

        #endregion

        /// <summary>
        /// Invoca de forma desacoplada el otorgamiento de un poder a través del sistema de recompensas.
        /// </summary>
        private void GrantPowerToTeam(PlayerTeam team, PowerType power)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[GameplayTester] El otorgamiento de poderes solo funciona en modo Play.");
                return;
            }

            PowerRewardSystem rewardSystem = Object.FindAnyObjectByType<PowerRewardSystem>();
            if (rewardSystem != null)
            {
                rewardSystem.GrantPower(team, power);
                Debug.Log($"[GameplayTester] Solicitada concesión de {power} para {team} al PowerRewardSystem.");
            }
            else
            {
                Debug.LogError("[GameplayTester] No se encontró la instancia de PowerRewardSystem en la escena para invocar el evento.");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Dibuja el radio de la explosión en la escena para referencia del diseñador.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Color baseColor = bombTeam == PlayerTeam.Cute ? Color.red : Color.cyan;
            Gizmos.color = baseColor;
            Gizmos.DrawWireSphere(bombPosition, bombRadius);

            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.15f);
            Gizmos.DrawSphere(bombPosition, bombRadius);
        }
#endif
    }
}
