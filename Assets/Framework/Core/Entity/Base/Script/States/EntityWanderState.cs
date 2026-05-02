using Pathfinding;
using UnityEngine;

public class EntityWanderState : BaseWanderState
{
    private AIPath _aiPath;
    private Vector2 _targetDestination;

    public override void Setup(EntityController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        _aiPath = controller.GetComponent<AIPath>();

        // Block AIPath direct control
        if (_aiPath != null)
        {
            _aiPath.canMove = false; // We use EntityMover for physics now
            _aiPath.updateRotation = false; // Let your animator/mover handle rotation
        }
    }

    public override void Enter()
    {
        // 1. Tell the AI to start calculating paths again
        _aiPath.canSearch = true;

        SetNewDestination();
    }

    private void SetNewDestination()
    {
        _targetDestination = (Vector2)controller.SpawnPosition + (Random.insideUnitCircle * controller.WanderRadius);
        _aiPath.destination = _targetDestination;
        _aiPath.SearchPath();
    }

    public override void Update()
    {
        // 2. Pause pathfinding execution if the entity is currently taking knockback
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack)
        {
            return;
        }

        // 3. Check if we have arrived
        if (!_aiPath.pathPending && _aiPath.reachedEndOfPath)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        // 4. Act as the bridge between AIPath (The Brain) and EntityMover (The Legs)
        Vector2 desiredVelocity = (Vector2)_aiPath.desiredVelocity;
        
        // Normalize the vector so EntityMover handles the speed via its moveSpeed variable
        Vector2 moveDirection = desiredVelocity.normalized;

        if (controller.EntityMover != null)
            controller.EntityMover.SetMoveDirection(moveDirection);
        

        // 5. Force AIPath to update its internal logic based on where our EntityMover just moved us
        _aiPath.MovementUpdate(Time.deltaTime, out Vector3 nextPos, out Quaternion nextRot);
    }

    public override void Exit()
    {
        // 6. Shut down pathfinding and halt the EntityMover
        _aiPath.canSearch = false;
        
        if (controller.EntityMover != null)
            controller.EntityMover.SetMoveDirection(Vector2.zero);
        
    }
}