//Responsibility: Listens for OnBombPlaced and instantiates BombObject.

using UnityEngine;
using System.Collections;

public class BombSpawner : MonoBehaviour
{
    public GameObject bombPrefab;

    private static BombSpawner instance;

    private void Awake()
    {
        instance = this;
    }

    public static void SpawnWithDrop(PlayerTeam attacker, Vector3 spawnPos, Vector3 groundPos)
    {
        instance.StartCoroutine(instance.DropBomb(attacker, spawnPos, groundPos));
    }

    private IEnumerator DropBomb(PlayerTeam attacker, Vector3 spawnPos, Vector3 groundPos)
    {
        GameObject obj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);
        BombObject bomb = obj.GetComponent<BombObject>();

        float dropDuration = 0.4f;
        float elapsed = 0f;
        
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;

            obj.transform.position = Vector3.Lerp(spawnPos, groundPos, t * t);
            yield return null;
        }

        obj.transform.position = groundPos;
        bomb.Initialize(attacker, groundPos);
    }
}
