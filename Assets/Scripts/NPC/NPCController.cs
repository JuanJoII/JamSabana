// Responsibility: To represent an NPC in the scene.
// Stores its team, handles linear patrol movement with obstacle/ground detection, and exposes the conversion method.

using UnityEngine;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    public PlayerTeam team;

    [Header("Prefabs de conversión")]
    public GameObject cutePrefab;
    public GameObject darkPrefab;

    [Header("Movimiento Lineal (Patrulla)")]
    [Tooltip("Activa o desactiva el movimiento lineal del NPC.")]
    public bool enableMovement = true;

    [Tooltip("Velocidad de desplazamiento del NPC.")]
    public float moveSpeed = 1.65f;

    [Tooltip("Distancia máxima desde su punto inicial antes de darse la vuelta.")]
    public float patrolDistance = 3.5f;

    [Tooltip("Velocidad de giro al cambiar de dirección.")]
    public float rotationSpeed = 5f;

    [Tooltip("Tiempo de pausa breve antes de caminar en sentido opuesto.")]
    public float waitTimeAtTurns = 2.0f;

    [Tooltip("Si está activo, elige una dirección inicial despejada de forma aleatoria al iniciar.")]
    public bool randomizeInitialDirection = true;

    [Header("Detección de Obstáculos y Suelo")]
    [Tooltip("Distancia hacia adelante para detectar paredes u obstáculos.")]
    public float obstacleCheckDistance = 0.8f;

    [Tooltip("Altura desde la base del NPC desde donde se lanza el rayo frontal.")]
    public float raycastHeightOffset = 0.5f;

    [Tooltip("Capas consideradas como obstáculos (ej. Default para edificios, árboles y bordes).")]
    public LayerMask obstacleLayer = 1 << 0; // Layer 0: Default

    [Tooltip("Capas de suelo válido por donde el NPC puede caminar (ej. ValidBomb).")]
    public LayerMask validGroundLayer = 1 << 6; // Layer 6: ValidBomb

    private static readonly List<NPCController> allNPCs = new List<NPCController>();
    public static IReadOnlyList<NPCController> AllNPCs => allNPCs;

    public static event System.Action<PlayerTeam> OnNPCConverted;

    private Vector3 startPosition;
    private Vector3 moveDirection;
    private float waitTimer;
    private bool isMovementInitialized;

    private void OnEnable() => allNPCs.Add(this);
    private void OnDisable() => allNPCs.Remove(this);

    private void Start()
    {
        if (!isMovementInitialized)
        {
            InitializeMovement();
        }
    }

    private void Update()
    {
        if (!enableMovement) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            RotateTowardsDirection();
            return;
        }

        if (ShouldTurnAround())
        {
            ReverseDirection();
            return;
        }

        transform.position += moveDirection * (moveSpeed * Time.deltaTime);
        RotateTowardsDirection();
    }

    private void InitializeMovement()
    {
        startPosition = transform.position;
        isMovementInitialized = true;

        if (randomizeInitialDirection)
        {
            moveDirection = FindClearDirection();
        }
        else
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            moveDirection = forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        }
    }

    private Vector3 FindClearDirection()
    {
        float baseAngle = Random.Range(0f, 360f);
        const int directionsToTest = 8;
        float angleStep = 360f / directionsToTest;

        for (int i = 0; i < directionsToTest; i++)
        {
            float angle = baseAngle + i * angleStep;
            Vector3 candidate = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            if (!IsPathBlocked(candidate))
            {
                return candidate.normalized;
            }
        }

        return Quaternion.Euler(0f, baseAngle, 0f) * Vector3.forward;
    }

    private bool ShouldTurnAround()
    {
        Vector3 offset = transform.position - startPosition;
        offset.y = 0f;

        if (Vector3.Dot(offset, moveDirection) >= patrolDistance)
        {
            return true;
        }

        return IsPathBlocked(moveDirection);
    }

    private bool IsPathBlocked(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return true;

        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;

        // 1. Detección frontal de obstáculos (edificios, árboles, muros)
        if (obstacleLayer.value != 0 &&
            Physics.Raycast(rayOrigin, direction, obstacleCheckDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        // 2. Detección de suelo válido más adelante para evitar salir del mapa o pisar zonas bloqueadas
        if (validGroundLayer.value != 0)
        {
            Vector3 aheadProbe = transform.position + direction * obstacleCheckDistance + Vector3.up * 2f;
            int combinedMask = validGroundLayer.value | obstacleLayer.value;

            if (Physics.Raycast(aheadProbe, Vector3.down, out RaycastHit hit, 5f, combinedMask, QueryTriggerInteraction.Ignore))
            {
                int hitBit = 1 << hit.collider.gameObject.layer;
                bool isValidFloor = (validGroundLayer.value & hitBit) != 0;
                if (!isValidFloor)
                {
                    return true;
                }
            }
            else
            {
                // No hay suelo delante (borde del mapa)
                return true;
            }
        }

        return false;
    }

    private void ReverseDirection()
    {
        Vector3 opposite = -moveDirection;

        // Si la dirección opuesta también está bloqueada, buscar una vía despejada y actualizar el centro de patrulla
        if (IsPathBlocked(opposite))
        {
            moveDirection = FindClearDirection();
            startPosition = transform.position;
        }
        else
        {
            moveDirection = opposite;
        }

        waitTimer = waitTimeAtTurns;
    }

    private void RotateTowardsDirection()
    {
        if (moveDirection.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void CopyMovementStateFrom(NPCController source)
    {
        enableMovement = source.enableMovement;
        moveSpeed = source.moveSpeed;
        patrolDistance = source.patrolDistance;
        rotationSpeed = source.rotationSpeed;
        waitTimeAtTurns = source.waitTimeAtTurns;
        randomizeInitialDirection = source.randomizeInitialDirection;
        obstacleCheckDistance = source.obstacleCheckDistance;
        raycastHeightOffset = source.raycastHeightOffset;
        obstacleLayer = source.obstacleLayer;
        validGroundLayer = source.validGroundLayer;

        startPosition = source.startPosition;
        moveDirection = source.moveDirection;
        waitTimer = source.waitTimer;
        isMovementInitialized = true;
    }

    public void Convert(PlayerTeam converterTeam)
    {
        if (team == converterTeam) return;

        team = converterTeam;
        OnNPCConverted?.Invoke(converterTeam);

        GameObject newPrefab = converterTeam == PlayerTeam.Cute ? cutePrefab : darkPrefab;
        if (newPrefab != null)
        {
            GameObject newNpcObj = Instantiate(newPrefab, transform.position, transform.rotation);
            if (newNpcObj.TryGetComponent<NPCController>(out NPCController newNpc))
            {
                newNpc.CopyMovementStateFrom(this);
            }
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Vector3 dir = Application.isPlaying && moveDirection.sqrMagnitude > 0.001f ? moveDirection : transform.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
        dir.Normalize();

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center - dir * patrolDistance, center + dir * patrolDistance);
        Gizmos.DrawWireSphere(center - dir * patrolDistance, 0.2f);
        Gizmos.DrawWireSphere(center + dir * patrolDistance, 0.2f);

        Gizmos.color = Color.yellow;
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + dir * obstacleCheckDistance);
    }
}
