using UnityEngine;
using Pathfinding;

[System.Serializable]
public class EntityChaseState : BaseChaseState
{

    public override void Enter()
    {
        // Tell the AI t o start calculating paths again
        if (controller.aiPath != null)
        {
            controller.aiPath.canSearch = true;

            // Set the initial destination and search for a path
            if (controller.currentTarget != null)
            {
                controller.aiPath.destination = controller.currentTarget.position;
                controller.aiPath.SearchPath();
            }
        }
    }

    public override void Update()
    {
        // Safety check
        if (controller.currentTarget == null || controller.aiPath == null)
        {
            Debug.LogWarning("EntityChaseState: Target is null or AIPath is null");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        // Pause the state if the entity is knocked back
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack)
        {
            return;
        }
        
        Vector2 currentPosition = controller.transform.position;
        Vector2 targetPosition = controller.currentTarget.position;
        float distance = Vector2.Distance(currentPosition, targetPosition);
        
        // Update A* destination to follow target
        controller.aiPath.destination = targetPosition;
        
        // If in action range and action cooldown is over, switch to the action state
        if (distance <= controller.ActionRange && controller.CheckActionCooldown())
        {
            stateMachine.ChangeState(controller.AttackState);
            return;
        }
        else if (distance <= controller.ActionRange) // If in action range, but action cooldown is not over
        {
            controller.EntityMover.SetMoveDirection(Vector2.zero);
            
            Vector2 faceDirection = (targetPosition - currentPosition).normalized;
            controller.EntityAnimator.FaceDirection(faceDirection);
        }
        
        // Movement Logic
        if (distance > controller.ActionRange)
        {
            Vector2 moveDirection = ((Vector2)controller.aiPath.desiredVelocity).normalized;
            controller.EntityMover.SetMoveDirection(moveDirection);

        }
        else // We are close enough to attack, but maybe waiting for attack cooldown
        {
            controller.EntityMover.SetMoveDirection(Vector2.zero);
        }
        
        // Tell the AIPath where the transform actually is so it can calculate the next velocity correctly
        controller.aiPath.MovementUpdate(Time.deltaTime, out Vector3 nextPos, out Quaternion nextRot);    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

    public override void Exit()
    {
        if (controller.EntityMover != null) 
            controller.EntityMover.SetMoveDirection(Vector2.zero);
        
        if (controller.aiPath != null)
            controller.aiPath.canSearch = false;
    }
}
