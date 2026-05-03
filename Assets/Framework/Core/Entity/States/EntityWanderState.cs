using Pathfinding;
using UnityEngine;

public class EntityWanderState : BaseWanderState
{
    private Vector2 _targetDestination;

    [Header("Anti-Stuck Variables")] 
    private float stuckTimer;
    private float stuckTimerMax = 5f;
    private Vector2 _lastPosition;
    private float stuckThreshold = 0.1f;
    
    public override void Enter()
    {
        // 1. Tell the AI to start calculating paths again
        controller.aiPath.canSearch = true;

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
        // 2. Pause pathfinding execution if the entity is currently taking knockback
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack)
        {
            return;
        }

        // 3. Check if we have arrived
        if (!controller.aiPath.pathPending && controller.aiPath.reachedEndOfPath)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        // 4. Act as the bridge between AIPath (The Brain) and EntityMover (The Legs)
        Vector2 desiredVelocity = (Vector2)controller.aiPath.desiredVelocity;
        
        // Normalize the vector so EntityMover handles the speed via its moveSpeed variable
        Vector2 moveDirection = desiredVelocity.normalized;

        if (controller.EntityMover != null)
            controller.EntityMover.SetMoveDirection(moveDirection);
        

        // 5. Force AIPath to update its internal logic based on where our EntityMover just moved us
        controller.aiPath.MovementUpdate(Time.deltaTime, out Vector3 nextPos, out Quaternion nextRot);
        
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
        if (Vector2.Distance(_lastPosition, controller.transform.position) < stuckThreshold)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0;
        }
        
        if (stuckTimer > stuckTimerMax)
        {
            Debug.Log("Entity stuck! Resetting path.");
            stuckTimer = 0;
            SetNewDestination();
        }
        
        _lastPosition = controller.transform.position;
    }
}