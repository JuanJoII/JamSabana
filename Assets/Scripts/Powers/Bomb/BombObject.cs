// Responsibility: Bomb lifecycle once placed.
// Listens for OnBombPlaced to spawn.
// Fires OnBombExploded and OnBombDefused

using UnityEngine;
using System.Collections;

public class BombObject : MonoBehaviour
{
    [Header("Config")]
    public float countdownSeconds = 10f;
    
    public static event System.Action<PlayerTeam, Vector3> OnBombExploded;
    public static event System.Action<PlayerTeam> OnBombDefused;

    private PlayerTeam attackingTeam;
    private Coroutine countdown;
    
    private void OnEnable()
    {
        PlayerController.OnInteractPressed += HandleInteract;
    }

    private void OnDisable()
    {
        PlayerController.OnInteractPressed -= HandleInteract;
    }

    private void HandleInteract(PlayerController player)
    {
        if (player.team == attackingTeam) return;
        
        if (Vector3.Distance(player.transform.position, transform.position) > 2f) return;

        Defuse();
    }
    public void Initialize(PlayerTeam attacker, Vector3 position)
    {
        attackingTeam = attacker;
        transform.position = position;
        countdown = StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(countdownSeconds);
        Explode();
    }
    
    public void Defuse()
    {
        if (countdown != null) StopCoroutine(countdown);
        OnBombDefused?.Invoke(attackingTeam);
        Destroy(gameObject);
    }

    private void Explode()
    {
        OnBombExploded?.Invoke(attackingTeam, transform.position);
        Destroy(gameObject);
    }
}
