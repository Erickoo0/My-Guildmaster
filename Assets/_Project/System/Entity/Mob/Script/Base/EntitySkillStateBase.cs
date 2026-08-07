using System;
using UnityEngine;
[Serializable]
public abstract class EntitySkillStateBase : EntityActionStateBase
{
	[Header("Attack Meta Data")]
	[SerializeField] protected string _skillID;

	protected float DefaultDetectionLostRange;
	protected bool HasTriggered = false;
	protected bool IsCasting;

	[Header("References")]
	protected SkillData SkillDataSource;
	protected Vector2 SkillDirection;
	protected SkillTree SkillTree; // Usually unused for entities
	public SkillDataInstance SkillDataInstance { get; private set; }
	public float SelectionWeight => SkillDataSource != null ? SkillDataSource.SelectionWeight : 0f;

	public override void Setup(ControllerEntity controllerEntity, StateMachine stateMachine)
	{
		base.Setup(controllerEntity, stateMachine);

		EventBus.OnSkillTreeLedgerChanged += HandleSkillTreeLedgerChanged;

		// 1. Get the skill data
		if (controllerEntity.SkillController?.SkillDatabase != null)
		{
			SkillDataSource = controllerEntity.SkillController.SkillDatabase.GetSkillDataByID<SkillData>(_skillID);
			if (SkillDataSource != null)
			{
				// 2. Find the SkillTree for this skill (Most likely null for non-players)
				SkillTree skillTree = controllerEntity.SkillController.SkillTreeDatabase != null
					? controllerEntity.SkillController.SkillTreeDatabase.GetSkillTreeByID(_skillID)
					: null;

				// 3. Compile SkilLDataInstance with SkillTree
				RefreshSkillDataInstance();
			} else Debug.LogError($"PlayerSkillStateBase: SkillDatabase is missing SkillID {_skillID}");
		} else Debug.LogError("PlayerSkillStateBase: SkillController or SkillDatabase is null");
	}

	public void OnDestroy() => EventBus.OnSkillTreeLedgerChanged -= HandleSkillTreeLedgerChanged;

	public override void Enter()
	{
		controller.EntityAnimator.OnAnimationEventRequested += HandleAnimationEvent;

		HasTriggered = false;
		IsCasting = false;

		// Safety check
		if (SkillDataSource == null)
		{
			Debug.LogWarning($"EntitySkillStateBase: SkillDataSource is null for {controller.gameObject.name}");
			stateMachine.ChangeState(controller.IdleState);
			return;
		}

		// 1. Change to EntityMover
		if (controller.AILerp != null)
		{
			controller.AILerp.canSearch = false;
			controller.AILerp.canMove = false;
			controller.AILerp.destination = controller.transform.position;
		}

		// Force Rigidbody velocity zero
		if (controller._rigidBody2D != null)
			controller._rigidBody2D.linearVelocity = Vector2.zero;

		// 2. Save default ranges
		DefaultDetectionLostRange = controller.TargetLostRange;
		controller.TargetLostRange = DefaultDetectionLostRange*4f;

		// 3. Force look direction update
		TryUpdateAttackDirection();
		controller.EntityAnimator.FaceDirection(SkillDirection);

		// 4. Start the animation
		controller.EntityMover.SetMoveDirection(Vector2.zero);
		controller.EntityAnimator.StartSpellAnimation(SkillDataInstance.AnimationTag);
		controller.EntityAnimator.animator.Update(0f); // Force transition

		// 5. Use the helper method to cleanly grab the timing
		float eventTime = GetAnimationEventTime();

		// 6. Casting logic
		float castSpeedMultiplier = eventTime/SkillDataInstance.CastTime;
		controller.EntityAnimator.animator.speed = castSpeedMultiplier;
		if (SkillDataInstance.DisplayCastBar)
		{
			IsCasting = true;
			controller.SkillController.CastBar?.BeginCast(SkillDataInstance.CastTime, SkillDataInstance.Name);
		}
	}

	public override void Update()
	{
		// 1. Disable cast bar after attack has triggered
		if (HasTriggered && IsCasting)
		{
			IsCasting = false;
			controller.SkillController.CastBar?.StopCast();
		}

		// 2. Transition to Idle as soon as animation gets set to false via animationEnd event
		if (!controller.EntityAnimator.animator.GetBool(SkillDataInstance.AnimationTag))
			stateMachine.ChangeState(controller.IdleState);

		// 3. Change to Chase State if knocked back
		if (controller.EntityMover.IsKnockedBack)
		{
			stateMachine.ChangeState(controller.ChaseState);
			return;
		}

		// 4. Face the target while winding up
		if (!HasTriggered)
			TryUpdateAttackDirection();
		controller.EntityAnimator.FaceDirection(SkillDirection);

	}

	public override void Exit()
	{
		controller.EntityAnimator.OnAnimationEventRequested -= HandleAnimationEvent;

		// 1. Disable cast bar
		IsCasting = false;
		controller.SkillController.CastBar?.StopCast();

		// 2. Reset animation
		controller.EntityAnimator.animator.speed = 1f;
		controller.EntityAnimator.animator.SetBool(SkillDataInstance.AnimationTag, false);
		controller.EntityAnimator.RequestAnimationCancel();
		controller.EntityAnimator.animator.Update(0f);

		// 3. Face direction 
		if (TryUpdateAttackDirection())
		{
			controller.EntityAnimator.FaceDirection(SkillDirection);
			controller.EntityAnimator.animator.Update(0f);
		}

		// 4. Restore default range
		controller.TargetLostRange = DefaultDetectionLostRange;

		// 5. Set cooldown
		if (HasTriggered)
			controller.SkillController.TriggerSkillCooldown(_skillID);
	}

	public override void PhysicsUpdate() {}
	public override void HandleInput() {}

	protected abstract void HandleAnimationEvent();

	// --- Helper Methods ---

	private float GetAnimationEventTime()
	{
		var clipInfo = controller.EntityAnimator.animator.GetCurrentAnimatorClipInfo(0);
		if (clipInfo.Length == 0) return 0f;

		AnimationClip clip = clipInfo[0].clip;

		foreach (var eventInfo in clip.events)
		{
			if (eventInfo.functionName == "RequestAnimationEvent")
			{
				return eventInfo.time;
			}
		}

		// Default to full clip length if there is no event
		return clip.length;
	}

	protected bool TryUpdateAttackDirection()
	{
		if (controller.CurrentTarget == null)
		{
			Debug.Log("EntitySkillStateBase: Target has become null");
			return false;
		}

		SkillDirection = ((Vector2)controller.CurrentTarget.position - (Vector2)controller.transform.position).normalized;

		return true;
	}

	public bool CheckRequirementsMet(GameObject context) => SkillDataInstance != null && SkillDataInstance.AreRequirementsMet(context);

	private void HandleSkillTreeLedgerChanged(string skillDataID)
	{
		// Ignore the event if it's not for this state's skill
		if (skillDataID != _skillID) return;
		RefreshSkillDataInstance();
	}

	private void RefreshSkillDataInstance()
	{
		// Safety Check
		if (SkillDataSource == null) return;

		// 1. Recompile the SkilLDataInstance with the SkillTree's modifiers if it exists,
		// otherwise, just create a new instance from the SkillDataSource
		SkillDataInstance = SkillTree != null
			? SkillTreeCompiler.CompileSkillDataInstance(SkillTree)
			: SkillDataSource.CreateSkillDataInstance();
	}
}
