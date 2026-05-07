using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class DungeonController : MonoBehaviour
{
    [Header("References")]
    private DungeonSpawner _dungeonSpawner;
    private DungeonEnemyTracker _dungeonEnemyTracker;
    
    [Header("Dungeon Database")]
    [Tooltip("A list of all biomes/dungeons.")]
    [SerializeField] private List<DungeonData> dungeonLibrary;

    private DungeonData _currentDungeon;
    private DungeonZone _currentZone; 
    private int _currentRound;
    private bool _isDungeonActive = false;

    public event Action OnDungeonStarted;
    public event Action OnDungeonEnded;
    public event Action<int> OnRoundStarted;
    
    private void Awake()
    {
        _dungeonSpawner = GetComponent<DungeonSpawner>();
        _dungeonEnemyTracker = GetComponent<DungeonEnemyTracker>();
    }

    private void Start()
    {
        LocationManager.Instance.OnLocationChanged += CheckIfDungeon;
        
        if (_dungeonEnemyTracker != null)
            _dungeonEnemyTracker.OnAllEnemiesCleared += StartNextRound;
    }
    
    private void OnDisable()
    {
        LocationManager.Instance.OnLocationChanged -= CheckIfDungeon;
        
        if (_dungeonEnemyTracker != null)
            _dungeonEnemyTracker.OnAllEnemiesCleared -= StartNextRound;
    }

    private void CheckIfDungeon(GameLocation newLocation)
    {
        // 1. Checks if the new location is a dungeon by checking the dungeon library
        // Does the library contain any DungeonData with the new GameLocation?
        DungeonData foundDungeon = dungeonLibrary.Find(d => d.dungeonLocation == newLocation);

        // 2. If a dungeon is found, check if the associated zone (GameObject) is registered and exists
        if (foundDungeon != null)
        {
            if (DungeonZone.Registry.TryGetValue(newLocation, out DungeonZone foundZone))
            {
                // 3. If both are found, start the dungeon
                StartDungeon(foundDungeon, foundZone); 
            }
        }
        else if (_isDungeonActive) // If the new location is not a dungeon, but the player just left from a dungeon
        {
            StopAndResetDungeon();
        }
    }

    // Called when entering a dungeon
    private void StartDungeon(DungeonData dungeon, DungeonZone zone)
    {
        Debug.Log($"Entering {dungeon.dungeonLocation}. Starting Round 1.");
        
        _currentDungeon = dungeon;
        _currentZone = zone; 
        _currentRound = 1;
        _isDungeonActive = true;
        
        StartCoroutine(BeginRoundRoutine());
        OnDungeonStarted?.Invoke();
    }
    
    // Called when leaving a dungeon or died
    private void StopAndResetDungeon()
    {
        Debug.Log($"Exiting {_currentDungeon?.dungeonLocation}.");
        
        StopAllCoroutines();
        
        _isDungeonActive = false;
        _currentRound = 0;
        _currentDungeon = null;
        _currentZone = null; // Clear the zone
        
        _dungeonSpawner.StopSpawning();
        _dungeonEnemyTracker.ClearDungeon();
        OnDungeonEnded?.Invoke();
    }

    // Called when all enemies in the current round have been cleared
    private void StartNextRound()
    {
        if (!_isDungeonActive) return;
        
        _currentRound++;
        StartCoroutine(BeginRoundRoutine());
        OnRoundStarted?.Invoke(_currentRound);
    }

    private IEnumerator BeginRoundRoutine()
    {
        // Wait for the delay between rounds
        yield return new WaitForSeconds(_currentDungeon.delayBetweenRounds);

        int totalToSpawn = _currentDungeon.baseEnemyCount +
                           (_currentRound - 1) * _currentDungeon.enemiesPerRoundScaling;
        
        // Tell the spawner to start spawning enemies
        _dungeonSpawner.StartSpawning(_currentDungeon, _currentZone, _currentRound, totalToSpawn);
        OnRoundStarted?.Invoke(_currentRound);
    }
}