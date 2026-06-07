// Responsabilidad: Teletransportar al jugador al mundo enemigo,
// manejar el disparo con cooldown por animación y el timer de regreso.

using UnityEngine;
using System.Collections;

public class ConversionPower : MonoBehaviour
{
    [Header("Config")]
    public float duration = 20f;
    public GameObject projectilePrefab;
    public Transform shootPoint; 

    [Header("Cooldown")]
    public float shootCooldown = 0.5f; 

    public static event System.Action<PlayerTeam> OnConversionPhaseStarted;
    public static event System.Action<PlayerTeam> OnConversionPhaseEnded;

    private PlayerController owner;
    private Animator animator;
    private bool isInEnemyWorld = false;
    private bool canShoot = true;
    private Coroutine conversionRoutine;

    public static event System.Action<PlayerController> OnShootFired;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        PlayerController.OnConversionActivated += HandleConversionActivated;
        PlayerController.OnInteractPressed += HandleShoot;
    }

    private void OnDisable()
    {
        PlayerController.OnConversionActivated -= HandleConversionActivated;
        PlayerController.OnInteractPressed -= HandleShoot;
    }

    private void HandleConversionActivated(PlayerController player)
    {
        if (player != owner) return;
        if (isInEnemyWorld) return;
        conversionRoutine = StartCoroutine(ConversionPhase());
    }

    private IEnumerator ConversionPhase()
    {
        isInEnemyWorld = true;
        canShoot = true;
        
        Vector3 originalPosition = owner.transform.position;
        PlayerTeam enemyWorld = owner.team == PlayerTeam.Cute ? PlayerTeam.Dark : PlayerTeam.Cute;
        Transform spawn = WorldSpawnPoints.GetSpawnFor(enemyWorld, owner.team);
        owner.transform.position = spawn.position;

        OnConversionPhaseStarted?.Invoke(owner.team);

        yield return new WaitForSeconds(duration);

        ReturnToOwnWorld(originalPosition);
    }

    private void HandleShoot(PlayerController player)
    {
        if (player != owner) return;
        if (!isInEnemyWorld) return;
        if (!canShoot) return;

        Shoot();
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector3 shootDirection = owner.transform.forward;
        if (shootDirection == Vector3.zero)
            shootDirection = new Vector3(owner.RawMoveInput.x, 0f, owner.RawMoveInput.y);

        Transform spawnTransform = shootPoint != null ? shootPoint : owner.transform;
        GameObject proj = Instantiate(projectilePrefab, spawnTransform.position, Quaternion.identity);
        proj.GetComponent<ConversionProjectile>().Initialize(owner.team, shootDirection);

        OnShootFired?.Invoke(owner); 
        StartCoroutine(ShootCooldown());
    }

    private IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    private void ReturnToOwnWorld(Vector3 originalPosition)
    {
        isInEnemyWorld = false;
        owner.transform.position = originalPosition;
        OnConversionPhaseEnded?.Invoke(owner.team);
    }
}