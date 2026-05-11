using System;
using UnityEngine;

public class NPCScheduleController : MonoBehaviour
{
    private NPCController _npcController;
    private int _lastCheckedHour = -1;
    public State CurrentScheduledState { get; private set; }
    
    private void Awake() => _npcController = GetComponent<NPCController>();

    private void OnEnable() => EventBus.OnWorldTimeChanged += HandleTimeChanged;
    
    private void OnDisable() => EventBus.OnWorldTimeChanged -= HandleTimeChanged;
    
    private void HandleTimeChanged(object sender, TimeSpan time)
    {
        // Check if the hour has changed
        if (time.Hours != _lastCheckedHour)
        {
            _lastCheckedHour = time.Hours;
            EvaluateSchedule(time.Hours);
        }
    }

    private void EvaluateSchedule(int currentHour)
    {
        State nextState;

        if (currentHour >= 23 || currentHour < 6)
        {
            nextState = _npcController.SleepState;  // 11 PM - 5:59 AM
        }
        else if (currentHour >= 8 && currentHour < 17)
        {
            nextState = _npcController.WorkState;   // 8 AM - 4:59 PM
        }
        else if (currentHour >= 17 && currentHour < 21)
        {
            nextState = _npcController.HobbyState;  // 5 PM - 8:59 PM
        }
        else 
        {
            // This covers 6 AM - 8 AM AND 9 PM - 11 PM
            nextState = _npcController.HomeState;
        }

        CurrentScheduledState = nextState;
    
        // State Change Logic
        if (_npcController.StateMachine.CurrentState != nextState)
        {
            _npcController.StateMachine.ChangeState(nextState);
        }
    }
}
