using UnityEngine;
using System.Collections.Generic;

public class DungeonEnemyTracker : MonoBehaviour
{
    public event System.Action OnAllEnemiesCleared;
    public event System.Action<int> OnEnemyCountChanged;
    
    // A list of all active enemies in the dungeon
    private readonly HashSet<GameObject> _currentEnemies = new HashSet<GameObject>();

    private void OnEnable() => EventBus.OnEntityDeathRequested += HandleEntityDeath;
    private void OnDisable() => EventBus.OnEntityDeathRequested -= HandleEntityDeath;

    // Called by the DungeonSpawner when it spawns a new enemy
    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        _currentEnemies.Add(enemy);
        OnEnemyCountChanged?.Invoke(_currentEnemies.Count);
    }

    private void HandleEntityDeath(GameObject entity)
    {
        // If the entity is in the list, remove it
        if (_currentEnemies.Contains(entity))
        {
            _currentEnemies.Remove(entity);
            
            OnEnemyCountChanged?.Invoke(_currentEnemies.Count);
            
            Debug.Log($"Remaining: {_currentEnemies.Count}");
            
            // If there are no enemies left, invoke the event
            if (_currentEnemies.Count == 0)
            {
                OnAllEnemiesCleared?.Invoke();
            }
        }
    }

    // Called by the DungeonController when the current round is cleared
    public void ClearDungeon()
    {
        // Loop through the HashSet and destroy each enemy
        foreach (GameObject enemy in _currentEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        
        _currentEnemies.Clear();
        OnEnemyCountChanged?.Invoke(0);
    }
}
