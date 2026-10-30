using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    public PlayerTeam team;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("References")]
    public PlayerInventory inventory;
    public BombShip bombShip;

    public Vector2 MoveInput => new Vector2(moveInput.x, moveInput.z);
    public Vector2 RawMoveInput { get; private set; }
    public bool IsRiftImmune { get; private set; } = false;

    public static event System.Action<PlayerController> OnInteractPressed;
    public static event System.Action<PlayerController> OnBombActivated;
    public static event System.Action<PlayerController> OnRiftActivated;
    public static event System.Action<PlayerController> OnConversionActivated;
    public static event System.Action<PlayerController, bool> OnRiftImmunityChanged;

    public void ActivateBomb() => OnBombActivated?.Invoke(this);
    public void ActivateRift() => OnRiftActivated?.Invoke(this);
    public void ActivateConversion() => OnConversionActivated?.Invoke(this);

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isInputEnabled = true;
    private Coroutine riftImmunityRoutine;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (inventory == null) inventory = GetComponent<PlayerInventory>();
    }
    private void Start()
    {
        if (bombShip != null)
            bombShip.Initialize(this);
    }

    public void OnMove(InputValue value)
    {
        Vector2 raw = value.Get<Vector2>();
        RawMoveInput = raw; 

        if (!isInputEnabled) return;
        moveInput = new Vector3(raw.x, 0f, raw.y);
    }

    public void OnConfirm(InputValue value)
    {
        if (!value.isPressed) return;
        
        if (bombShip != null && bombShip.IsActive)
        {
            bombShip.ConfirmPlacement();
            return;
        }

        if (!isInputEnabled) return;
        OnInteractPressed?.Invoke(this);
    }

    private void UsePower()
    {
        PowerType power = inventory.ConsumePower();

        switch (power)
        {
            case PowerType.Bomb:
                Debug.Log($"ActivateBomb llamado | team: {team}");
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

        if (bombShip != null && bombShip.IsActive)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
        
        HandleRotation();
    }
    private void HandleRotation()
    {
   
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            
            rb.MoveRotation(smoothedRotation);
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
        if (!enabled)
        {
            moveInput = Vector3.zero; 
            rb.linearVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Otorga inmunidad temporal contra caer en grietas, evitando spawn-traps y bucles infinitos.
    /// </summary>
    public void SetRiftImmunity(float duration)
    {
        if (riftImmunityRoutine != null)
        {
            StopCoroutine(riftImmunityRoutine);
        }
        riftImmunityRoutine = StartCoroutine(RiftImmunityRoutine(duration));
    }

    private System.Collections.IEnumerator RiftImmunityRoutine(float duration)
    {
        IsRiftImmune = true;
        OnRiftImmunityChanged?.Invoke(this, true);

        yield return new WaitForSeconds(duration);

        IsRiftImmune = false;
        OnRiftImmunityChanged?.Invoke(this, false);
        riftImmunityRoutine = null;
    }
}
