using System;
using UnityEngine;
using Random = UnityEngine.Random;
[Serializable]
public class EntityIdleState : BaseIdleState
{
	private float _idleTime;

	public override void Enter()
	{
		// 1. Set the entity to idle
		controller.EntityMover?.SetMoveDirection(Vector2.zero);
		controller.EntityAnimator?.SetMoveAnimation(Vector2.zero);
		controller._rigidBody2D.linearVelocity = Vector2.zero;

		// 2. Disable AILerp
		if (controller.AILerp != null)
		{
			controller.AILerp.canSearch = false;
			controller.AILerp.canMove = false;
		}

		_idleTime = Random.Range(0.5f, 1.5f);
	}

	public override void Update()
	{
		// 1. If theres a target
		if (controller.CurrentTarget != null && controller.ChaseState != null)
		{
			stateMachine.ChangeState(controller.ChaseState);
			return;
		}

		// 2. If theres no target
		if (_idleTime > 0)
			_idleTime -= Time.deltaTime;
		else if (controller.WanderState != null)
			stateMachine.ChangeState(controller.WanderState);
	}

	public override void PhysicsUpdate() {}

	public override void HandleInput() {}

	public override void Exit()
	{
		controller.EntityMover.SetMoveDirection(Vector2.zero);
	}
}
