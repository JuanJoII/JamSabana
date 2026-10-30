using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    IPointerClickHandler, ISelectHandler,
    IDeselectHandler, ISubmitHandler
{
    [Header("Target")]
    [Tooltip("What gets animated. If empty, uses this object's RectTransform.")]
    public RectTransform target;
    [Tooltip("Optional Graphic to tint (button image). Leave empty to skip color changes.")]
    public Graphic tintGraphic;

    [Header("Scale")]
    public float hoverScale = 1.1f;
    public float pressedScale = 0.92f;
    [Tooltip("Scale when selected by gamepad/keyboard navigation.")]
    public float selectedScale = 1.1f;

    [Header("Spring (bounce feel)")]
    [Tooltip("Higher = snappier.")]
    public float stiffness = 220f;
    [Tooltip("Higher = less bounce. Lower = more wobble.")]
    public float damping = 14f;

    [Header("Click Punch")]
    [Tooltip("Extra kick added on click. 0 = none.")]
    public float clickPunch = 4f;

    [Header("Hover Tilt (optional)")]
    [Tooltip("Degrees of Z rotation on hover. 0 = none.")]
    public float hoverTiltDegrees = 0f;

    [Header("Idle Pulse (optional)")]
    public bool idlePulse = false;
    public float idlePulseAmount = 0.03f;
    public float idlePulseSpeed = 2.5f;

    [Header("Color Tint (needs Tint Graphic)")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.white;
    public Color pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    public float colorSpeed = 14f;

    [Header("Events (use for sounds)")]
    public UnityEvent onHover;
    public UnityEvent onClick;

    private Selectable selectable;
    private Vector3 baseScale;
    private Quaternion baseRotation;
    private float scale = 1f;
    private float velocity;
    private bool isHovered, isPressed, isSelected;

    private bool Interactable => selectable == null || selectable.interactable;

    private void Awake()
    {
        if (target == null) target = (RectTransform)transform;
        selectable = GetComponent<Selectable>();
        baseScale = target.localScale;
        baseRotation = target.localRotation;
        if (tintGraphic != null) normalColor = tintGraphic.color;
    }

    private void OnEnable()
    {
        scale = 1f;
        velocity = 0f;
        isHovered = isPressed = isSelected = false;
        Apply(0f);
        if (tintGraphic != null) tintGraphic.color = normalColor;
    }

    private void OnDisable()
    {
        if (target == null) return;
        target.localScale = baseScale;
        target.localRotation = baseRotation;
        if (tintGraphic != null) tintGraphic.color = normalColor;
    }

    public void OnSubmit(BaseEventData e)
    {
        if (!Interactable) return;
        velocity += clickPunch;
        onClick?.Invoke();
    }

    private void Update()
    {
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.033f);

        // Spring toward the target scale
        float goal = GetGoalScale();
        velocity += (goal - scale) * stiffness * dt;
        velocity *= Mathf.Exp(-damping * dt);
        scale += velocity * dt;

        Apply(dt);
    }

    private float GetGoalScale()
    {
        if (!Interactable) return 1f;

        float goal = 1f;
        if (isPressed) goal = pressedScale;
        else if (isHovered) goal = hoverScale;
        else if (isSelected) goal = selectedScale;

        if (idlePulse && !isHovered && !isPressed && !isSelected)
            goal += Mathf.Sin(Time.unscaledTime * idlePulseSpeed) * idlePulseAmount;

        return goal;
    }

    private void Apply(float dt)
    {
        target.localScale = baseScale * scale;

        float tilt = (isHovered && Interactable) ? hoverTiltDegrees : 0f;
        Quaternion goalRot = baseRotation * Quaternion.Euler(0f, 0f, tilt);
        target.localRotation = Quaternion.Slerp(target.localRotation, goalRot, 1f - Mathf.Exp(-12f * dt));

        if (tintGraphic != null)
        {
            Color goalColor = isPressed ? pressedColor : (isHovered || isSelected ? hoverColor : normalColor);
            tintGraphic.color = Color.Lerp(tintGraphic.color, goalColor, 1f - Mathf.Exp(-colorSpeed * dt));
        }
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!Interactable) return;
        isHovered = true;
        onHover?.Invoke();
    }

    public void OnPointerExit(PointerEventData e)
    {
        isHovered = false;
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!Interactable) return;
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData e) => isPressed = false;

    public void OnPointerClick(PointerEventData e)
    {
        if (!Interactable) return;
        velocity += clickPunch;
        onClick?.Invoke();
    }

    public void OnSelect(BaseEventData e)
    {
        if (!Interactable) return;
        isSelected = true;
        onHover?.Invoke();
    }

    public void OnDeselect(BaseEventData e) => isSelected = false;
}