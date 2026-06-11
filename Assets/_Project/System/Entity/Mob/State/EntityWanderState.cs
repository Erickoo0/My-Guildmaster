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
        controller.aiPath.canSearch = true;
        
        _stuckTimer = 0f;
        _positionCheckTimer = _positionCheckInterval;
        _lastPosition = controller.transform.position;
        
        SetNewDestination();
    }

    private void SetNewDestination()
    {
        _targetDestination = (Vector2)controller.SpawnPosition + (Random.insideUnitCircle * controller.WanderRadius);
        controller.aiPath.destination = _targetDestination;
        controller.aiPath.SearchPath();
    }

    public override void Update()
    {
        // 2. Check for knocked back
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack) return;
        

        // 3. Check for arrival
        if (!controller.aiPath.pathPending && controller.aiPath.reachedEndOfPath)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        // 3. Tell AIPath to calculate the next step
        controller.aiPath.MovementUpdate(Time.deltaTime, out Vector3 nextPos, out Quaternion nextRot);

        // 4. CALCULATE THE DIRECTION
        // We move toward 'nextPos' rather than using 'desiredVelocity'
        Vector2 moveDirection = ((Vector2)nextPos - (Vector2)controller.transform.position).normalized;
        
        // 5. Handle Overshooting: Check distance to the destination
        float distanceToTarget = Vector2.Distance(controller.transform.position, controller.aiPath.destination);
        if (distanceToTarget < controller.aiPath.endReachedDistance)
        {
            controller.EntityMover.SetMoveDirection(Vector2.zero);
        }
        else
        {
            controller.EntityMover.SetMoveDirection(moveDirection);
        }
        
        // 6. Animation Logic
        if (controller.EntityAnimator != null)
            controller.EntityAnimator.SetMoveAnimation(controller.EntityMover.MoveDirection);
        
        CheckForStuck();

    }

    public override void Exit()
    {
        // 6. Shut down pathfinding and halt the EntityMover
        controller.aiPath.canSearch = false;
        
        if (controller.EntityMover != null)
            controller.EntityMover.SetMoveDirection(Vector2.zero);
        
    }

    private void CheckForStuck()
    {
        // Tick down interval timer
        _positionCheckTimer -= Time.deltaTime;
        
        if (_positionCheckTimer <= 0f)
        {
            // Check if the entity moved less than the threshold over the LAST 0.5 SECONDS
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
            
            // Reset the interval variables
            _lastPosition = controller.transform.position;
            _positionCheckTimer = _positionCheckInterval;
        }
    }
}