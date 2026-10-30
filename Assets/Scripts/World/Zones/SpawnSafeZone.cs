using System.Collections.Generic;
using UnityEngine;

namespace JamSabana.World
{
    /// <summary>
    /// Delimita una zona segura alrededor de los puntos de spawn.
    /// Impide que se coloquen grietas (RiftPower) dentro de su perímetro para evitar
    /// situaciones de bloqueo o spawn-trapping.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SpawnSafeZone : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Radio de seguridad por defecto si se requiere comprobación por distancia.")]
        public float safeRadius = 4f;

        [Header("Gizmos de Depuración")]
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 0.4f, 0.25f);
        [SerializeField] private Color gizmoWireColor = new Color(0.2f, 0.9f, 0.4f, 0.9f);

        private static readonly List<SpawnSafeZone> activeSafeZones = new List<SpawnSafeZone>();
        private static readonly HashSet<PlayerController> playersInSafeZones = new HashSet<PlayerController>();

        private Collider zoneCollider;

        private void Awake()
        {
            zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null)
            {
                zoneCollider.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            if (!activeSafeZones.Contains(this))
            {
                activeSafeZones.Add(this);
            }
        }

        private void OnDisable()
        {
            activeSafeZones.Remove(this);
            playersInSafeZones.RemoveWhere(player => player == null);
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                playersInSafeZones.Add(player);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                playersInSafeZones.Remove(player);
            }
        }

        /// <summary>
        /// Comprueba si el jugador dado se encuentra físicamente dentro de un trigger de zona segura.
        /// </summary>
        public static bool IsPlayerInSafeZone(PlayerController player)
        {
            if (player == null) return false;
            return playersInSafeZones.Contains(player);
        }

        /// <summary>
        /// Comprueba si una coordenada Vector3 cae dentro del volumen o radio de alguna zona segura activa.
        /// </summary>
        public static bool IsPositionInAnySafeZone(Vector3 position, float extraPadding = 0f)
        {
            for (int i = 0; i < activeSafeZones.Count; i++)
            {
                SpawnSafeZone zone = activeSafeZones[i];
                if (zone == null) continue;

                if (zone.ContainsPosition(position, extraPadding))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Evalúa si la posición está dentro del collider o dentro del safeRadius de esta zona.
        /// </summary>
        public bool ContainsPosition(Vector3 position, float extraPadding = 0f)
        {
            if (zoneCollider == null)
            {
                zoneCollider = GetComponent<Collider>();
            }

            if (zoneCollider is SphereCollider sphere)
            {
                Vector3 center = transform.TransformPoint(sphere.center);
                float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                float radius = (sphere.radius * maxScale) + extraPadding;
                return Vector3.Distance(position, center) <= radius;
            }

            if (zoneCollider != null)
            {
                Vector3 closest = zoneCollider.ClosestPoint(position);
                return Vector3.Distance(closest, position) <= (0.05f + extraPadding);
            }

            // Fallback si no hay colisionador inicializado
            return Vector3.Distance(position, transform.position) <= (safeRadius + extraPadding);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawZoneGizmo(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawZoneGizmo(true);
        }

        private void DrawZoneGizmo(bool isSelected)
        {
            Collider col = zoneCollider != null ? zoneCollider : GetComponent<Collider>();
            Color baseColor = isSelected ? Color.cyan : gizmoColor;
            Color wireColor = isSelected ? Color.white : gizmoWireColor;

            if (col is SphereCollider sphere)
            {
                Vector3 center = transform.TransformPoint(sphere.center);
                float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                float radius = sphere.radius * maxScale;

                Gizmos.color = baseColor;
                Gizmos.DrawSphere(center, radius);

                Gizmos.color = wireColor;
                Gizmos.DrawWireSphere(center, radius);
            }
            else if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = baseColor;
                Gizmos.DrawCube(box.center, box.size);

                Gizmos.color = wireColor;
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else
            {
                Gizmos.color = baseColor;
                Gizmos.DrawSphere(transform.position, safeRadius);

                Gizmos.color = wireColor;
                Gizmos.DrawWireSphere(transform.position, safeRadius);
            }
        }
#endif
    }
}
