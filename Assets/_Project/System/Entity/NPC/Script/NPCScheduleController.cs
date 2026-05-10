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

        // 10 PM  to 6 AM
        if (currentHour >= 22 || currentHour < 6)
        {
            nextState = _npcController.SleepState;
        }
        // 6 AM to 8 AM 
        else if (currentHour >= 6 && currentHour < 8)
        {
            nextState = _npcController.HomeState;
        }
        // 8 AM to 5 PM 
        else if (currentHour >= 8 && currentHour < 17)
        {
            nextState = _npcController.WorkState;
        }
        // 5 PM to 10 PM
        else 
        {
            nextState = _npcController.HobbyState;
        }

        // Save the current state so Idle knows what to do next
        CurrentScheduledState = nextState;
        
        // Check if already in that state
        if (_npcController.StateMachine.CurrentState != nextState)
            _npcController.StateMachine.ChangeState(nextState);
        
    }
}
