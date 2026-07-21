using System;
using UnityEngine;
[Serializable]
public abstract class PlayerSkillStateBase : State<PlayerController>
{
	[Header("Spell Meta Data")]
	[SerializeField] protected string _skillID;
	[HideInInspector] public int CurrentSlotIndex = -1;

	[Header("Cast Bar")]
	protected CastBar CastBar;

	protected bool HasTriggered = false;
	protected bool IsCasting;
	protected SkillData SkillDataSource;
	protected SkillTree SkillTree;
	protected IStatProvider StatProvider;
	public SkillDataInstance SkillDataInstance { get; private set; }

	public float MpCost => SkillDataInstance != null ? SkillDataInstance.MpCost : 0f;
	public SkillTree SkillTreeInstance => SkillTree;

	public override void Setup(PlayerController controller, StateMachine stateMachine)
	{
		base.Setup(controller, stateMachine);

		StatProvider = controller.GetComponent<IStatProvider>();
		EventBus.OnSkillTreeLedgerChanged += HandleSkillTreeLedgerChanged;

		CastBar = controller.SkillController?.CastBar;

		if (controller.SkillController != null && controller.SkillController.SkillDatabase != null)
		{
			// 1. Find the SkilLData from database
			SkillDataSource = controller.SkillController.SkillDatabase.GetSkillDataByID<SkillData>(_skillID);
			if (SkillDataSource != null)
			{
				// 2. Find the SkillTree for this SkillData
				SkillTree = controller.SkillController.SkillTreeDatabase != null
					? controller.SkillController.SkillTreeDatabase.GetSkillTreeByID(_skillID)
					: null;

				// 3. Compile SkilLDataInstance with SkillTree and SkillDataSource
				RefreshSkillDataInstance();
			}
		} else
			Debug.LogError("PlayerSkillCastState: PlayerController.SkillController.SkillDatabase is null");
	}

	public override void Enter()
	{
		controller.EntityAnimator.OnAnimationEventRequested += HandleAnimationEvent;
		HasTriggered = false;
		IsCasting = false;

		// Safety check
		if (SkillDataSource == null || CurrentSlotIndex == -1)
		{
			Debug.LogWarning($"spellInstance or CurrentSlotIndex is null for {controller.gameObject.name}");
			stateMachine.ChangeState(controller.IdleState);
			return;
		}

		controller.EntityMover.SetMoveDirection(Vector2.zero);
		controller.EntityAnimator.StartSpellAnimation(SkillDataInstance.AnimationTag);
		controller.EntityAnimator.animator.Update(0f); // Force transition

		// Get the exact time the event fires, rather than just clip length
		float eventTime = GetAnimationEventTime();

		// Cast Logic
		float castSpeedMultiplier = eventTime/SkillDataInstance.CastTime;
		controller.EntityAnimator.animator.speed = castSpeedMultiplier;
		if (SkillDataInstance.DisplayCastBar)
		{
			IsCasting = true;
			CastBar?.BeginCast(SkillDataInstance.CastTime, SkillDataInstance.Name);
		}
	}

	public void OnDestroy() => EventBus.OnSkillTreeLedgerChanged -= HandleSkillTreeLedgerChanged;

	public override void Update()
	{
		// Safety check
		if (SkillDataInstance == null || CurrentSlotIndex == -1) return;

		// 1. Check if the skill keybind is still being held
		if (!HasTriggered && IsCasting)
		{
			if (!controller.SkillController.IsSpellKeyHeld(CurrentSlotIndex))
			{
				CastBar?.StopCast();
				stateMachine.ChangeState(controller.IdleState);
				return;
			}
		}

		// 2. Stop the cast bar when the skill executes
		if (HasTriggered && IsCasting)
		{
			IsCasting = false;
			CastBar?.StopCast();
		}

		if (!controller.EntityAnimator.animator.GetBool(SkillDataInstance.AnimationTag))
			stateMachine.ChangeState(controller.IdleState);
	}

	public override void Exit()
	{
		controller.EntityAnimator.OnAnimationEventRequested -= HandleAnimationEvent;

		IsCasting = false;
		CastBar?.StopCast();

		controller.EntityAnimator.animator.speed = 1f;
		controller.EntityAnimator.animator.SetBool(SkillDataInstance.AnimationTag, false);

		if (HasTriggered)
			controller.SkillController.SetActionCooldown();
	}

	public override void PhysicsUpdate() {}
	public override void HandleInput() {}

	// Child classes should implement this method to handle the attack logic
	protected abstract void HandleAnimationEvent();

	//----Helper Methods----
	private float GetAnimationEventTime()
	{
		var clipInfo = controller.EntityAnimator.animator.GetCurrentAnimatorClipInfo(0);
		if (clipInfo.Length == 0) return 0f;

		AnimationClip clip = clipInfo[0].clip;

		foreach (var eventInfo in clip.events)
		{
			// This string must perfectly match the method in EntityAnimator.cs
			if (eventInfo.functionName == "RequestAnimationEvent")
			{
				return eventInfo.time;
			}
		}

		// Default to full clip length if you forgot to place an event on the timeline
		return clip.length;
	}

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
