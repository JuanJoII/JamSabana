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
    private BombPlacementController placement;

    public Vector2 MoveInput => new Vector2(moveInput.x, moveInput.z);
    
    public static event System.Action<PlayerController> OnInteractPressed;
    public static event System.Action<PlayerController> OnBombActivated;
    public static event System.Action<PlayerController> OnRiftActivated;
    public static event System.Action<PlayerController> OnConversionActivated;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isInputEnabled = true;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (inventory == null) inventory = GetComponent<PlayerInventory>();
        placement = GetComponent<BombPlacementController>();
    }

    public void OnMove(InputValue value)
    {
        if (!isInputEnabled) return;
        Vector2 raw = value.Get<Vector2>();
        moveInput = new Vector3(raw.x, 0f, raw.y);
    }

    public void OnConfirm(InputValue value)
    {
        if (!isInputEnabled && !placement.IsAiming) return; 
        if (!value.isPressed) return;

        if (placement.IsAiming)
        {
            placement.ConfirmPlacement();
            return;
        }

        if (inventory.HasPower)
            UsePower();
        else
            OnInteractPressed?.Invoke(this);
    }

    private void UsePower()
    {
        PowerType power = inventory.ConsumePower();

        switch (power)
        {
            case PowerType.Bomb:
                OnBombActivated?.Invoke(this);
                break;
            case PowerType.Rift:
                OnRiftActivated?.Invoke(this);
                break;
            case PowerType.Conversion:
                OnConversionActivated?.Invoke(this);
                break;
        }
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