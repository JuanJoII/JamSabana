using UnityEngine;
using System.Collections;
using JamSabana.Core;

namespace JamSabana.World
{
    /// <summary>
    /// Representa un elemento o sección del escenario que puede cambiar de facción visual y lógicamente.
    /// Reacciona a las explosiones de bombas para aplicar la máscara de la esfera en el material.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class WorldZone : MonoBehaviour
    {
        [Header("Configuración de Zona")]
        [SerializeField] private int zoneId;
        [SerializeField] private PlayerTeam initialTeam = PlayerTeam.Cute;
        [SerializeField] private float progressAmount = 0.1f;

        [Header("Configuración de Conversión")]
        [SerializeField] private float maxExplosionDistance = 8f;
        [SerializeField] private float transitionDuration = 1.5f;

        private PlayerTeam currentTeam;
        private Renderer targetRenderer;
        private Coroutine transitionCoroutine;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            currentTeam = initialTeam;
        }

        private void OnEnable()
        {
            BombObject.OnBombExploded += HandleBombExploded;
        }

        private void OnDisable()
        {
            BombObject.OnBombExploded -= HandleBombExploded;
        }

        /// <summary>
        /// Detecta la explosión e inicia la conversión si está dentro del radio y proviene de la facción opuesta.
        /// </summary>
        private void HandleBombExploded(PlayerTeam attackingTeam, Vector3 explosionPosition)
        {
            if (attackingTeam == currentTeam) return;

            float distance = Vector3.Distance(transform.position, explosionPosition);
            if (distance <= maxExplosionDistance)
            {
                currentTeam = attackingTeam;

                // Lanzar evento para que el AssimilationManager actualice el balance
                GameEventsB.TriggerWorldZoneConverted(attackingTeam, zoneId, progressAmount);

                // Iniciar la transición visual del shader
                if (transitionCoroutine != null)
                {
                    StopCoroutine(transitionCoroutine);
                }
                transitionCoroutine = StartCoroutine(AnimateMaterialConversion(explosionPosition));
            }
        }

        /// <summary>
        /// Controla los parámetros del material para simular la expansión de la conversión en el shader.
        /// </summary>
        private IEnumerator AnimateMaterialConversion(Vector3 explosionPosition)
        {
            Material mat = targetRenderer.material;
            
            // Configurar la posición de la bomba en el material (Vector4 para compatibilidad con shaders)
            mat.SetVector("_BombPosition", new Vector4(explosionPosition.x, explosionPosition.y, explosionPosition.z, 1.0f));

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                
                // Animar el radio de la esfera en el material
                float currentRadius = Mathf.Lerp(0f, maxExplosionDistance, t);
                mat.SetFloat("_BombRadius", currentRadius);

                yield return null;
            }

            // Asegurar que el radio quede al máximo al finalizar
            mat.SetFloat("_BombRadius", maxExplosionDistance);
        }
    }
}
