// Responsibility: Move the ship over the enemy world, verify valid zone
// and confirm where the bomb falls.

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BombShip : MonoBehaviour
{
    [Header("Config")]
    public float moveSpeed = 10f;
    public float shipHeight = 10f;       
    public float bombDropHeight = 1f;    
    public LayerMask validPlacementLayer;

    public static event System.Action<PlayerTeam, Vector3> OnBombPlaced;

    private PlayerController owner;
    private Rigidbody rb;
    private bool isActive = false;
    
    public bool IsActive => isActive;
    
    [Header("Límites mundo enemigo")]
    public float worldHalfSize = 25f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        PlayerController.OnBombActivated += HandleBombActivated;
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
        gameObject.SetActive(true);
        
        float enemyX = owner.team == PlayerTeam.Cute ? 50f : -50f;
        transform.position = new Vector3(enemyX, shipHeight, 0f);

        owner.SetInputEnabled(false);
    }

    private void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
        owner.SetInputEnabled(true);
    }

    private void Update()
    {
        if (!isActive) return;
        MoveShip();
    }

    private void MoveShip()
    {
        Vector2 input = owner.RawMoveInput;
        Vector3 movement = new Vector3(input.x, 0f, input.y) * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;

        // Clamp so that he doesn't leave the enemy world
        float enemyX = owner.team == PlayerTeam.Cute ? 50f : -50f;
        newPos.x = Mathf.Clamp(newPos.x, enemyX - worldHalfSize, enemyX + worldHalfSize);
        
        newPos.y = shipHeight;
        
        if (HasValidGroundBelow(newPos))
            transform.position = newPos;
    }

    private bool HasValidGroundBelow(Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        bool hit = Physics.Raycast(ray, out _, shipHeight + 5f, validPlacementLayer);
        Debug.DrawRay(position, Vector3.down * (shipHeight + 5f), hit ? Color.green : Color.red);
        return hit;
    }

    public void ConfirmPlacement()
    {
        if (!isActive) return;

        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, shipHeight + 5f, validPlacementLayer))
        {
            Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.blue, 2f);
            Debug.Log($"Bomba confirmada en: {hit.point} | superficie: {hit.collider.name}");

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
}