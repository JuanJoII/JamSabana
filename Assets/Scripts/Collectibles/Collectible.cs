// Responsibility: Base collectable object. It is collected upon contact.
// It doesn't know what to do with the player; that's defined by the subclasses.

using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Collectible : MonoBehaviour
{
    private void Awake()
    {
        // Asegura que el collider sea trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        Collect(inventory);
        Destroy(gameObject);
    }

    protected abstract void Collect(PlayerInventory inventory);
}