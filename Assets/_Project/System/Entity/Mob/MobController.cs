using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

public enum MobType { Passive, Neutral, Aggressive }

public class MobController : BaseEntityController
{
    
    [Header("Movement Settings")]
    [HideInInspector] public AILerp aiLerp;
    [field: SerializeField] public float WanderRadius { get; private set; } = 5f;
    public Vector2 SpawnPosition { get; private set; }

    [Header("Spell Settings")]
    public SpellDataDatabase globalSpellDatabase;
    
    [Header("Mob Type")] 
    [field: SerializeField] public MobType mobType { get; private set; }  = MobType.Aggressive;
    
    [Header("Targeting Settings")]
    [field: SerializeField] public List<string> TargetableList { get; private set; }
    [field: SerializeField] public float TargetRange { get; private set; } = 6f;
    [field: SerializeField] public float TargetLostRange { get; set; } = 10f;
    [field: SerializeField] public float ActionRange { get; set; } = 5f;
    [HideInInspector] public Transform currentTarget ;
    private readonly Collider2D[] _targetingResults = new Collider2D[10]; // Pre-allocated array for targeting results
    private ContactFilter2D _targetingFilter;
    
    [Header("Action Settings")] 
    [field: SerializeField] public float ActionCooldown  { get; private set; } = 1f;
    private float _lastActionTime;
    
    [Header("References")]
    [SerializeReference, SubclassSelector] public BaseSpawnState SpawnState;
    [SerializeReference, SubclassSelector] public BaseIdleState IdleState;
    [SerializeReference, SubclassSelector] public BaseWanderState WanderState;
    [SerializeReference, SubclassSelector] public BaseChaseState ChaseState;
    [SerializeReference, SubclassSelector] public BaseActionState AttackState;
    public Rigidbody2D _rigidBody2D;
    
    protected override void Awake()
    {
        base.Awake();
        
        
        _targetingFilter = new ContactFilter2D();
        _targetingFilter.NoFilter();
        _rigidBody2D = GetComponent<Rigidbody2D>();
        
        // Disable aiLerp movement by default (Controlled via states)
        aiLerp = GetComponent<AILerp>();
        if (aiLerp != null)
        {
            aiLerp.canMove = false;
            aiLerp.updateRotation = false;
        }

        // Setup all states
        SpawnState?.Setup(this, StateMachine);
        IdleState?.Setup(this, StateMachine);
        WanderState?.Setup(this, StateMachine);
        ChaseState?.Setup(this, StateMachine);
        AttackState?.Setup(this, StateMachine);
        
        SpawnPosition = transform.position;
        StateMachine.SetupState(SpawnState);
    }

    protected override void Update()
    {
        base.Update();
        
        // Safety Check
        if (ChaseState == null || AttackState == null) return;
        
        // Begin target scan only after spawning
        if (StateMachine.CurrentState != SpawnState)
            UpdateTargeting();
    }

    //---- Targeting Methods ----
    private void UpdateTargeting()
    {
        // If we have a target, check if they ran away too far
        if (currentTarget != null)
        {
            // Drop target if they are out of range
            if (!IsTargetInRange(TargetLostRange))
                ClearTarget();
            // Transition to chase if we are not already chasing
            else if (StateMachine.CurrentState != AttackState && StateMachine.CurrentState != ChaseState)
                StateMachine.ChangeState(ChaseState);
            
            return;
        }

        // If we DON'T have a target, scan the area
        FindTarget();
    }

    private void FindTarget()
    {
        // Safety Check
        if (ChaseState == null || TargetableList == null) return;
        
        int hitCount = Physics2D.OverlapCircle(transform.position, TargetRange, _targetingFilter, _targetingResults);
        
        // Check all collided instances if they are targetable
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _targetingResults[i];
            
            // If the target is not ITargetable, skip it
            if (!hit.TryGetComponent(out ITargetable targetInterface)) continue;
            // If the target is not in the targetable list, skip it
            if (!TargetableList.Contains(targetInterface.GetTargetID())) continue;
            
            currentTarget = hit.transform;
            StateMachine.ChangeState(ChaseState);
            return; // Lock onto the first valid target and exit
        }
    }
    
    private void ClearTarget()
    {
        currentTarget = null;
        StateMachine.ChangeState(IdleState);
    }
    
    public bool IsTargetInRange(float range)
    {
        if (currentTarget == null) return false;
        else return Vector2.Distance(transform.position, currentTarget.transform.position) <= range;
    }
    
    //---- Action Methods -----
    public bool CheckActionCooldown() => Time.time >= _lastActionTime + ActionCooldown;
    
    public void SetActionCooldown() => _lastActionTime = Time.time;
    
    //----Debug Methods-----
    private void OnDrawGizmosSelected()
    {
        // 1. Detection Range (Aggro Zone)
        Gizmos.color = Color.yellow;
        DrawGizmoCircle(transform.position, TargetRange);

        // 2. Detection Lost Range (Leash Zone)
        Gizmos.color = Color.red;
        DrawGizmoCircle(transform.position, TargetLostRange);

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