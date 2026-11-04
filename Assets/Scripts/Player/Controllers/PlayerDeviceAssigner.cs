// Responsibility: Decide which device each player uses.
//  - Left player : keyboard scheme "KeyboardLeft"  (WASD)   -> or a gamepad if one is connected.
//  - Right player: keyboard scheme "KeyboardRight" (arrows) -> or a gamepad if one is connected.
// Gamepads fill the slots in order (first gamepad -> left, second -> right).
// Reacts to gamepads being plugged / unplugged during play.

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeviceAssigner : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerInputRouter leftPlayer;
    [SerializeField] private PlayerInputRouter rightPlayer;

    [Header("Control scheme names (must match the Input Actions asset)")]
    [SerializeField] private string leftKeyboardScheme = "KeyboardLeft";
    [SerializeField] private string rightKeyboardScheme = "KeyboardRight";
    [SerializeField] private string gamepadScheme = "Gamepad";

    private void OnEnable()
    {
        InputSystem.onDeviceChange += HandleDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= HandleDeviceChange;
    }

    // Start (not Awake/OnEnable) so every PlayerInput has finished initializing.
    private void Start()
    {
        AssignDevices();
    }

    private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad) return;

        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed ||
            change == InputDeviceChange.Reconnected || change == InputDeviceChange.Disconnected)
        {
            AssignDevices();
        }
    }

    private void AssignDevices()
    {
        var pads = Gamepad.all;
        Assign(leftPlayer, leftKeyboardScheme, pads.Count > 0 ? pads[0] : null);
        Assign(rightPlayer, rightKeyboardScheme, pads.Count > 1 ? pads[1] : null);
    }

    private void Assign(PlayerInputRouter router, string keyboardScheme, Gamepad pad)
    {
        if (router == null) return;

        PlayerInput input = router.PlayerInput;

        // Both players share one Keyboard device; schemes decide which keys belong to whom.
        // Auto-switch would make them fight over it.
        input.neverAutoSwitchControlSchemes = true;

        if (pad != null)
            input.SwitchCurrentControlScheme(gamepadScheme, pad);
        else if (Keyboard.current != null)
            input.SwitchCurrentControlScheme(keyboardScheme, Keyboard.current);
    }
}
