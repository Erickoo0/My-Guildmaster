using UnityEngine;

public class GoblinArcherAttackState : BaseActionState
{
    public override void Enter()
    {
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator.animator.SetBool("IsAttacking", true);
        Debug.Log("GoblinArcherAttackState entered");
    }

    public override void Update()
    {
        if (controller.EntityAnimator.animator.GetBool("IsAttacking") == false)
        {
            Debug.Log("GoblinArcherAttackState exited");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
    }
}
