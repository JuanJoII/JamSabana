//Responsibility: Listens for OnBombPlaced and instantiates BombObject.

using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    public GameObject bombPrefab;

    private void OnEnable()
    {
        BombPlacementController.OnBombPlaced += HandleBombPlaced;
    }

    private void OnDisable()
    {
        BombPlacementController.OnBombPlaced -= HandleBombPlaced;
    }

    private void HandleBombPlaced(PlayerTeam attacker, Vector3 position)
    {
        GameObject obj = Instantiate(bombPrefab, position, Quaternion.identity);
        obj.GetComponent<BombObject>().Initialize(attacker, position);
    }
}
