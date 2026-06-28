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
        // 1. Safety check
        if (controller.aiLerp == null) return;
        
        // 2. If current target is cleared
        if (controller.currentTarget == null)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        // 3. If being knocked back
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack) return;
        
        Vector2 currentPosition = controller.transform.position;
        Vector2 targetPosition = controller.currentTarget.position;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
        
        // Update the destination
        controller.aiLerp.destination = targetPosition;

        // 4. Check if any attacks have requirements met
        SkillStateBase selectedAttack = controller.SkillController.GetRandomSkillState();
        
        // 5. Execute attack
        if (selectedAttack != null)
        {
            // Stop moving
            controller.aiLerp.canMove = false;
            controller.aiLerp.destination = currentPosition; // Clear destination target to stop moving smoothly
            
            // Face the target
            Vector2 faceDirection = (targetPosition - currentPosition).normalized;
            controller.EntityAnimator.FaceDirection(faceDirection); 
            
            // Check if the action cooldown is over
            if (controller.SkillController.CheckActionCooldown())
                stateMachine.ChangeState(selectedAttack);
        }
        else // 5. If there is no valid attacks (likely out of range). Keep chasing OR kitting
        {
            // Kite Logic
            if (controller.KiteState != null && controller.KiteState.CheckShouldKite(distanceToTarget))
            {
                stateMachine.ChangeState(controller.KiteState);
                return;
            }
            
            // Chase Logic
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
