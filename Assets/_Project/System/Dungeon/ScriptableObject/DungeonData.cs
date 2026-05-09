using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Information about an enemy that can spawn in a dungeon.
/// </summary>
[System.Serializable]
public struct EnemySpawnWeight
{
    public GameObject enemyPrefab;
    [Range(0, 100)] public int spawnWeight;
    public int minRoundRequirement;
}

/// <summary>
/// Contains all the metadata for a dungeon.
/// Enemy spawn pool, Round scaling, Wave settings, Game Location Tag.
/// </summary>
[CreateAssetMenu(fileName = "DungeonData", menuName = "Dungeon/Dungeon Data")]
public class DungeonData : ScriptableObject
{
    [Header("Dungeon Metadata")] 
    public GameLocation dungeonLocation;

    [Header("Round Scaling")] 
    public int baseEnemyCount = 5;
    public int enemiesPerRoundScaling = 2;
    public float delayBetweenRounds = 5f;
    
    [Header("Wave Settings")]
    public int enemiesPerWave = 3;
    public float delayBetweenWaves = 2f;

    [Header("Enemy Pool")] 
    public List<EnemySpawnWeight> enemySpawnPool;
    
    /// <summary>
    /// Finds all eligible enemies for the current round and returns a random one
    /// </summary>
    public GameObject SelectRandomEligibleEnemy(int currentRound)
    {
        int totalWeight = 0;

        // Check every eligible enemy and add their spawn weights
        foreach (EnemySpawnWeight enemy in enemySpawnPool)
            if (currentRound >= enemy.minRoundRequirement) totalWeight += enemy.spawnWeight;
        
        if (totalWeight <= 0) return null;
        
        int randomValue = Random.Range(0, totalWeight);
        int cursor = 0;

        // Selec
        foreach (EnemySpawnWeight enemy in enemySpawnPool)
        {
            // Skip non-eligible enemies
            if (currentRound < enemy.minRoundRequirement) continue;
            
            // 
            cursor += enemy.spawnWeight;
            if (cursor > randomValue) return enemy.enemyPrefab;
        }

        return null;
    }
}
