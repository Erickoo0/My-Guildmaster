using UnityEngine;
using Pathfinding;

[System.Serializable]
public class EntityChaseState : BaseChaseState
{

    public override void Enter()
    {
        // Tell the AI t o start calculating paths again
        if (controller.aiLerp != null)
        {
            controller.aiLerp.canSearch = true;

            // Set the initial destination and search for a path
            if (controller.currentTarget != null)
            {
                controller.aiLerp.destination = controller.currentTarget.position;
                controller.aiLerp.SearchPath();
            }
        }
    }

    public override void Update()
    {
        if (controller.currentTarget == null || controller.aiLerp == null) return;
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack) return;
        
        if (!controller.IsTargetInRange(controller.DetectionLostRange))
        {
            controller.currentTarget = null;
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        Vector2 currentPosition = controller.transform.position;
        Vector2 targetPosition = controller.currentTarget.position;
        float distance = Vector2.Distance(currentPosition, targetPosition);
        
        // Simply update the destination. AIPath handles the movement physics automatically!
        controller.aiLerp.destination = targetPosition;

        // In Range
        if (distance <= controller.ActionRange)
        {
            // Stop moving
            controller.aiLerp.canMove = false;
            controller.aiLerp.destination = currentPosition; // Clear destination target to stop moving smoothly
            
            Vector2 faceDirection = (targetPosition - currentPosition).normalized;
            controller.EntityAnimator.FaceDirection(faceDirection); 
            
            if (controller.CheckActionCooldown())
            {
                stateMachine.ChangeState(controller.AttackState);
                return;
            }
        }
        else // Out of range
        {
            controller.aiLerp.canMove = true;
            controller.aiLerp.destination = targetPosition;
            
            controller.EntityAnimator.SetMoveAnimation(controller.aiLerp.velocity);
        }
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

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
        controller.EntityMover?.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator?.SetMoveAnimation(Vector2.zero);
    }
}
