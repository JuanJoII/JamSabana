// Responsibility: Base collectable object. It is collected upon contact.
// It doesn't know what to do with the player; that's defined by the subclasses.

using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Collectible : MonoBehaviour
{
    [Header("Config")]
    public PlayerTeam worldTeam;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        if (player.team != worldTeam) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        Collect(inventory);
        Destroy(gameObject);
    }

    protected abstract void Collect(PlayerInventory inventory);
}