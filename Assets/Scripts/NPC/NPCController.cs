// Responsibility: To represent an NPC in the scene.
// Stores your equipment and exposes the conversion method.

using UnityEngine;
using System.Collections.Generic;
public class NPCController : MonoBehaviour
{
    public PlayerTeam team;

    [Header("Prefabs de conversión")]
    public GameObject cutePrefab;
    public GameObject darkPrefab;
    
    private static readonly List<NPCController> allNPCs = new List<NPCController>();
    public static IReadOnlyList<NPCController> AllNPCs => allNPCs;

    private void OnEnable() => allNPCs.Add(this);
    private void OnDisable() => allNPCs.Remove(this);
    
    public static event System.Action<PlayerTeam> OnNPCConverted;

    public void Convert(PlayerTeam converterTeam)
    {
        if (team == converterTeam) return; 

        team = converterTeam;
        OnNPCConverted?.Invoke(converterTeam);
        
        GameObject newPrefab = converterTeam == PlayerTeam.Cute ? cutePrefab : darkPrefab;
        if (newPrefab != null)
        {
            Instantiate(newPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}