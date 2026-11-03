using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

    [Header("Feedback Visual de Inmunidad")]
    [Tooltip("Intervalo normal de parpadeo durante la inmunidad a grietas.")]
    [SerializeField] private float immunityBlinkInterval = 0.12f;

    [Tooltip("Intervalo rápido de parpadeo cuando la inmunidad está por terminar.")]
    [SerializeField] private float immunityFastBlinkInterval = 0.05f;

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
    private readonly List<Renderer> characterRenderers = new List<Renderer>();
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (inventory == null) inventory = GetComponent<PlayerInventory>();
        CacheCharacterRenderers();
    }

    private void Start()
    {
        if (bombShip != null)
            bombShip.Initialize(this);
    }

    private void OnDisable()
    {
        SetCharacterRenderersVisible(true);
        IsRiftImmune = false;
        riftImmunityRoutine = null;
    }

    private void CacheCharacterRenderers()
    {
        characterRenderers.Clear();
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer r = allRenderers[i];
            if (r is MeshRenderer || r is SkinnedMeshRenderer)
            {
                characterRenderers.Add(r);
            }
        }
    }

    private void SetCharacterRenderersVisible(bool visible)
    {
        for (int i = 0; i < characterRenderers.Count; i++)
        {
            if (characterRenderers[i] != null)
            {
                characterRenderers[i].enabled = visible;
            }
        }
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
        if (duration <= 0f) return;

        if (riftImmunityRoutine != null)
        {
            StopCoroutine(riftImmunityRoutine);
            SetCharacterRenderersVisible(true);
        }
        riftImmunityRoutine = StartCoroutine(RiftImmunityRoutine(duration));
    }

    private System.Collections.IEnumerator RiftImmunityRoutine(float duration)
    {
        IsRiftImmune = true;
        OnRiftImmunityChanged?.Invoke(this, true);

        float elapsed = 0f;
        bool isVisible = true;

        while (elapsed < duration)
        {
            float remainingRatio = 1f - (elapsed / duration);
            float currentInterval = remainingRatio <= 0.3f
                ? Mathf.Max(0.02f, immunityFastBlinkInterval)
                : Mathf.Max(0.03f, immunityBlinkInterval);

            isVisible = !isVisible;
            SetCharacterRenderersVisible(isVisible);

            yield return new WaitForSeconds(currentInterval);
            elapsed += currentInterval;
        }

        SetCharacterRenderersVisible(true);
        IsRiftImmune = false;
        OnRiftImmunityChanged?.Invoke(this, false);
        riftImmunityRoutine = null;
    }
}
