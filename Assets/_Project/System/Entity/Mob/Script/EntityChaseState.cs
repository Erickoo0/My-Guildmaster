using System;
using UnityEngine;
[Serializable]
public class EntityChaseState : EntityChaseStateBase
{
	public override void Enter()
	{
		// Tell the AI t o start calculating paths again
		if (controller.AILerp != null)
		{
			controller.AILerp.canSearch = true;

			// Set the initial destination and search for a path
			if (controller.CurrentTarget != null)
			{
				controller.AILerp.destination = controller.CurrentTarget.position;
				controller.AILerp.SearchPath();
			}
		}
	}

	public override void Update()
	{
		// 1. Safety check
		if (controller.AILerp == null) return;

		// 2. If current target is cleared
		if (controller.CurrentTarget == null)
		{
			stateMachine.ChangeState(controller.IdleState);
			return;
		}

		// 3. If being knocked back
		if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack) return;

		Vector2 currentPosition = controller.transform.position;
		Vector2 targetPosition = controller.CurrentTarget.position;
		float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

		// Update the destination
		controller.AILerp.destination = targetPosition;

		// 4. Check if any attacks have requirements met
		EntitySkillStateBase selectedAttack = controller.SkillController.GetRandomSkillState();

		// 5. Execute attack
		if (selectedAttack != null)
		{
			// Stop moving
			controller.AILerp.canMove = false;
			controller.AILerp.destination = currentPosition; // Clear destination target to stop moving smoothly

			// Face the target
			Vector2 faceDirection = (targetPosition - currentPosition).normalized;
			controller.EntityAnimator.FaceDirection(faceDirection);

			// Execute the attack
			stateMachine.ChangeState(selectedAttack);
		} else // 5. If there is no valid attacks (likely out of range). Keep chasing OR kitting
		{
			// Kite Logic
			if (controller.KiteState != null && controller.KiteState.CheckShouldKite(distanceToTarget))
			{
				stateMachine.ChangeState(controller.KiteState);
				return;
			}

			// Chase Logic
			controller.AILerp.canMove = true;
			controller.AILerp.destination = targetPosition;

			controller.EntityAnimator.SetMoveAnimation(controller.AILerp.velocity);
		}
	}

	public override void PhysicsUpdate() {}
	public override void HandleInput() {}

	public override void Exit()
	{
		// 1. Shut down pathfinding search
		if (controller.AILerp != null)
		{
			controller.AILerp.canSearch = false;
			controller.AILerp.canMove = false;
			controller.AILerp.destination = controller.transform.position;
		}

		// 2. Set movement to zero
		controller.EntityMover?.SetMoveDirection(Vector2.zero);
		controller.EntityAnimator?.SetMoveAnimation(Vector2.zero);
	}
}
