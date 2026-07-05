using System.Collections.Generic;
using UnityEngine;
public abstract class HitBox : MonoBehaviour
{
	[Header("References")]
	[HideInInspector] public Collider2D EntityCollider;

	[Header("Base Settings")]
	public LayerMask VictimLayer;
	private CombatContext _combatContext;
	private bool _enableHitBox = false;
	private HitBoxSettings _hitBoxSettings;

	public bool EnableHitBox
	{
		get => _enableHitBox;
		set
		{
			_enableHitBox = value;
			if (EntityCollider != null)
				EntityCollider.enabled = value;
		}
	}

	protected virtual void Awake() => EntityCollider = GetComponent<Collider2D>();

	public void OnTriggerEnter2D(Collider2D other) => OnTriggerStay2D(other);

	public void OnTriggerStay2D(Collider2D other)
	{
		// Safety Checks
		if (!EnableHitBox || other.isTrigger) return;
		if (((1 << other.gameObject.layer) & VictimLayer) == 0) return;
		if (other.transform.root.gameObject == _combatContext.User.transform.root.gameObject) return;

		// Check if the target is a valid victim or a wall
		IDamagable victim = other.GetComponentInParent<IDamagable>();
		bool isWall = other.gameObject.layer == LayerMask.NameToLayer("Collisions");

		if (victim == null && !isWall) return;
		if (victim != null && _hitBoxSettings.HitOncePerTarget && _hitBoxSettings.InheritedTargetsList.Contains(victim)) return;

		CalculateImpactPhysics(other, out Vector2 direction, out Vector2 impactPoint);

		// 1. Construct the Payload
		EffectPayload effectPayload = new EffectPayload(_combatContext.User)
		{
			Target = other.gameObject,
			TargetPosition = other.transform.position,
			HitDirection = direction,
			HitImpactPoint = impactPoint,
			HitTargets = _hitBoxSettings.InheritedTargetsList,
			SkillDataInstance = _combatContext.SkillDataInstance,
			SkillDamageBase = _combatContext.SkillDamageBase
		};

		// 2. Execute all skill effects
		bool anyEffectSucceeded = false;
		if (_combatContext.EffectsList?.Count > 0)
		{
			foreach (Effect effect in _combatContext.EffectsList)
			{
				if (effect.Execute(effectPayload))
					anyEffectSucceeded = true;
			}
		}

		// 3. Handle successful hits
		if (anyEffectSucceeded)
		{
			if (victim != null) _hitBoxSettings.InheritedTargetsList.Add(victim);
			HandlePostHit(other);

			if (_hitBoxSettings.HitImpact > 0f)
				EventBus.RequestHitImpact(_hitBoxSettings.HitImpact, impactPoint);

			if (isWall)
			{
				if (_hitBoxSettings.DestroyOnMaxHits)
					Destroy(gameObject);
				return;
			}

			if (_hitBoxSettings.MaxEnemiesHit > 0)
			{
				_hitBoxSettings.MaxEnemiesHit--;
				if (_hitBoxSettings.MaxEnemiesHit <= 0)
				{
					EnableHitBox = false;
					if (_hitBoxSettings.DestroyOnMaxHits)
						Destroy(gameObject);
				}
			}
		}
	}

	public virtual void Setup(CombatContext combatContext, HitBoxSettings hitBoxSettings)
	{
		_combatContext = combatContext;
		_hitBoxSettings = hitBoxSettings;

		// Ensure inherited targets is never null
		_hitBoxSettings.InheritedTargetsList ??= new HashSet<IDamagable>();
	}

	protected abstract void CalculateImpactPhysics(Collider2D other, out Vector2 knockbackDirection, out Vector2 impactPoint);
	protected virtual void HandlePostHit(Collider2D other) {}
}
