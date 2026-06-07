// Responsibility: Travel in a straight line and convert the first NPC you touch.

using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ConversionProjectile : MonoBehaviour
{
    [Header("Config")]
    public float speed = 15f;
    public float lifetime = 3f;

    private PlayerTeam attackerTeam;
    private Vector3 direction;

    public void Initialize(PlayerTeam attacker, Vector3 shootDirection)
    {
        attackerTeam = attacker;
        direction = shootDirection.normalized;

        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = direction * speed;

        GetComponent<Collider>().isTrigger = true;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        NPCController npc = other.GetComponent<NPCController>();
        if (npc == null) return;

        npc.Convert(attackerTeam);
        Destroy(gameObject);
    }
}
