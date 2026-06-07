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
        [Tooltip("Establecido en 0f por defecto porque el balance de la bomba se procesa directamente en el AssimilationManager al explotar.")]
        [SerializeField] private float progressAmount = 0f;

        [Header("Configuración de Transición")]
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
        private void HandleBombExploded(PlayerTeam attackingTeam, Vector3 explosionPosition, float explosionRadius)
        {
            if (attackingTeam == currentTeam) return;

            float distance = float.MaxValue;
            Collider col = GetComponent<Collider>();

            if (col != null)
            {
                // Calcular la distancia desde el punto más cercano de nuestro colisionador (ideal para suelos o zonas grandes)
                Vector3 closestPoint = col.ClosestPoint(explosionPosition);
                distance = Vector3.Distance(closestPoint, explosionPosition);
            }
            else
            {
                // Fallback al centro/pivote si no hay colisionador
                distance = Vector3.Distance(transform.position, explosionPosition);
            }

            if (distance <= explosionRadius)
            {
                currentTeam = attackingTeam;

                // Lanzar evento para que el AssimilationManager actualice el balance
                GameEventsB.TriggerWorldZoneConverted(attackingTeam, zoneId, progressAmount);

                // Iniciar la transición visual del shader
                if (transitionCoroutine != null)
                {
                    StopCoroutine(transitionCoroutine);
                }
                transitionCoroutine = StartCoroutine(AnimateMaterialConversion(explosionPosition, explosionRadius));
            }
        }

        /// <summary>
        /// Controla los parámetros del material para simular la expansión de la conversión en el shader.
        /// </summary>
        private IEnumerator AnimateMaterialConversion(Vector3 explosionPosition, float explosionRadius)
        {
            Material mat = targetRenderer.material;
            
            // Configurar la posición de la bomba en el material (Vector4 para compatibilidad con shaders)
            mat.SetVector("_BombPosition", new Vector4(explosionPosition.x, explosionPosition.y, explosionPosition.z, 1.0f));

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                
                // Animar el radio de la esfera en el material usando el radio dinámico de la bomba
                float currentRadius = Mathf.Lerp(0f, explosionRadius, t);
                mat.SetFloat("_BombRadius", currentRadius);

                yield return null;
            }

            // Asegurar que el radio quede al máximo al finalizar
            mat.SetFloat("_BombRadius", explosionRadius);
        }
    }
}
