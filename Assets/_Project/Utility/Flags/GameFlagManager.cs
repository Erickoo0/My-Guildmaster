using System;
using UnityEngine;
using System.Collections.Generic;

public class GameFlagManager : MonoBehaviour
{
    public static GameFlagManager Instance { get; private set; }
    
    public Dictionary<FlagKeys.GameFlag, bool> GameFlags = new Dictionary<FlagKeys.GameFlag, bool>();
    public Dictionary<FlagKeys.GameStat, int> GameStats = new Dictionary<FlagKeys.GameStat, int>();
    
    public Dictionary<FlagKeys.GameFlag, bool> GetAllGameFlags() => GameFlags;

    public Dictionary<FlagKeys.GameStat, int> GetAllGameStats() => GameStats;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Multiple GameFlagManagers detected. Disabling script.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetGameFlag(FlagKeys.GameFlag flag, bool state)
    {
        // 1. Find the flag in the dictionary and find its current state
        bool flagExists = GameFlags.TryGetValue(flag, out bool flagState);

        // 2. If the flag doesn't exist or its state is different, update it'
        if (!flagExists || flagState != state)
        {
            GameFlags[flag] = state;
            
            // 3. Let other systems know about the flag change
            EventBus.RequestGameFlagChanged(flag, state);
        }
    }

    public bool GetGameFlag(FlagKeys.GameFlag flag)
    {
        return GameFlags.TryGetValue(flag, out bool flagState) && flagState;
    }

    public void SetGameStat(FlagKeys.GameStat gameStat, int value)
    {
        // 1. Find the stat in the dictionary and find its current value
        bool statExists = GameStats.TryGetValue(gameStat, out int statValue);
       
        // 2. If the stat doesn't exist or its value is different, update it'
        if (!statExists || statValue != value)
        {
            GameStats[gameStat] = value;
            
            // 3. Let other systems know about the flag change
            EventBus.RequestGameStatChanged(gameStat, value);
        }
    }

    public void IncrementGameStat(FlagKeys.GameStat gameStat, int value)
    {
        // 1. Find the stat in the dictionary and find its current value
        int currentValue = GetGameStat(gameStat);
        
        // 2. Increment the stat
        SetGameStat(gameStat, currentValue + value);
    }

    public int GetGameStat(FlagKeys.GameStat gameStat)
    {
        return GameStats.TryGetValue(gameStat, out int statValue) ? statValue : 0;
    }
}
