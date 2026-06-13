using UnityEngine;

[System.Serializable]
public class EntityIdleState : BaseIdleState
{
    private float _idleTime;
    
    public override void Enter()
    {
        // 1. Set the entity to idle
        controller.EntityMover?.SetMoveDirection(Vector2.zero); 
        controller.EntityAnimator?.SetMoveAnimation(Vector2.zero);
        controller.rigidBody2D.linearVelocity = Vector2.zero;
        
        // 2. Disable AILerp
        if (controller.aiLerp != null)
        {
            controller.aiLerp.canSearch = false;
            controller.aiLerp.canMove = false;
        }
        
        _idleTime = Random.Range(0.5f, 1.5f);
    }

    public override void Update()
    {
        // if  no target, wait for a random time, then switch to wander state
        if (_idleTime > 0) _idleTime -= Time.deltaTime;
        else if (controller.WanderState != null)
            stateMachine.ChangeState(controller.WanderState);
    }
    
    public override void PhysicsUpdate() { }
    
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityMover.SetMoveDirection(Vector2.zero);
    }
}
