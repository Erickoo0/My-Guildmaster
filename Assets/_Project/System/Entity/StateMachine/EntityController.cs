using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Unity.VisualScripting;

public enum MobType { Passive, Neutral, Aggressive }

public class EntityController : BaseEntityController
{
    
    [Header("Movement Settings")]
    [HideInInspector] public AIPath aiPath;
    [field: SerializeField] public float WanderRadius { get; private set; } = 5f;
    public Vector2 SpawnPosition { get; private set; }
    
    [Header("Attack Library")]
    [SerializeField] private List<AttackData> attackLibrary;
    
    [Header("Mob Type & Targeting")] 
    [field: SerializeField] public MobType mobType { get; private set; }  = MobType.Aggressive;
    [field: SerializeField] public float DetectionRange { get; private set; } = 6f;
    [field: SerializeField] public float DetectionLostRange { get; private set; } = 10f;
    [field: SerializeField] public float ActionRange { get; set; } = 5f;
    [field: SerializeField] public List<string> TargetableList { get; private set; }
    public Transform currentTarget ;
    
    [Header("Action Settings")] 
    [field: SerializeField] public float ActionCooldown  { get; private set; } = 1f;
    private float _lastActionTime;
    
    [Header("State References")]
    [SerializeReference, SubclassSelector] public BaseIdleState IdleState;
    [SerializeReference, SubclassSelector] public BaseWanderState WanderState;
    [SerializeReference, SubclassSelector] public BaseChaseState ChaseState;
    [SerializeReference, SubclassSelector] public BaseActionState AttackState;
    
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
        WanderState?.Setup(this, StateMachine);
        ChaseState?.Setup(this, StateMachine);
        AttackState?.Setup(this, StateMachine);
        
        SpawnPosition = transform.position;
    }

    protected virtual void Start()
    {
        // Start by wandering to first waypoint
        StateMachine.SetupState(WanderState);
    }

    protected override void Update()
    {
        base.Update();
        UpdateTargeting();
        //Debug.Log($"Current State: {StateMachine.CurrentState}");
    }

    //---- Targeting Methods ----
    private void UpdateTargeting()
    {
        // If we have a target, check if they ran away too far
        if (currentTarget != null)
        {
            if (!IsTargetInRange(DetectionLostRange))
            {
                ClearTarget();
            }
            return; 
        }

        // If we DON'T have a target, scan the area
        FindTarget();
    }

    private void FindTarget()
    {
        if (ChaseState == null || TargetableList == null) return;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, DetectionRange);

        // Check all collided instances if they are targetable
        foreach (Collider2D hit in hits)
        {
            ITargetable targetInterface = hit.GetComponentInParent<ITargetable>();

            if (targetInterface == null) continue;
            if (!TargetableList.Contains((targetInterface.GetTargetID()))) continue;
            
            // Set the target
            currentTarget = hit.transform;
            StateMachine.ChangeState(ChaseState);
            return;
        }
    }
    
    private void ClearTarget()
    {
        currentTarget = null;
        StateMachine.ChangeState(IdleState);
    }
    
    private bool IsTargetInRange(float range)
    {
        if (currentTarget == null) return false;
        else return Vector2.Distance(transform.position, currentTarget.transform.position) <= range;
    }
    
    //---- Action Methods -----
    public bool CheckActionCooldown() 
    {
        return Time.time >= _lastActionTime + ActionCooldown;
    }
    
    // A method to reset the timer (called when the action finishes)
    public void SetActionCooldown()
    {
        _lastActionTime = Time.time;
    }
    
    public T GetAttackData<T>(string id) where T : AttackData
    {
        // Search the library for a piece of data that:
        // 1. Matches the ID string
        // 2. Is of the type (T) we are looking for
        return attackLibrary.OfType<T>().FirstOrDefault(data => data.attackID == id);
    }
    
    
    //----Debug Methods-----
    private void OnDrawGizmosSelected()
    {
        // 1. Detection Range (Aggro Zone)
        Gizmos.color = Color.yellow;
        DrawGizmoCircle(transform.position, DetectionRange);

        // 2. Detection Lost Range (Leash Zone)
        Gizmos.color = Color.red;
        DrawGizmoCircle(transform.position, DetectionLostRange);

        // 3. Action Range (Attack Zone)
        Gizmos.color = Color.cyan;
        DrawGizmoCircle(transform.position, ActionRange);
    
        // 4. Wander Radius (Spawn Zone)
        // We check if SpawnPosition is zero to avoid drawing at world center before Start()
        if (SpawnPosition != Vector2.zero)
        {
            Gizmos.color = Color.green;
            DrawGizmoCircle(SpawnPosition, WanderRadius);
        }
    }

    private void DrawGizmoCircle(Vector2 center, float radius)
    {
        float angle = 0f;
        Vector2 lastPoint = center + new Vector2(Mathf.Cos(0) * radius, Mathf.Sin(0) * radius);

        for (int i = 1; i <= 32; i++)
        {
            angle += (2f * Mathf.PI) / 32f;
            Vector2 nextPoint = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}