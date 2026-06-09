using System;
using UnityEngine;

public static class EventBus
{
    //-----------------------Time Events--------------------------
    public static event EventHandler<TimeSpan> OnWorldTimeChanged;
    public static void RequestUpdateWorldTime(object sender,TimeSpan time) => OnWorldTimeChanged?.Invoke(sender, time);
    
    public static EventHandler<TimeSpan> OnDayChanged;
    public static void RequestDayChanged(object sender, TimeSpan time) => OnDayChanged?.Invoke(sender, time);
    
    //-----------------------Dialogue Events--------------------------
    //Signals when a dialogue option is selected that has an action
    public static Action<string, object> OnDialogueEventRequested;
    public static void RequestDialogueEvent(string dialogueEvent, object data)
    {
        OnDialogueEventRequested?.Invoke(dialogueEvent, data);
    }
    
    //-----------------------Stat/Flag Events--------------------------
    public static event Action<FlagKeys.GameFlag, bool> OnGameFlagChanged;
    public static void RequestGameFlagChanged(FlagKeys.GameFlag flag, bool state) => OnGameFlagChanged?.Invoke(flag, state);
    
    public static event Action<FlagKeys.GameStat, int> OnGameStatChanged;
    public static void RequestGameStatChanged(FlagKeys.GameStat stat, int value) => OnGameStatChanged?.Invoke(stat, value);
    
    //-----------------------UI Events--------------------------
    public static event Action<GameObject> OnMenuOpenRequested;
    public static void RequestOpenMenu(GameObject menu) => OnMenuOpenRequested?.Invoke(menu);

    public static event Action<GameObject> OnMenuCloseRequested;
    public static void RequestCloseMenu(GameObject menu = null) => OnMenuCloseRequested?.Invoke(menu);

    public static event Action<GameObject> OnMenuClosed;
    public static void NotifyMenuClosed(GameObject menu) => OnMenuClosed?.Invoke(menu);
    
    public static event Action<bool> OnPlayerMovementToggleRequested;
    public static void RequestPlayerMovementToggle(bool canMove) => OnPlayerMovementToggleRequested?.Invoke(canMove);
    
    //--------------------------Quest Events-------------------------
    public static event Action<string, int> OnUpdateQuestObjectiveRequested;
    public static void RequestUpdateQuestObjective(string targetID, int number) => OnUpdateQuestObjectiveRequested?.Invoke(targetID, number);
    
    public static event Action OnUpdateQuestRequested;
    public static void RequestUpdateQuest()=> OnUpdateQuestRequested?.Invoke();

    public static event Action<string> OnQuestProgressCompleted;
    public static void RequestQuestProgressCompleted(string questID) => OnQuestProgressCompleted?.Invoke(questID);
    
    //--------------------------Combat Events-------------------------
    // Signals when a floating text gets requested
    public static event Action<int, Vector3> OnFloatingTextRequested;
    // Any script can call this method to request a floating number
    public static void RequestFloatingText(int amount, Vector3 position)
    {
        OnFloatingTextRequested?.Invoke(amount, position);
    }

    public static event Action<GameObject> OnEntityDeathRequested;

    public static void RequestEntityDeathUpdate(GameObject entity)
    {
        OnEntityDeathRequested?.Invoke(entity);
    }
}