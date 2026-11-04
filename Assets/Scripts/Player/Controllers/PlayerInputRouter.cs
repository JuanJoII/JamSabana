// Responsibility: Receive raw input from PlayerInput (Behavior: Send Messages) and expose it.
// It knows nothing about movement, powers or game rules.
// Must live on the same GameObject as the PlayerInput component.

using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputRouter : MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; }

    /// <summary>Current move value, always up to date (even while gameplay input is disabled).</summary>
    public Vector2 Move { get; private set; }

    public event Action ConfirmPressed;

    [Header("Pause")]
    [SerializeField] private PauseController pauseController;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        if (pauseController == null) pauseController = FindFirstObjectByType<PauseController>();
    }

    private void OnDisable()
    {
        Move = Vector2.zero;
    }

    // Called by PlayerInput. Method names must match the actions in the asset.
    public void OnMove(InputValue value) => Move = value.Get<Vector2>();

    public void OnConfirm(InputValue value)
    {
        if (value.isPressed) ConfirmPressed?.Invoke();
    }

    public void OnPause(InputValue value)
    {
        Debug.Log($"Pause | {name} | scheme: {PlayerInput.currentControlScheme} | pressed: {value.isPressed} | menuAbierto: {(pauseController != null ? pauseController.IsPauseMenuOpen.ToString() : "SIN PauseController")}");
        if (value.isPressed && pauseController != null) pauseController.TogglePause();
    }
}