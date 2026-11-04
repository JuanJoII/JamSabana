// Responsibility: Player identity (team), movement and the entry point for interaction/powers.
// Raw input lives in PlayerInputRouter.

using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInputRouter))]
public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    public PlayerTeam team;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    [SerializeField] private float groundCheckDistance = 1.5f;
    [SerializeField] private float groundForce = 20f;

    [Header("References")]
    public PlayerInventory inventory;
    public BombShip bombShip;

    public static event System.Action<PlayerController> OnInteractPressed;
    public static event System.Action<PlayerController> OnBombActivated;
    public static event System.Action<PlayerController> OnRiftActivated;
    public static event System.Action<PlayerController> OnConversionActivated;

    public void ActivateBomb() => OnBombActivated?.Invoke(this);
    public void ActivateRift() => OnRiftActivated?.Invoke(this);
    public void ActivateConversion() => OnConversionActivated?.Invoke(this);

    /// <summary>Raw input, ignores SetInputEnabled (used by the bomb ship).</summary>
    public Vector2 RawMoveInput => inputRouter.Move;

    /// <summary>Gameplay input: zero while input is disabled.</summary>
    public Vector2 MoveInput => isInputEnabled ? inputRouter.Move : Vector2.zero;

    private Rigidbody rb;
    private PlayerInputRouter inputRouter;
    private bool isInputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputRouter = GetComponent<PlayerInputRouter>();
        if (inventory == null) inventory = GetComponent<PlayerInventory>();
    }

    private void OnEnable()
    {
        inputRouter.ConfirmPressed += HandleConfirm;
    }

    private void OnDisable()
    {
        inputRouter.ConfirmPressed -= HandleConfirm;
    }

    private void Start()
    {
        if (bombShip != null)
            bombShip.Initialize(this);
    }

    private void HandleConfirm()
    {
        if (bombShip != null && bombShip.IsActive)
        {
            bombShip.ConfirmPlacement();
            return;
        }

        if (!isInputEnabled) return;
        OnInteractPressed?.Invoke(this);
    }

    private void FixedUpdate()
    {
        CheckGround();

        // The router always holds the current input, so re-enabling input
        // picks up a key that is still held (no need to release and press again).
        Vector2 move = MoveInput;
        Vector3 direction = new Vector3(move.x, 0f, move.y);

        // Keep the vertical velocity so gravity works. If movement feels odd,
        // go back to: rb.linearVelocity = direction * moveSpeed;
        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);

        HandleRotation(direction);
    }

    private void HandleRotation(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothedRotation);
    }

    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
        if (!enabled)
            rb.linearVelocity = Vector3.zero;
    }

    /// <summary>Single entry point to move the player instantly (rifts, returns, spawns).</summary>
    public void TeleportTo(Vector3 position)
    {
        rb.position = position;
        transform.position = position;
        rb.linearVelocity = Vector3.zero;
    }

    private void CheckGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance))
            rb.AddForce(Vector3.down * groundForce, ForceMode.Acceleration);
    }
}