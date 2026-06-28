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

public class NPCController : EntityControllerBase
{
    [Header("Movement Settings")]
    [HideInInspector] public AILerp aiLerp;
    
    [Header("References")]
    [SerializeReference, SubclassSelector] public BaseNPCIdleState IdleState;
    [SerializeReference, SubclassSelector] public BaseNPCHomeState HomeState;
    [SerializeReference, SubclassSelector] public BaseNPCSleepState SleepState;
    [SerializeReference, SubclassSelector] public BaseNPCHobbyState HobbyState;
    [SerializeReference, SubclassSelector] public BaseNPCWorkState WorkState;
    [SerializeReference, SubclassSelector] public List<BaseNPCOverrideWanderState> dormantOverrideStates = new List<BaseNPCOverrideWanderState>();
    private NPCScheduleController _scheduleController;
    [HideInInspector] public Rigidbody2D _rigidbody2D;

    [Header("NPC Schedule")]
    public GameLocation currentLocation;
    [field: SerializeField] public NPCScheduleData NpcScheduleData {get; private set;}
    public State OverrideState { get; private set; }
    public bool IsOverrideState => OverrideState != null;

    [Header("NPC Interaction")]
    public bool IsInteractable { get; set; } = true;
    
    [Header("Evaluation timers")]
    public float evaluationTimer = 0f;
    private float _evaluationInterval = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        
        _scheduleController = GetComponent<NPCScheduleController>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        
        aiLerp = GetComponent<AILerp>();
        if (aiLerp != null)
        {
            aiLerp.canMove = false;
            aiLerp.updateRotation = false;
        }
        
        // Setup all base states
        IdleState?.Setup(this, StateMachine);
        HomeState?.Setup(this, StateMachine);
        SleepState?.Setup(this, StateMachine);
        HobbyState?.Setup(this, StateMachine);
        WorkState?.Setup(this, StateMachine);
        
        // Setup all override states
        foreach (BaseNPCOverrideWanderState overrideState in dormantOverrideStates)
            overrideState?.Setup(this, StateMachine);
    }
    
    protected virtual void Start()
    {
        // Start by entering the spawn state 
        StateMachine.SetupState(IdleState);
    }
    
    protected override void Update()
    {
        base.Update();
        
        // If we are currently overriding the schedule, do not evaluate schedule shifts
        if (IsOverrideState) return;
        
        evaluationTimer += Time.deltaTime;
        if (evaluationTimer >= _evaluationInterval)
        {
            EvaluateOverrideStates();
            evaluationTimer = 0f;
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
        
        // If an override state was found, trigger it immediately
        if (highestPriorityOverride != null)
            SetOverrideState(highestPriorityOverride);
    }

    private void SetOverrideState(IStateOverrider newState)
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
        if (_scheduleController != null)
            StateMachine.ChangeState(_scheduleController.CurrentScheduledState);
        else
            StateMachine.ChangeState(IdleState);
    }
}
