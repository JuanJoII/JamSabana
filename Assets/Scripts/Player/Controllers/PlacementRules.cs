// Responsibility: Single source of truth for "can something be placed here?".
// Used by BombShip and RiftPower so the rule is defined once.
//
// Setup:
//  - groundLayers : layers of the walkable floor (where placing IS allowed).
//  - blockedLayers: layer "NoPlacement". Trigger volumes around stations, spawn points
//                   and return points (where placing is NOT allowed).

using UnityEngine;

[CreateAssetMenu(menuName = "Jam/Placement Rules", fileName = "PlacementRules")]
public class PlacementRules : ScriptableObject
{
    [Header("Layers")]
    [Tooltip("Superficies donde SÍ se puede colocar algo (el suelo).")]
    public LayerMask groundLayers;

    [Tooltip("Volúmenes trigger donde NO se puede colocar nada (estaciones, spawns, puntos de retorno).")]
    public LayerMask blockedLayers;

    [Header("Config")]
    [Tooltip("Radio de comprobación alrededor del punto. Evita colocar justo en el borde de una zona bloqueada.")]
    [Min(0f)] public float clearance = 1.5f;

    /// <summary>Raycast hacia abajo contra el suelo.</summary>
    public bool TryRaycastGround(Vector3 origin, float maxDistance, out RaycastHit hit) =>
        Physics.Raycast(origin, Vector3.down, out hit, maxDistance, groundLayers, QueryTriggerInteraction.Ignore);

    /// <summary>True si el punto toca (con margen) una zona bloqueada.</summary>
    public bool IsBlocked(Vector3 point) =>
        Physics.CheckSphere(point, clearance, blockedLayers, QueryTriggerInteraction.Collide);

    /// <summary>Hay suelo debajo del origen Y ese punto no está en una zona bloqueada.</summary>
    public bool TryGetPlacement(Vector3 origin, float maxDistance, out Vector3 point)
    {
        point = default;
        if (!TryRaycastGround(origin, maxDistance, out RaycastHit hit)) return false;
        if (IsBlocked(hit.point)) return false;

        point = hit.point;
        return true;
    }
}
