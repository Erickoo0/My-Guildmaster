using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Unity.VisualScripting;

[System.Serializable]
public class NPCScheduleData
{
    public List<string> HomePOIList;
    public List<string> SleepPOIList;
    public List<string> HobbyPOIList;
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
    
    [Header("NPC Schedule")]
    [field: SerializeField] public NPCScheduleData NpcScheduleData {get; private set;}
    

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
    }
    
    protected virtual void Start()
    {
        // Start by entering the spawn state 
        StateMachine.SetupState(IdleState);
    }
}
