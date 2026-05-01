using UnityEngine;
using UnityEngine.AI;

public class EntityWanderState : BaseWanderState
{
    [Header("Wander Settings")]
    private NavMeshPath _navMeshPath;
    private int _pathIndex;
    private Vector2 _targetDestination;
    
    [Header("Anti Stuck Variables")]
    private float _stuckTimer;
    private Vector2 _lastPosition;
    private const float StuckThreshold = 0.05f;
    private const float MaxStuckTime = 1.5f;

    // Cache the squared thresholds for performance
    private readonly float _stuckThresholdSqr = StuckThreshold * StuckThreshold;
    private const float WaypointThresholdSqr = 1.0f * 1.0f; 

    public override void Enter() 
    {
        _navMeshPath = new NavMeshPath();
        _stuckTimer = 0f;
        SetNewDestination();
    }

    private void SetNewDestination()
    {
        _lastPosition = controller.transform.position;
        
        // 1. Pick a random point within a circle around Spawn
        Vector2 randomPoint = controller.SpawnPosition + (Random.insideUnitCircle * controller.WanderRadius);

        // 2. Snap to the Navmesh to find a valid destination
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            _targetDestination = hit.position;
            
            // 3. CRITICAL FIX: Actually calculate the path!
            NavMesh.CalculatePath(controller.transform.position, _targetDestination, NavMesh.AllAreas, _navMeshPath);
            
            // 4. Validate the path we just generated
            if (_navMeshPath.status != NavMeshPathStatus.PathComplete || _navMeshPath.corners.Length < 2)
            {
                stateMachine.ChangeState(controller.IdleState);
                return;
            }
            
            _pathIndex = 1; // Start at index 1 (0 is our current position)
        }
        else
        {
            stateMachine.ChangeState(controller.IdleState);
        }
    }

    public override void Update()
    {
        if (_navMeshPath == null || _pathIndex >= _navMeshPath.corners.Length)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        Vector2 currentPos = controller.transform.position;

        // 1. Optimized Stuck Check (using sqrMagnitude)
        if ((currentPos - _lastPosition).sqrMagnitude < _stuckThresholdSqr)
        {
            _stuckTimer += Time.deltaTime;
        }
        else
        {
            _stuckTimer = 0f;
            _lastPosition = currentPos;
        }
        
        if (_stuckTimer >= MaxStuckTime)
        {
            Debug.Log("Stuck for too long, going back to idle");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        // 2. Movement Logic
        Vector2 targetCorner = _navMeshPath.corners[_pathIndex];
        Vector2 direction = (targetCorner - currentPos).normalized;
        controller.EntityMover.SetMoveDirection(direction);

        // 3. Progress index 
        if ((currentPos - targetCorner).sqrMagnitude < WaypointThresholdSqr)
        {
            _pathIndex++;
        }
        
        ShowDebugLine();
    }

    public override void Exit()
    {
        controller.EntityMover.SetMoveDirection(Vector2.zero);
    }

    private void ShowDebugLine()
    {
#if UNITY_EDITOR
        // Wrapped in UNITY_EDITOR so it compiles out of final build automatically
        if (_navMeshPath == null) return;
        for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(_navMeshPath.corners[i] + Vector3.back, _navMeshPath.corners[i + 1] + Vector3.back, Color.cyan);
        }
#endif
    }
}