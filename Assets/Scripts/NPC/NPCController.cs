// Responsibility: To represent an NPC in the scene.
// Stores its team, handles dynamic linear patrol movement with obstacle/ground/crowd avoidance, and exposes the conversion method.

using UnityEngine;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    public PlayerTeam team;

    [Header("Prefabs de conversión")]
    public GameObject cutePrefab;
    public GameObject darkPrefab;

    [Header("Movimiento Lineal (Patrulla Dinámica)")]
    [Tooltip("Activa o desactiva el movimiento lineal del NPC.")]
    public bool enableMovement = true;

    [Tooltip("Velocidad de desplazamiento del NPC.")]
    public float moveSpeed = 2.2f;

    [Tooltip("Distancia base de cada tramo lineal antes de cambiar de dirección.")]
    public float patrolDistance = 3.0f;

    [Tooltip("Variación aleatoria (+/-) en la distancia de cada tramo para desincronizar a los NPCs.")]
    public float patrolDistanceVariance = 1.0f;

    [Tooltip("Radio máximo permitido desde su posición original de aparición.")]
    public float maxWanderRadius = 4f;

    [Tooltip("Velocidad de giro al cambiar de dirección.")]
    public float rotationSpeed = 10f;

    [Tooltip("Tiempo de pausa breve antes de caminar en la nueva dirección.")]
    public float waitTimeAtTurns = 0.25f;

    [Tooltip("Si está activo, elige una dirección inicial despejada de forma aleatoria al iniciar.")]
    public bool randomizeInitialDirection = true;

    [Header("Detección de Obstáculos, Suelo y Agrupamiento")]
    [Tooltip("Distancia hacia adelante para detectar paredes u obstáculos.")]
    public float obstacleCheckDistance = 0.8f;

    [Tooltip("Altura desde la base del NPC desde donde se lanza el rayo frontal.")]
    public float raycastHeightOffset = 0.5f;

    [Tooltip("Distancia a la que evita caminar hacia otros NPCs para no agruparse.")]
    public float npcSeparationRadius = 2.5f;

    [Tooltip("Capas consideradas como obstáculos (ej. Default para edificios, árboles y bordes).")]
    public LayerMask obstacleLayer = 1 << 0; // Layer 0: Default

    [Tooltip("Capas de suelo válido por donde el NPC puede caminar (ej. ValidBomb).")]
    public LayerMask validGroundLayer = 1 << 6; // Layer 6: ValidBomb

    private static readonly List<NPCController> allNPCs = new List<NPCController>();
    public static IReadOnlyList<NPCController> AllNPCs => allNPCs;

    public static event System.Action<PlayerTeam> OnNPCConverted;

    private Vector3 homePosition;
    private Vector3 segmentStartPosition;
    private Vector3 moveDirection;
    private float currentLegDistance;
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

        if (ShouldChangeDirection())
        {
            PickNewLinearLeg();
            return;
        }

        transform.position += moveDirection * (moveSpeed * Time.deltaTime);
        RotateTowardsDirection();
    }

    private void InitializeMovement()
    {
        homePosition = transform.position;
        segmentStartPosition = transform.position;
        currentLegDistance = CalculateRandomLegDistance();
        isMovementInitialized = true;

        if (randomizeInitialDirection)
        {
            moveDirection = FindBestDirection();
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

    private float CalculateRandomLegDistance()
    {
        float minDistance = Mathf.Max(1.0f, patrolDistance - patrolDistanceVariance);
        float maxDistance = Mathf.Max(minDistance, patrolDistance + patrolDistanceVariance);
        return Random.Range(minDistance, maxDistance);
    }

    private bool ShouldChangeDirection()
    {
        // 1. ¿Completó la distancia del tramo lineal actual?
        Vector3 segmentOffset = transform.position - segmentStartPosition;
        segmentOffset.y = 0f;
        if (Vector3.Dot(segmentOffset, moveDirection) >= currentLegDistance)
        {
            return true;
        }

        // 2. ¿Se salió del radio máximo permitido (maxWanderRadius) y sigue alejándose de su origen?
        if (maxWanderRadius > 0f)
        {
            Vector3 fromHome = transform.position - homePosition;
            fromHome.y = 0f;
            if (fromHome.sqrMagnitude >= maxWanderRadius * maxWanderRadius &&
                Vector3.Dot(fromHome, moveDirection) > 0f)
            {
                return true;
            }
        }

        // 3. ¿Hay un obstáculo, borde de mapa o acumulación de NPCs justo enfrente?
        return IsPathBlocked(moveDirection) || IsNpcDirectlyAhead(moveDirection);
    }

    private void PickNewLinearLeg()
    {
        segmentStartPosition = transform.position;
        currentLegDistance = CalculateRandomLegDistance();
        moveDirection = FindBestDirection();
        waitTimer = waitTimeAtTurns;
    }

    private Vector3 FindBestDirection()
    {
        float baseAngle = Random.Range(0f, 360f);
        const int directionsToTest = 12;
        float angleStep = 360f / directionsToTest;

        Vector3 toHome = homePosition - transform.position;
        toHome.y = 0f;
        float distFromHome = toHome.magnitude;
        Vector3 toHomeDir = distFromHome > 0.001f ? toHome / distFromHome : Vector3.zero;

        Vector3 bestDirection = -moveDirection;
        float bestScore = float.NegativeInfinity;
        bool foundUnblocked = false;

        for (int i = 0; i < directionsToTest; i++)
        {
            float angle = baseAngle + i * angleStep;
            Vector3 candidate = (Quaternion.Euler(0f, angle, 0f) * Vector3.forward).normalized;

            if (IsPathBlocked(candidate))
            {
                continue;
            }

            foundUnblocked = true;
            float score = Random.Range(0f, 0.35f); // Ruido aleatorio para romper simetrías

            // 1. Respetar maxWanderRadius (preferir direcciones que mantengan o devuelvan al NPC dentro de su radio)
            if (maxWanderRadius > 0f)
            {
                Vector3 projectedPos = transform.position + candidate * Mathf.Min(currentLegDistance, maxWanderRadius);
                Vector3 projectedFromHome = projectedPos - homePosition;
                projectedFromHome.y = 0f;

                if (distFromHome >= maxWanderRadius * 0.85f)
                {
                    // Si está cerca del borde o fuera de su radio, premiar fuertemente volver hacia homePosition
                    score += Vector3.Dot(candidate, toHomeDir) * 3.0f;
                }
                else if (projectedFromHome.magnitude > maxWanderRadius)
                {
                    // Penalizar si el tramo sacaría al NPC muy fuera de su maxWanderRadius
                    score -= 1.5f;
                }
            }

            // 2. Evitar agruparse con otros NPCs cercanos (Propuesta 3)
            score += EvaluateCrowdSeparationScore(candidate);

            // 3. Evitar repetir exactamente el mismo eje anterior para dinamizar la patrulla
            if (moveDirection.sqrMagnitude > 0.001f)
            {
                float alignmentWithPreviousAxis = Mathf.Abs(Vector3.Dot(candidate, moveDirection));
                if (alignmentWithPreviousAxis > 0.9f)
                {
                    score -= 0.75f;
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = candidate;
            }
        }

        if (!foundUnblocked)
        {
            return moveDirection.sqrMagnitude > 0.001f
                ? -moveDirection
                : Quaternion.Euler(0f, baseAngle, 0f) * Vector3.forward;
        }

        return bestDirection;
    }

    private float EvaluateCrowdSeparationScore(Vector3 candidateDirection)
    {
        if (npcSeparationRadius <= 0f) return 0f;

        float crowdScore = 0f;
        float radiusSqr = npcSeparationRadius * npcSeparationRadius;

        for (int i = 0; i < allNPCs.Count; i++)
        {
            NPCController other = allNPCs[i];
            if (other == null || other == this) continue;

            Vector3 toOther = other.transform.position - transform.position;
            toOther.y = 0f;
            float distSqr = toOther.sqrMagnitude;

            if (distSqr < 0.0001f || distSqr > radiusSqr) continue;

            float dist = Mathf.Sqrt(distSqr);
            Vector3 dirToOther = toOther / dist;
            float proximityWeight = 1f - (dist / npcSeparationRadius);

            // Penalizar si el candidato apunta hacia otro NPC cercano; premiar si se aleja
            float dot = Vector3.Dot(candidateDirection, dirToOther);
            if (dot > 0.2f)
            {
                crowdScore -= dot * (1.5f + proximityWeight * 2.0f);
            }
            else if (dot < -0.2f)
            {
                crowdScore += (-dot) * 0.5f * proximityWeight;
            }
        }

        return crowdScore;
    }

    private bool IsNpcDirectlyAhead(Vector3 direction)
    {
        if (npcSeparationRadius <= 0f) return false;

        float closeThreshold = Mathf.Min(1.2f, npcSeparationRadius * 0.5f);
        float closeThresholdSqr = closeThreshold * closeThreshold;

        for (int i = 0; i < allNPCs.Count; i++)
        {
            NPCController other = allNPCs[i];
            if (other == null || other == this) continue;

            Vector3 toOther = other.transform.position - transform.position;
            toOther.y = 0f;
            float distSqr = toOther.sqrMagnitude;

            if (distSqr > 0.0001f && distSqr <= closeThresholdSqr)
            {
                Vector3 dirToOther = toOther.normalized;
                if (Vector3.Dot(direction, dirToOther) > 0.65f)
                {
                    return true;
                }
            }
        }

        return false;
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
        patrolDistanceVariance = source.patrolDistanceVariance;
        maxWanderRadius = source.maxWanderRadius;
        rotationSpeed = source.rotationSpeed;
        waitTimeAtTurns = source.waitTimeAtTurns;
        randomizeInitialDirection = source.randomizeInitialDirection;
        obstacleCheckDistance = source.obstacleCheckDistance;
        raycastHeightOffset = source.raycastHeightOffset;
        npcSeparationRadius = source.npcSeparationRadius;
        obstacleLayer = source.obstacleLayer;
        validGroundLayer = source.validGroundLayer;

        homePosition = source.homePosition;
        segmentStartPosition = source.segmentStartPosition;
        currentLegDistance = source.currentLegDistance;
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
        Vector3 home = Application.isPlaying ? homePosition : transform.position;
        Vector3 segmentStart = Application.isPlaying ? segmentStartPosition : transform.position;
        float legDist = Application.isPlaying ? currentLegDistance : patrolDistance;
        Vector3 dir = Application.isPlaying && moveDirection.sqrMagnitude > 0.001f ? moveDirection : transform.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
        dir.Normalize();

        // Radio máximo de patrulla (maxWanderRadius = 4f)
        if (maxWanderRadius > 0f)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.5f);
            Gizmos.DrawWireSphere(home, maxWanderRadius);
        }

        // Tramo lineal actual
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(segmentStart, segmentStart + dir * legDist);
        Gizmos.DrawWireSphere(segmentStart + dir * legDist, 0.2f);

        // Rayo frontal de obstáculos
        Gizmos.color = Color.yellow;
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + dir * obstacleCheckDistance);
    }
}
