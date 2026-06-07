using UnityEngine;
using System.Reflection;

/// <summary>
/// Componente de depuración para probar el poder de conversión y el VFX de disparo en tiempo de ejecución.
/// </summary>
public class ConversionPowerTester : MonoBehaviour
{
    [Header("Configuración de Prueba")]
    [Tooltip("Duración temporal extendida para pruebas (en segundos) para que no expire rápido.")]
    [SerializeField] private float testDuration = 9999f;

    /// <summary>
    /// Otorga el poder de conversión al Jugador Dark (Jugador 1) de forma normal (tiempo original).
    /// </summary>
    [ContextMenu("Depuración: Otorgar Conversión normal (Dark)")]
    public void GrantConversionDark()
    {
        GrantPowerToTeam(PlayerTeam.Dark, false);
    }

    /// <summary>
    /// Otorga el poder de conversión al Jugador Cute (Jugador 2) de forma normal (tiempo original).
    /// </summary>
    [ContextMenu("Depuración: Otorgar Conversión normal (Cute)")]
    public void GrantConversionCute()
    {
        GrantPowerToTeam(PlayerTeam.Cute, false);
    }

    /// <summary>
    /// Otorga el poder de conversión al Jugador Dark con tiempo indefinido para probar el disparo y su VFX.
    /// </summary>
    [ContextMenu("Depuración: Otorgar Conversión INDEFINIDA (Dark)")]
    public void GrantConversionIndefiniteDark()
    {
        GrantPowerToTeam(PlayerTeam.Dark, true);
    }

    /// <summary>
    /// Otorga el poder de conversión al Jugador Cute con tiempo indefinido para probar el disparo y su VFX.
    /// </summary>
    [ContextMenu("Depuración: Otorgar Conversión INDEFINIDA (Cute)")]
    public void GrantConversionIndefiniteCute()
    {
        GrantPowerToTeam(PlayerTeam.Cute, true);
    }

    private void GrantPowerToTeam(PlayerTeam team, bool indefinite)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ConversionPowerTester] Las pruebas de depuración solo funcionan en modo Play.");
            return;
        }

        PlayerController targetPlayer = null;
        foreach (PlayerController p in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (p.team == team)
            {
                targetPlayer = p;
                break;
            }
        }

        if (targetPlayer == null)
        {
            Debug.LogError($"[ConversionPowerTester] No se encontró ningún PlayerController para el equipo {team}.");
            return;
        }

        // Si es indefinido, modificamos la duración del componente usando Reflexión
        if (indefinite)
        {
            ConversionPower powerComponent = targetPlayer.GetComponent<ConversionPower>();
            if (powerComponent != null)
            {
                FieldInfo durationField = typeof(ConversionPower).GetField("duration", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (durationField != null)
                {
                    durationField.SetValue(powerComponent, testDuration);
                    Debug.Log($"[ConversionPowerTester] Duración de {team} cambiada temporalmente a {testDuration}s para pruebas.");
                }
            }
        }

        // Simular que el inventario del jugador recibe el poder
        PlayerInventory inventory = targetPlayer.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            FieldInfo currentPowerField = typeof(PlayerInventory).GetField("currentPower", BindingFlags.NonPublic | BindingFlags.Instance);
            if (currentPowerField != null)
            {
                currentPowerField.SetValue(inventory, PowerType.Conversion);
            }
            
            // Invocar el método privado ActivatePower para realizar la animación/activación del poder
            MethodInfo activatePowerMethod = typeof(PlayerInventory).GetMethod("ActivatePower", BindingFlags.NonPublic | BindingFlags.Instance);
            if (activatePowerMethod != null)
            {
                activatePowerMethod.Invoke(inventory, null);
                Debug.Log($"[ConversionPowerTester] Poder de conversión activado en {team} {(indefinite ? "indefinidamente" : "normalmente")}.");
            }
            else
            {
                targetPlayer.ActivateConversion();
                Debug.Log($"[ConversionPowerTester] Activación alternativa directa invocada en {team}.");
            }
        }
        else
        {
            targetPlayer.ActivateConversion();
            Debug.Log($"[ConversionPowerTester] Activación alternativa directa invocada en {team}.");
        }
    }
}
