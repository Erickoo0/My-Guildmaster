using UnityEngine;
using Pathfinding;

[System.Serializable]
public class EntityKiteState : BaseKiteState
{
    private Vector3 _currentKiteDestination;
    
    public override void Enter()
    {
        if (controller.aiLerp == null)
        {
            Debug.LogError("EntityKiteState: AiLerp is null!");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }


        controller.aiLerp.canMove = true;
        controller.aiLerp.canSearch = true;

        _recheckTimer = 0f;
    }

    public override void Update()
    {
        // 1. Target lost check
        if (controller.currentTarget == null)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        // 2. If knocked back
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack) return;
        
        Vector2 currentPosition = controller.transform.position;
        Vector2 targetPosition = controller.currentTarget.position;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
        
        // 2. Check if far enough
        if (distanceToTarget >= preferredDistance)
        {
            BaseAttackState selectedAttack = controller.GetRandomAttackState();

            // If attack is valid and cooldown is over, change to attack state
            if (selectedAttack != null && controller.CheckActionCooldown())
                stateMachine.ChangeState(selectedAttack);
            else // Otherwise, change to chase state (which will just hold position until cooldown is over)
                stateMachine.ChangeState(controller.ChaseState);

            return;
        }
        
        // 3. Kitting Movement Logic
        _recheckTimer -= Time.deltaTime;

        if (_recheckTimer <= 0)
        {
            TrySetKiteDestination();
            _recheckTimer = recheckPathInterval;
        }

        // 4. Animation Logic (face the player while backing up)
        Vector2 currentVelocity = controller.aiLerp.velocity;
        
        // If moving, face the player
        if (currentVelocity.magnitude > 0.01f)
        {
            Vector2 facePlayerDirection = (targetPosition - currentPosition).normalized;
            controller.EntityAnimator.FaceDirection(facePlayerDirection);
            controller.EntityAnimator.SetMoveAnimation(currentVelocity);
            
        } 
        else // Otherwise, just idle
        {
            controller.EntityAnimator.SetMoveAnimation(Vector2.zero);
        }
    }

    public override void PhysicsUpdate() { }
    public override void HandleInput() { }
    
    public override void Exit()
    {
        //controller.aiLerp.canSearch = false;
        controller.aiLerp.canMove = false;
        controller.aiLerp.destination = controller.transform.position;
        
        controller.EntityMover?.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator?.SetMoveAnimation(Vector2.zero);
    }
    
    private void TrySetKiteDestination()
    {
        Vector2 currentPosition = controller.transform.position;
        Vector2 targetPosition = controller.currentTarget.position;


        Vector2 awayDirection =
            (currentPosition - targetPosition).normalized;



        // Try several angles instead of only straight backwards
        Vector2[] directions =
        {
            awayDirection,
            Quaternion.Euler(0,0,45) * awayDirection,
            Quaternion.Euler(0,0,-45) * awayDirection,
            Quaternion.Euler(0,0,90) * awayDirection,
            Quaternion.Euler(0,0,-90) * awayDirection
        };



        foreach(Vector2 direction in directions)
        {
            Vector3 testPosition = currentPosition + direction * kiteDistance;
            NNInfo nearest = AstarPath.active.GetNearest(testPosition, NNConstraint.Default);
            
            if(nearest.node == null) continue;


            if(!nearest.node.Walkable) continue;
            
            Vector3 validPosition = (Vector3)nearest.position;

            // Confirm the path actually exists
            ABPath path = ABPath.Construct(controller.transform.position, validPosition);

            AstarPath.StartPath(path);
            path.BlockUntilCalculated();

            if(!path.error)
            {
                _currentKiteDestination = validPosition;
                controller.aiLerp.destination = _currentKiteDestination;
                controller.aiLerp.SearchPath();
                return;
            }
        }
        
        // No valid kite position found
        controller.aiLerp.destination =
            controller.transform.position;
    }
}
