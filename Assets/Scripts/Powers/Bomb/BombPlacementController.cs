// Responsibility: Move the crosshair on the canvas and confirm its position.
// He doesn't know what the bomb does, only where it lands.

using UnityEngine;
using UnityEngine.UI;

public class BombPlacementController : MonoBehaviour
{
    [Header("Referencias")]
    public Camera enemyCamera;           
    public RectTransform crosshairUI;    
    public LayerMask validPlacementLayer; 

    [Header("Config")]
    public float crosshairSpeed = 300f;  // Velocity in pixels/seconds
    
    public static event System.Action<PlayerTeam, Vector3> OnBombPlaced;

    private bool isAiming = false;
    private Vector2 aimInput;
    private PlayerController owner;

    public bool IsAiming => isAiming;
    private void Awake()
    {
        owner = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        PlayerController.OnBombActivated += HandleBombActivated;
    }

    private void OnDisable()
    {
        PlayerController.OnBombActivated -= HandleBombActivated;
    }

    private void HandleBombActivated(PlayerController player)
    {
        if (player != owner) return;
        StartAiming();
    }

    public void StartAiming()
    {
        isAiming = true;
        crosshairUI.gameObject.SetActive(true);
        crosshairUI.anchoredPosition = Vector2.zero;
        owner.SetInputEnabled(false); 
    }

    public void StopAiming()
    {
        isAiming = false;
        crosshairUI.gameObject.SetActive(false);
        owner.SetInputEnabled(true);
    }

    private void Update()
    {
        if (!isAiming) return;
        MoveCrosshair();
    }

    private void MoveCrosshair()
    {
        Vector2 input = GetMoveInput();
        crosshairUI.anchoredPosition += input * crosshairSpeed * Time.deltaTime;
        ClampCrosshair();
    }

    private Vector2 GetMoveInput()
    {
        return owner.MoveInput;
    }

    private void ClampCrosshair()
    {
        Canvas canvas = crosshairUI.GetComponentInParent<Canvas>();
        Vector2 halfSize = canvas.GetComponent<RectTransform>().sizeDelta * 0.5f;

        crosshairUI.anchoredPosition = new Vector2(
            Mathf.Clamp(crosshairUI.anchoredPosition.x, -halfSize.x * 0.5f, halfSize.x * 0.5f),
            Mathf.Clamp(crosshairUI.anchoredPosition.y, -halfSize.y, halfSize.y)
        );
    }
    
    public void ConfirmPlacement()
    {
        if (!isAiming) return;

        Ray ray = enemyCamera.ScreenPointToRay(GetScreenPosition());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, validPlacementLayer))
        {
            StopAiming();
            OnBombPlaced?.Invoke(owner.team, hit.point);
        }
    }

    private Vector3 GetScreenPosition()
    {
        Canvas canvas = crosshairUI.GetComponentInParent<Canvas>();
        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 pos = crosshairUI.anchoredPosition;
        
        float screenX = (pos.x / canvasSize.x + 0.5f) * Screen.width;
        float screenY = (pos.y / canvasSize.y + 0.5f) * Screen.height;

        return new Vector3(screenX, screenY, 0f);
    }
}
