using UnityEngine;

[System.Serializable]
public class EntityAttackMeleeState : BaseAttackState
{
	[Header("Melee Settings")]
	[SerializeField] private float activeDuration = 0.2f;

	private HitBox _meleeHitbox;
	private float _deactivateTimer;
	private bool _isHitBoxActive;

	public override void Setup(MobController controller, StateMachine stateMachine)
	{
		base.Setup(controller, stateMachine);

		_meleeHitbox = controller.GetComponentInChildren<HitBox>(true);

		if (_meleeHitbox == null) Debug.LogError("No HitBox found on " + controller.gameObject.name);
		else _meleeHitbox.enableHitbox = false;
	}
	
	public override void Enter()
	{
		base.Enter();
		
		_isHitBoxActive = true;
	}

	public override void Update()
	{
		base.Update();
		
		// Face the target while winding up (stops tracking once we swing)
		if (controller.currentTarget != null && !hasTriggered)
		{
			Vector2 aimDirection = (controller.currentTarget.transform.position - controller.transform.position).normalized;
			controller.EntityAnimator.FaceDirection(aimDirection);
		}
		
		// Turn the hitbox off once the active duration expires
		if (_isHitBoxActive && Time.time >= _deactivateTimer)
		{
			if (_meleeHitbox != null) _meleeHitbox.enableHitbox = false;
			_isHitBoxActive = false;
		}
	}

	protected override void HandleAnimationEvent()
	{
		if (hasTriggered) return;
		hasTriggered = true;

		if (_meleeHitbox != null && attackData != null)
		{
			_meleeHitbox.Setup(controller.gameObject, attackData.spellEffects, 999, true, false);
			_meleeHitbox.enableHitbox = true;
			
			_isHitBoxActive = true;
			_deactivateTimer = Time.time + activeDuration;
		}
	}

	public override void Exit()
	{
		if (_meleeHitbox != null) _meleeHitbox.enableHitbox = false;
		_isHitBoxActive = false;
		
		base.Exit();
	}
}
