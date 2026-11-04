// Responsibility: Move the ship over the enemy world, verify valid zone
// and confirm where the bomb falls.

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BombShip : MonoBehaviour
{
    [Header("Config")]
    public float moveSpeed = 10f;
    public float shipHeight = 10f;
    [SerializeField] private PlacementRules placementRules;
    [SerializeField] private float groundRayExtra = 5f;

    [Header("Límites mundo enemigo")]
    public float worldHalfSize = 25f;

    [Header("Visualización")]
    public LineRenderer aimLine;

    public static event System.Action<PlayerTeam, Vector3> OnBombPlaced;
    public static event System.Action<PlayerTeam, Vector3> OnBombPlacementRejected;

    public bool IsActive => isActive;

    private PlayerController owner;
    private Rigidbody rb;
    private bool isActive;
    private Vector3 activationCenter;

    private float RayDistance => shipHeight + groundRayExtra;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        PlayerController.OnBombActivated += HandleBombActivated;
        SetVisible(false);

        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.startWidth = 0.5f;
            aimLine.endWidth = 0.5f;
        }
    }

    private void OnDestroy()
    {
        PlayerController.OnBombActivated -= HandleBombActivated;
    }

    public void Initialize(PlayerController playerOwner)
    {
        owner = playerOwner;
    }

    private void HandleBombActivated(PlayerController player)
    {
        if (player != owner) return;
        Activate();
    }

    private void Activate()
    {
        isActive = true;
        SetVisible(true);
        activationCenter = transform.position;
        owner.SetInputEnabled(false);
    }

    private void Deactivate()
    {
        isActive = false;
        SetVisible(false);
        owner.SetInputEnabled(true);
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
    }

    private void Update()
    {
        if (!isActive) return;
        MoveShip();
        UpdateAimLine();
    }

    private void MoveShip()
    {
        Vector2 input = owner.RawMoveInput;
        Vector3 movement = new Vector3(input.x, 0f, input.y) * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;

        newPos.x = Mathf.Clamp(newPos.x, activationCenter.x - worldHalfSize, activationCenter.x + worldHalfSize);
        newPos.z = Mathf.Clamp(newPos.z, activationCenter.z - worldHalfSize, activationCenter.z + worldHalfSize);
        newPos.y = shipHeight;

        transform.position = newPos;
    }

    public void ConfirmPlacement()
    {
        if (!isActive) return;

        // Valid = there is floor below AND the point is not inside a "NoPlacement" zone.
        if (!placementRules.TryGetPlacement(transform.position, RayDistance, out Vector3 groundPoint))
        {
            OnBombPlacementRejected?.Invoke(owner.team, transform.position);
            return;
        }

        Vector3 spawnPos = new Vector3(groundPoint.x, transform.position.y, groundPoint.z);
        BombSpawner.SpawnWithDrop(owner.team, spawnPos, groundPoint);
        Deactivate();
    }

    private void UpdateAimLine()
    {
        if (aimLine == null) return;

        bool hasGround = placementRules.TryRaycastGround(transform.position, RayDistance, out RaycastHit hit);
        bool isValid = hasGround && !placementRules.IsBlocked(hit.point);

        Vector3 endPoint = hasGround ? hit.point : transform.position + Vector3.down * RayDistance;
        Color lineColor = isValid ? Color.green : Color.lightCoral;

        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1, endPoint);
        aimLine.startColor = lineColor;
        aimLine.endColor = lineColor;
    }
}