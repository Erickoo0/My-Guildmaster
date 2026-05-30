using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

[System.Serializable]
public class NPCScheduleData
{
    [HierarchySelector(typeof(PointOfInterest))]
    public List<string> HomePOIList;
    [HierarchySelector(typeof(PointOfInterest))]
    public List<string> SleepPOIList;
    [HierarchySelector(typeof(PointOfInterest))]
    public List<string> HobbyPOIList;
    [HierarchySelector(typeof(PointOfInterest))]
    public List<string> WorkPOIList;
}

public class NPCController : BaseEntityController
{
    [Header("Movement Settings")]
    [HideInInspector] public AIPath aiPath;
    
    [Header("References")]
    [SerializeReference, SubclassSelector] public BaseNPCIdleState IdleState;
    [SerializeReference, SubclassSelector] public BaseNPCHomeState HomeState;
    [SerializeReference, SubclassSelector] public BaseNPCSleepState SleepState;
    [SerializeReference, SubclassSelector] public BaseNPCHobbyState HobbyState;
    [SerializeReference, SubclassSelector] public BaseNPCWorkState WorkState;
    [SerializeReference, SubclassSelector] public List<BaseNPCOverrideWanderState> dormantOverrideStates = new List<BaseNPCOverrideWanderState>();

    [Header("NPC Schedule")]
    public GameLocation currentLocation;
    [field: SerializeField] public NPCScheduleData NpcScheduleData {get; private set;}
    public State OverrideState { get; private set; }
    public bool IsOverrideState => OverrideState != null;

    protected override void Awake()
    {
        base.Awake();
        
        aiPath = GetComponent<AIPath>();
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.updateRotation = false;
        }
        
        IdleState?.Setup(this, StateMachine);
        HomeState?.Setup(this, StateMachine);
        SleepState?.Setup(this, StateMachine);
        HobbyState?.Setup(this, StateMachine);
        WorkState?.Setup(this, StateMachine);
        foreach (BaseNPCOverrideWanderState overrideState in dormantOverrideStates)
        {
            overrideState?.Setup(this, StateMachine);
        }
    }
    
    protected virtual void Start()
    {
        // Start by entering the spawn state 
        StateMachine.SetupState(IdleState);
    }
    
    protected override void Update()
    {
        base.Update();
        // Only evaluate if we aren't already IN an override state.
        if (!IsOverrideState)
        {
            EvaluateOverrideStates();
        }
    }

    public void EvaluateOverrideStates()
    {
        IStateOverrider highestPriorityOverride = null;

        // Loops through the list of all override states for any valid ones
        // Selects the highest priority one
        foreach (BaseNPCOverrideWanderState overrideState in dormantOverrideStates)
        {
            if (overrideState.EvaluateRequirements())
            {
                if (highestPriorityOverride == null || overrideState.Priority > highestPriorityOverride.Priority)
                {
                    highestPriorityOverride = overrideState;
                }
            }
        }
        
        if (highestPriorityOverride != null)
            SetOverrideState(highestPriorityOverride);
    }

    public void SetOverrideState(IStateOverrider newState)
    {
        OverrideState = newState as State;
        StateMachine.ChangeState(OverrideState);
        Debug.Log("NPC is now in override state: " + newState.GetType().Name);
    }

    public void ClearOverrideState()
    {
        Debug.Log("NPC is no longer in override state");
        OverrideState = null;
        
        // Fallback safely to whatever the schedule says they should be doing right now
        var scheduleController = GetComponent<NPCScheduleController>();
        if (scheduleController != null)
        {
            StateMachine.ChangeState(scheduleController.CurrentScheduledState);
        }
        else
        {
            StateMachine.ChangeState(IdleState);
        }
    }
}
