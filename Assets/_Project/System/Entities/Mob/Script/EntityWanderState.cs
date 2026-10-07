using System;
using UnityEngine;
using Random = UnityEngine.Random;
[Serializable]
public class EntityWanderState : EntityWanderStateBase
{
	private readonly float _positionCheckInterval = 0.5f;
	private readonly float _stuckThreshold = 0.1f;
	private readonly float _stuckTimerMax = 5f;
	private Vector2 _lastPosition;
	private float _positionCheckTimer;

	[Header("Anti-Stuck Variables")]
	private float _stuckTimer;
	private Vector2 _targetDestination;

	public override void Enter()
	{
		// 1. Tell the AI to start calculating paths again
		if (controller.AILerp != null)
		{
			controller.AILerp.canSearch = true;
			controller.AILerp.canMove = true;
		}

		// 2. Reset state flags
		_stuckTimer = 0f;
		_positionCheckTimer = _positionCheckInterval;
		_lastPosition = controller.transform.position;

		SetNewDestination();
	}

	private void SetNewDestination()
	{
		if (controller.AILerp == null) return;

		_targetDestination = (Vector2)controller.SpawnPosition + (Random.insideUnitCircle*controller.WanderRadius);
		controller.AILerp.destination = _targetDestination;
		controller.AILerp.SearchPath();
	}

	public override void Update()
	{
		// 1. If theres a target
		if (controller.CurrentTarget != null && controller.ChaseState != null)
		{
			stateMachine.ChangeState(controller.ChaseState);
			return;
		}

		// 2. If being knocked back
		if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack)
			return;

		// 3. If arrived at destination
		if (controller.AILerp != null && !controller.AILerp.pathPending && controller.AILerp.reachedEndOfPath)
		{
			stateMachine.ChangeState(controller.IdleState);
			return;
		}

		// 4. Feed the animator using AIPath's native velocity
		if (controller.EntityAnimator != null && controller.AILerp != null)
			controller.EntityAnimator.SetMoveAnimation(controller.AILerp.velocity);


		// 5. Apply anti-stuck safety
		CheckForStuck();
	}

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
		controller._rigidBody2D.linearVelocity = Vector2.zero;
		controller.EntityMover?.SetMoveDirection(Vector2.zero);
		controller.EntityAnimator?.SetMoveAnimation(Vector2.zero);
	}

	private void CheckForStuck()
	{
		_positionCheckTimer -= Time.deltaTime;

		if (_positionCheckTimer <= 0f)
		{
			// Check if the entity moved less than the threshold over the last 0.5 seconds
			if (Vector2.Distance(_lastPosition, controller.transform.position) < _stuckThreshold)
			{
				_stuckTimer += _positionCheckInterval;
			} else
			{
				_stuckTimer = 0;
			}

			if (_stuckTimer > _stuckTimerMax)
			{
				Debug.Log("SkillControllerEntity stuck! Resetting path.");
				_stuckTimer = 0;
				SetNewDestination();
			}

			_lastPosition = controller.transform.position;
			_positionCheckTimer = _positionCheckInterval;
		}
	}
}
