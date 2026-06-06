using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    public PlayerTeam team;
    public float moveSpeed = 5f;

    [Header("References")]
    public PlayerInventory inventory;

    public static event System.Action<PlayerController> OnInteractPressed;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isInputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();
    }

    public void OnMove(InputValue value)
    {
        if (!isInputEnabled) return;
        Vector2 raw = value.Get<Vector2>();
        moveInput = new Vector3(raw.x, 0f, raw.y);
    }

    public void OnConfirm(InputValue value)
    {
        if (!isInputEnabled) return;
        if (value.isPressed)
            OnInteractPressed?.Invoke(this);
    }

    private void FixedUpdate()
    {
        if (!isInputEnabled)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
        if (!enabled) rb.linearVelocity = Vector3.zero;
    }
}