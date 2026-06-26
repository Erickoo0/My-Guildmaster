using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

public enum MobType { Passive, Neutral, Aggressive }

public class MobController : BaseEntityController
{
    
    [Header("References")]
    [HideInInspector] public Rigidbody2D _rigidBody2D;
    [HideInInspector] public SpellControllerEntity _spellController;
    
    [Header("Movement Settings")]
    [HideInInspector] public AILerp aiLerp;
    [field: SerializeField] public float WanderRadius { get; private set; } = 5f;
    public Vector2 SpawnPosition { get; private set; }
    
    [Header("Mob Type")] 
    [field: SerializeField] public MobType mobType { get; private set; }  = MobType.Aggressive;
    
    [Header("Targeting Settings")]
    [field: SerializeField] public List<string> TargetableList { get; private set; }
    [field: SerializeField] public float TargetRange { get; private set; } = 6f;
    [field: SerializeField] public float TargetLostRange { get; set; } = 10f;
    [HideInInspector] public Transform currentTarget ;
    private readonly Collider2D[] _targetingResults = new Collider2D[10]; // Pre-allocated array for targeting results
    private ContactFilter2D _targetingFilter;
    
    [Header("Alert Settings")]
    [SerializeField] private GameObject _alertIcon;
    private float alertedTime = 1f;
    private float alertedTimer;
    
    [Header("States")]
    [SerializeReference, SubclassSelector] public BaseSpawnState SpawnState;
    [SerializeReference, SubclassSelector] public BaseIdleState IdleState;
    [SerializeReference, SubclassSelector] public BaseWanderState WanderState;
    [SerializeReference, SubclassSelector] public BaseChaseState ChaseState;
    [SerializeReference, SubclassSelector] public BaseKiteState KiteState;
    
    protected override void Awake()
    {
        base.Awake();
        
        
        _targetingFilter = ContactFilter2D.noFilter;
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _spellController = GetComponent<SpellControllerEntity>();
        
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
        KiteState?.Setup(this, StateMachine);
        
        SpawnPosition = transform.position;
        StateMachine.SetupState(SpawnState);
    }

    protected override void Update()
    {
        base.Update();
        
        // Safety Check
        if (ChaseState == null) return;
        
        // Begin target scan only after spawning
        if (StateMachine.CurrentState != SpawnState)
            UpdateTargeting();
        
        // 2. Alert Countdown
        if (alertedTimer > 0)
            alertedTimer -= Time.deltaTime;
        else if (_alertIcon != null)
            _alertIcon.SetActive(false);
        
    }

    //---- Targeting Methods ----
    private void UpdateTargeting()
    {
        if (currentTarget == null)
            FindTarget();
        else if (currentTarget != null && !IsTargetInRange(TargetLostRange))
            currentTarget = null;
        
    }

    private void FindTarget()
    {
        // Safety Check
        if (TargetableList == null) return;
        
        int hitCount = Physics2D.OverlapCircle(transform.position, TargetRange, _targetingFilter, _targetingResults);
        
        // Check all collided instances if they are targetable
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _targetingResults[i];
            
            if (!hit.TryGetComponent(out ITargetable targetInterface)) continue;
            if (!TargetableList.Contains(targetInterface.GetTargetID())) continue;
            
            currentTarget = hit.transform;
            
            // Set the alert icon
            alertedTimer = alertedTime;
            _alertIcon.SetActive(true);
            
            return; // Lock onto the first valid target and exit
        }
    }
    
    public bool IsTargetInRange(float range) => 
        currentTarget != null && Vector2.Distance(transform.position, currentTarget.transform.position) <= range;
    
    //----Debug Methods-----
    private void OnDrawGizmosSelected()
    {
        // 1. Target Range 
        Gizmos.color = Color.yellow;
        DrawGizmoCircle(transform.position, TargetRange);

        // 2. Target Lost Range
        Gizmos.color = Color.red;
        DrawGizmoCircle(transform.position, TargetLostRange);
        
    
        // 4. Wander Radius
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