// Responsibility: Move the ship over the enemy world, verify valid zone
// and confirm where the bomb falls.

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BombShip : MonoBehaviour
{
    [Header("Config")]
    public float moveSpeed = 10f;
    public float shipHeight = 10f;       
    public LayerMask validPlacementLayer;

    public static event System.Action<PlayerTeam, Vector3> OnBombPlaced;

    private PlayerController owner;
    private Rigidbody rb;
    private bool isActive = false;
    
    public bool IsActive => isActive;
    
    [Header("Límites mundo enemigo")]
    public float worldHalfSize = 25f;
    private Vector3 activationCenter;
    
    [Header("Visualización")]
    public LineRenderer aimLine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        PlayerController.OnBombActivated += HandleBombActivated;
        GetComponent<Renderer>().enabled = false;
        
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
    private void HandleBombActivated(PlayerController player)
    {
        Debug.Log($"BombShip recibió OnBombActivated | player: {player.team} | owner: {(owner == null ? "NULL" : owner.team.ToString())}");
        if (player != owner) return;
        Activate();
    }

    public void Initialize(PlayerController playerOwner)
    {
        owner = playerOwner;
    }

    private void Activate()
    {
        isActive = true;
        GetComponent<Renderer>().enabled = true;
        SetVisible(true);
        activationCenter = transform.position; 
        owner.SetInputEnabled(false);
    }

    private void Deactivate()
    {
        isActive = false;
        GetComponent<Renderer>().enabled = false;
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

    private bool HasValidGroundBelow(Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        bool hit = Physics.Raycast(ray, out _, shipHeight + 20f, validPlacementLayer);
        Debug.DrawRay(position, Vector3.down * (shipHeight + 20f), hit ? Color.green : Color.red, 6f);
        return hit;
    }

    public void ConfirmPlacement()
    {
        
        if (!isActive) return;

        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, shipHeight + 5f, validPlacementLayer))
        {
            Vector3 spawnPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            BombSpawner.SpawnWithDrop(owner.team, spawnPos, hit.point);
            Deactivate();
        }
        else
        {
            Debug.Log("ConfirmPlacement sin hit — revisa el layer del suelo");
            Debug.DrawRay(transform.position, Vector3.down * (shipHeight + 5f), Color.yellow, 2f);
        }
    }
    private void UpdateAimLine()
    {
        if (aimLine == null) return;

        Ray ray = new Ray(transform.position, Vector3.down);
        Vector3 endPoint;
        Color lineColor;
        float rayDistance = shipHeight + 5f;
        Debug.Log($"Raycast desde: {transform.position} | distancia: {rayDistance} | layer: {validPlacementLayer.value}");


        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, validPlacementLayer))
        {
            endPoint = hit.point;
            lineColor = Color.green;
        }
        else
        {
            endPoint = transform.position + Vector3.down * (shipHeight + 5f);
            lineColor = Color.red; 
        }

        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1, endPoint);
        aimLine.startColor = lineColor;
        aimLine.endColor = lineColor;
    }
}