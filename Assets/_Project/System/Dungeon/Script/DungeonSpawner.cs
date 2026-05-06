using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles spawning enemies in a dungeon.
/// </summary>
public class DungeonSpawner : MonoBehaviour
{
    [Header("References")]
    private DungeonEnemyTracker _dungeonEnemyTracker;
    
    private Coroutine _spawnRoutine;

    private void Awake() => _dungeonEnemyTracker = GetComponent<DungeonEnemyTracker>();
    
    // Called by the DungeonController when a new round starts
    public void StartSpawning(DungeonData currentDungeon, DungeonZone dungeonZone, int currentRound, int totalToSpawn)
    {
        // Safety check
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        
        _spawnRoutine = StartCoroutine(SpawnWaveRoutine(currentDungeon, dungeonZone, currentRound, totalToSpawn));
    }

    // Called by the DungeonController when the current round is cleared
    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    private IEnumerator SpawnWaveRoutine(DungeonData currentDungeon, DungeonZone dungeonZone, int currentRound, int totalToSpawn)
    {
        int enemiesSpawned = 0;

        // Loop until all enemies are spawned
        while (enemiesSpawned < totalToSpawn)
        {
            int remainingEnemies = Mathf.Min(currentDungeon.enemiesPerWave, totalToSpawn - enemiesSpawned);
            Debug.Log($"Spawning Wave: {remainingEnemies} Enemies");

            for (int i = 0; i < remainingEnemies; i++)
            {
                // Select a random enemy
                GameObject enemyPrefab = currentDungeon.SelectRandomEligibleEnemy(currentRound);
                // Select a random spawn point
                Transform spawnPoint = dungeonZone.enemySpawnPoints[Random.Range(0, dungeonZone.enemySpawnPoints.Count)];
                
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                _dungeonEnemyTracker.RegisterEnemy(enemy);
                enemiesSpawned++;
            }
            
            if (enemiesSpawned < totalToSpawn)
                yield return new WaitForSeconds(currentDungeon.delayBetweenWaves);
        }
        
        Debug.Log($"All enemies spawned in round {currentRound}");
    }
}
