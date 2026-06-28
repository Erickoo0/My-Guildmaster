using UnityEngine;

[System.Serializable]
public class EntitySkillStateMelee : SkillStateBase
{
	[Header("Melee Settings")]
	[SerializeField] private float _activeDuration = 0.2f;

	[Header("Attack Movement Settings")]
	[SerializeField] private float _lungeSpeed = 12.0f;
	[SerializeField] private float _lungeDelay = 0.1f;
	[SerializeField] private float _lungeStoppingDistance = 1.5f;
	
	private float _currentLungeDelay;
	private bool _hasLunged = false;
	private bool _reachedTarget = false;
	
	private Collider2D _entityCollider;
	private LayerMask _originalExcludeLayers; 
	private HitBox _meleeHitbox;
	private float _deactivateTimer;

	public override void Setup(MobController controller, StateMachine stateMachine)
	{
		base.Setup(controller, stateMachine);

		_meleeHitbox = controller.GetComponentInChildren<HitBox>(true);
		if (_meleeHitbox == null)
		{
			Debug.LogError("No HitBox found on " + controller.gameObject.name);
			stateMachine.ChangeState(controller.IdleState);
			return;
		}	
		_meleeHitbox.EnableHitBox = false;
		
		_entityCollider = controller.GetComponent<Collider2D>();

		if (_entityCollider == null)
		{
			Debug.LogError("No Collider2D found on " + controller.gameObject.name + "");
			stateMachine.ChangeState(controller.IdleState);
			return;
		}
	}

	public override void Enter()
	{
		base.Enter();
		
		// Reset state variables
		_currentLungeDelay = _lungeDelay;
		_hasLunged = false;
		_reachedTarget = false;
	}
	
	public override void Update()
	{
		base.Update();
		// Safety Check
		if (stateMachine.CurrentState != this) return;
		
		// 1. Lunge Initialization
		if (!_hasLunged)
		{
			_currentLungeDelay -= Time.deltaTime;
			if (_currentLungeDelay <= 0)
			{
				controller.EntityMover.StartMeleeLunge(SkillDirection, _lungeSpeed);
				_hasLunged = true;
				
				// Ignore collisions during lunge
				_originalExcludeLayers = _entityCollider.excludeLayers;
				_entityCollider.excludeLayers |= _meleeHitbox.VictimLayer;
			}
		}
		// 2. Handle Lunge Movement & Stopping Logic
		else if (!HasTriggered && !_reachedTarget && controller.currentTarget != null)
		{
			float distanceToTarget = Vector2.Distance(controller.transform.position, controller.currentTarget.position);
			
			if (distanceToTarget <= _lungeStoppingDistance)
			{
				controller.EntityMover.StopMeleeLunge();
				_reachedTarget = true;
			} 
			else
			{
				controller.EntityMover.SetMoveDirection(SkillDirection);
			}
		}
		
		// 3. Turn the hitbox off once the active Duration expires
		if (_meleeHitbox.EnableHitBox && Time.time >= _deactivateTimer)
			_meleeHitbox.EnableHitBox = false;
		
	}

	protected override void HandleAnimationEvent()
	{
		if (HasTriggered) return;
		HasTriggered = true;

		// 1. Position the Hitbox
		float angle = Mathf.Atan2(SkillDirection.y, SkillDirection.x) * Mathf.Rad2Deg;
		_meleeHitbox.transform.rotation = Quaternion.Euler(0, 0, angle);
		
		// 2. Pass the Data and turn on the hitbox
		_meleeHitbox.Setup(controller.gameObject, SkillDataInstance.EffectsList, 999, true, false);
		_meleeHitbox.EnableHitBox = true;
		
		// 3. Stop the lunge
		controller.EntityMover.StopMeleeLunge();	
		
		_deactivateTimer = Time.time + _activeDuration;
	}

	public override void Exit()
	{
		_hasLunged = false;
		
		controller.EntityMover.StopMeleeLunge();
		_meleeHitbox.EnableHitBox = false;
		_entityCollider.excludeLayers = _originalExcludeLayers;
		
		base.Exit();
	}
}
