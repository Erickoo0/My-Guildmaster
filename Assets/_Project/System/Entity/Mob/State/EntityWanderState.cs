using Pathfinding;
using UnityEngine;

[System.Serializable]
public class EntityWanderState : BaseWanderState
{
    private Vector2 _targetDestination;

    [Header("Anti-Stuck Variables")] 
    private float _stuckTimer;
    private readonly float _stuckTimerMax = 5f;
    private Vector2 _lastPosition;
    private readonly float _stuckThreshold = 0.1f;
    private float _positionCheckTimer;
    private readonly float _positionCheckInterval = 0.5f;
    
    public override void Enter()
    {
        // 1. Tell the AI to start calculating paths again
        if (controller.aiLerp != null)
        {
            controller.aiLerp.canSearch = true;
            controller.aiLerp.canMove = true;
        }
        
        // 2. Reset state flags
        _stuckTimer = 0f;
        _positionCheckTimer = _positionCheckInterval;
        _lastPosition = controller.transform.position;
        
        SetNewDestination();
    }

    private void SetNewDestination()
    {
        if (controller.aiLerp == null) return;

        _targetDestination = (Vector2)controller.SpawnPosition + (Random.insideUnitCircle * controller.WanderRadius);
        controller.aiLerp.destination = _targetDestination;
        controller.aiLerp.SearchPath();
    }

    public override void Update()
    {
        // 1. If theres a target
        if (controller.currentTarget != null && controller.ChaseState != null)
        {
            stateMachine.ChangeState(controller.ChaseState);
            return;
        }
        
        // 2. If being knocked back
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack) 
            return;
        
        // 3. If arrived at destination
        if (controller.aiLerp != null && !controller.aiLerp.pathPending && controller.aiLerp.reachedEndOfPath)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        // 4. Feed the animator using AIPath's native velocity
        if (controller.EntityAnimator != null && controller.aiLerp != null)
            controller.EntityAnimator.SetMoveAnimation(controller.aiLerp.velocity);
        
        
        // 5. Apply anti-stuck safety
        CheckForStuck();
    }

    public override void Exit()
    {
        // 1. Shut down pathfinding search
        if (controller.aiLerp != null)
        {
            controller.aiLerp.canSearch = false;
            controller.aiLerp.canMove = false;
            controller.aiLerp.destination = controller.transform.position; 
        }
        
        // 2. Set movement to zero
        controller._rigidBody2D.linearVelocity = Vector2.zero;
        controller.EntityMover?.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator?.SetMoveAnimation(Vector2.zero);
    }

    private void CheckForStuck()
    {
        _positionCheckTimer -= Time.deltaTime;
        
        if (_positionCheckTimer <= 0f)
        {
            // Check if the entity moved less than the threshold over the last 0.5 seconds
            if (Vector2.Distance(_lastPosition, controller.transform.position) < _stuckThreshold)
            {
                _stuckTimer += _positionCheckInterval;
            }
            else
            {
                _stuckTimer = 0;
            }
            
            if (_stuckTimer > _stuckTimerMax)
            {
                Debug.Log("Entity stuck! Resetting path.");
                _stuckTimer = 0;
                SetNewDestination();
            }
            
            _lastPosition = controller.transform.position;
            _positionCheckTimer = _positionCheckInterval;
        }
    }
}