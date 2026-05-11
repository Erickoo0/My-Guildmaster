using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attached to a GameObject that represents a dungeon zone.
/// Contains a list of spawn points for enemies and ties the GameLocation tag to the zone.
/// </summary>
public class DungeonZone : MonoBehaviour
{
    public GameLocation zoneLocation;
    [HideInInspector] public List<Transform> enemySpawnPoints = new List<Transform>();
    
    // A global registry of all zones in the game
    public static Dictionary<GameLocation, DungeonZone> Registry = new Dictionary<GameLocation, DungeonZone>();

    private void Awake()
    {
        // Link the zone to the GameLocation tag and register it
        Registry[zoneLocation] = this;
        
        // Grab all childed transform to act as spawn points
        Transform container = transform.Find("SpawnPoints");
        if (container != null)
        {
            // 3. Loop through all children of the SpawnPoints folder
            foreach (Transform point in container)
            {
                enemySpawnPoints.Add(point);
            }
        }
        else
        {
            // Error handling so you know if you made a typo in the Hierarchy
            Debug.LogError($"[DungeonZone] {gameObject.name} is missing a child named 'SpawnPoints'!");
        }
    }
    
    private void OnDestroy() => Registry.Remove(zoneLocation);
}
