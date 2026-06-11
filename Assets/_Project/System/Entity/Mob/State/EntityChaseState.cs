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
        
        // Check if target is in range
        if (!controller.IsTargetInRange(controller.DetectionLostRange))
        {
            controller.currentTarget = null;
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        Vector2 currentPosition = controller.transform.position;
        Vector2 targetPosition = controller.currentTarget.position;
        float distance = Vector2.Distance(currentPosition, targetPosition);
        
        // Update A* destination to follow target
        controller.aiPath.destination = targetPosition;

        if (distance <= controller.ActionRange)
        {
            // 1. In range, halt movement
            controller.EntityMover.SetMoveDirection(Vector2.zero);
            Vector2 faceDirection = (targetPosition - currentPosition).normalized;
            controller.EntityAnimator.FaceDirection(faceDirection);
            
            // 2. Attack if off cooldown
            if (controller.CheckActionCooldown())
            {
                stateMachine.ChangeState(controller.AttackState);
                return;
            }
        }
        else
        {
            // 3. Out of range, move towards target
            Vector2 moveDirection = ((Vector2)controller.aiPath.desiredVelocity).normalized;
            controller.EntityMover.SetMoveDirection(moveDirection);
            controller.EntityAnimator.SetMoveAnimation(moveDirection);
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
