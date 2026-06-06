using UnityEngine;

public class WorldSpawnPoints : MonoBehaviour
{
    public PlayerTeam worldTeam; 

    [Header("Spawn points")]
    public Transform cuteSpawn; 
    public Transform darkSpawn; 
    
    private static WorldSpawnPoints cuteWorld;
    private static WorldSpawnPoints darkWorld;

    private void Awake()
    {
        if (worldTeam == PlayerTeam.Cute) cuteWorld = this;
        else darkWorld = this;
    }

    public static Transform GetSpawnFor(PlayerTeam worldTeam, PlayerTeam playerTeam)
    {
        WorldSpawnPoints world = worldTeam == PlayerTeam.Cute ? cuteWorld : darkWorld;
        return playerTeam == PlayerTeam.Cute ? world.cuteSpawn : world.darkSpawn;
    }
}