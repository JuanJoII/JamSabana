using UnityEngine;
using JamSabana.Core;

namespace JamSabana.Utilities
{
    /// <summary>
    /// Componente de depuración para simular explosiones de bombas a través del Inspector en tiempo de ejecución.
    /// </summary>
    public class BombExplosionTester : MonoBehaviour
    {
        [Header("Configuración de Simulación")]
        [SerializeField] private PlayerTeam testAttackingTeam = PlayerTeam.Dark;
        [SerializeField] private Vector3 testExplosionPosition = Vector3.zero;
        [SerializeField] private float sphereVisualRadius = 8f;

        /// <summary>
        /// Instancia un objeto temporal y acciona el evento OnBombExploded de forma natural.
        /// Adicionalmente dibuja una esfera transparente en la escena para comprobar el área.
        /// </summary>
        [ContextMenu("Depuración: Simular Explosión de Bomba")]
        public void SimulateExplosion()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[BombExplosionTester] La simulación solo funciona en modo Play.");
                return;
            }

            // 1. Crear representación visual de la explosión en la escena
            GameObject visualSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visualSphere.name = $"ExplosionGizmo_{testAttackingTeam}";
            visualSphere.transform.position = testExplosionPosition;
            visualSphere.transform.localScale = Vector3.one * (sphereVisualRadius * 2f);

            // Intentar usar un renderizado semitransparente básico
            Renderer sphereRenderer = visualSphere.GetComponent<Renderer>();
            if (sphereRenderer != null)
            {
                // Configurar color transparente
                Color col = testAttackingTeam == PlayerTeam.Cute ? new Color(1f, 0.5f, 0.5f, 0.4f) : new Color(0.2f, 0.2f, 0.2f, 0.4f);
                sphereRenderer.material.color = col;
                
                // Desactivar sombras
                sphereRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                sphereRenderer.receiveShadows = false;
                
                // Desactivar el collider para que no interfiera físicamente en la simulación
                Collider colComponent = visualSphere.GetComponent<Collider>();
                if (colComponent != null) Destroy(colComponent);
            }

            // Destruir la representación visual después de 5 segundos
            Destroy(visualSphere, 5f);

            // 2. Instanciar una bomba ficticia para disparar el evento real sin alterar el código de BombObject
            GameObject dummyBomb = new GameObject("DummyBombObject");
            BombObject bombComponent = dummyBomb.AddComponent<BombObject>();
            bombComponent.countdownSeconds = 0f;
            bombComponent.Initialize(testAttackingTeam, testExplosionPosition);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Dibuja el área de efecto y el centro de la explosión cuando el objeto está seleccionado en la escena.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Color baseColor = testAttackingTeam == PlayerTeam.Cute ? Color.red : Color.cyan;
            
            Gizmos.color = baseColor;
            Gizmos.DrawWireSphere(testExplosionPosition, sphereVisualRadius);

            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.2f);
            Gizmos.DrawSphere(testExplosionPosition, sphereVisualRadius);
        }
#endif
    }
}
