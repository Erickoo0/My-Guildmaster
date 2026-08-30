using System;
using UnityEngine;
[Serializable]
public class EntitySkillStateCharge : EntitySkillStateBase
{
	[Header("Charge Settings")]
	[SerializeField] private float _chargeSpeedMultiplier = 5.0f;
	[SerializeField] private float _overshootDistance = 4.0f;
	private float _afterImageInterval = 0.04f;
	private float _afterImageTimer;

	private HitBox _chargeHitbox;

	[Header("Timers & Tracking")]
	private float _chargeTimer;
	private Collider2D _entityCollider;
	private bool _isCharging;
	private LayerMask _originalExcludeLayers;
	private SpriteRenderer _spriteRenderer;

	public override void Setup(ControllerEntity controller, StateMachine stateMachine)
	{
		base.Setup(controller, stateMachine);

		_chargeHitbox = controller.GetComponentInChildren<HitBox>(true);
		if (_chargeHitbox == null)
		{
			Debug.LogError("No HitBox found on " + controller.gameObject.name);
			stateMachine.ChangeState(controller.IdleState);
			return;
		}
		_chargeHitbox.EnableHitBox = false;

		_entityCollider = controller.GetComponent<Collider2D>();
		_spriteRenderer = controller.gameObject.GetComponentInChildren<SpriteRenderer>(true);

		if (_entityCollider == null || _spriteRenderer == null)
		{
			Debug.LogError("No Collider2D or SpriteRenderer found on " + controller.gameObject.name + "");
			stateMachine.ChangeState(controller.IdleState);
			return;
		}
	}


	public override void Update()
	{
		base.Update();

		// Charge Phase Logic
		if (_isCharging)
		{
			_chargeTimer -= Time.deltaTime;

			// Handle AfterImages
			if (_afterImageTimer <= 0)
			{
				SpawnAfterImage();
				_afterImageTimer = _afterImageInterval;
			}
			_afterImageTimer -= Time.deltaTime;

			// End the charge phase when the timer is over
			if (_chargeTimer <= 0)
				stateMachine.ChangeState(controller.IdleState);
		}
	}

	protected override void HandleAnimationEvent()
	{
		// Safety Check
		if (HasTriggered) return;
		HasTriggered = true;

		float chargeSpeed = controller.EntityMover.moveSpeed*_chargeSpeedMultiplier;

		// 1. Freeze the Animator so it doesnt trigger animationEnd event and ending the attack before the timer
		if (controller.EntityAnimator != null)
			controller.EntityAnimator.animator.speed = 0f;

		// 2. Calculate Charge Vector and Timing
		TryUpdateAttackDirection();
		Vector2 chargeDirection = SkillDirection;

		float distanceToTarget = Vector2.Distance(controller.transform.position, controller.CurrentTarget.position);
		float totalDistance = distanceToTarget + _overshootDistance;
		_chargeTimer = totalDistance/chargeSpeed;


		// 3. Ignore collisions with victims during dash to avoid getting stuck
		_originalExcludeLayers = _entityCollider.excludeLayers;
		_entityCollider.excludeLayers |= _chargeHitbox.VictimLayer;


		// 4. Bundle the data and pass it to the hitbox
		CombatContext combatContext = new CombatContext
		{
			User = controller.gameObject,
			EffectsList = SkillDataInstance.EffectsList,
			SkillDataInstance = SkillDataInstance,
			SkillDamageBase = DamageCalculator.ComputeBaseSkillDamage(SkillDataInstance, controller.StatProvider) // Assuming parent state tracks pre-computed base damage
		};

		HitBoxSettings hitBoxSettings = new HitBoxSettings
		{
			MaxEnemiesHit = 999, // Dash typically hits anyone in the path
			HitOncePerTarget = true,
			DestroyOnMaxHits = false,
			HitImpact = SkillDataInstance.HitImpact
		};

		_chargeHitbox.Setup(combatContext, hitBoxSettings);
		_chargeHitbox.EnableHitBox = true;

		// 5. Tell EntityMover to take over movement and pause AILerp
		controller.EntityMover.StartCharge(chargeDirection, chargeSpeed);

		_isCharging = true;
		_afterImageTimer = 0f;
	}



	public override void Exit()
	{
		_isCharging = false;

		controller.EntityMover.StopCharge();
		_chargeHitbox.EnableHitBox = false;
		_entityCollider.excludeLayers = _originalExcludeLayers;

		base.Exit();
	}

	private void SpawnAfterImage()
	{
		GameObject entity = controller.gameObject;

		if (AfterImageManager.Instance != null && _spriteRenderer != null)
			AfterImageManager.Instance.SpawnAfterImage(_spriteRenderer.sprite, entity.transform.position, Color.red);
	}
}
