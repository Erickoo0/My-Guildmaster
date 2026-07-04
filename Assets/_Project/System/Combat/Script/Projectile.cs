using System.Collections.Generic;
using UnityEngine;
public class Projectile : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private Transform _projectileVisual;

	[Header("Timers")]
	private float _currentDuration;

	[Header("Behavior Settings")]
	private bool _destroyOnCollisions;
	// Passed from EffectSpawnProjectile
	private List<Effect> _effectsList;
	private HitBox _hitBox;
	private Vector3 _linearDirection;

	[Header("Trajectory Settings")]
	private MovementType _movementType;
	private AnimationCurve _projectileCurve;
	private float _projectileMaxRelativeHeight;
	private float _projectileScale;
	private float _projectileSpeed;
	private Vector3 _projectileStartPosition;
	private Vector3 _projectileTargetPosition;
	private float _skillDamageBase;
	private SkillDataInstance _skillDataInstance;
	private float _totalDuration;
	private GameObject _user;


	private void Update()
	{
		if (_movementType == MovementType.Linear)
			UpdateLinearMovement();
		else if (_movementType == MovementType.Curved)
			UpdateCurvedMovement();

	}

	public void Setup(
		Vector3 projectileTargetPosition, float projectileSpeed, float projectileDuration, AnimationCurve projectileCurve,
		float projectileMaxHeight, GameObject user, List<Effect> onHitEffects, int maxHits, bool hitOnce, bool destroyOnMax, float projectileScale,
		float hitImpact = 0f, SkillDataInstance skillDataInstance = null, float skillDamageBase = 0f
	)
	{
		// 1. Pass the cached data
		_user = user;
		_effectsList = onHitEffects;
		_skillDataInstance = skillDataInstance;
		_skillDamageBase = skillDamageBase;
		_projectileStartPosition = transform.position;
		_projectileTargetPosition = projectileTargetPosition;
		_projectileSpeed = projectileSpeed;
		_destroyOnCollisions = destroyOnMax;
		_projectileScale = projectileScale;

		// 2. Get the Hitbox and pass it the data
		if (TryGetComponent(out _hitBox))
			_hitBox.Setup(user, onHitEffects, maxHits, hitOnce, destroyOnMax, hitImpact: hitImpact,
				skillDataInstance: _skillDataInstance, skillDamageBase: _skillDamageBase);

		// 3. Pass the duration and activate method on duration expiration
		Invoke(nameof(OnExpire), projectileDuration);

		// 4. Calculate flat 2d travel direction
		_linearDirection = (_projectileTargetPosition - _projectileStartPosition).normalized;
		if (_linearDirection == Vector3.zero) _linearDirection = Vector3.right;
		FaceTargetDirection(_linearDirection);

		// 5. Set the projectile type
		if (projectileMaxHeight <= 0f)
		{
			_movementType = MovementType.Linear;
			FaceTargetDirection(_linearDirection);

			// Enable hitbox immediately
			if (_hitBox != null) _hitBox.EnableHitBox = true;
		} else if (projectileMaxHeight > 0f)
		{
			_movementType = MovementType.Curved;

			_projectileTargetPosition = projectileTargetPosition;
			_projectileCurve = projectileCurve;
			_currentDuration = 0f;

			// Disable hitbox during movement
			if (_hitBox != null) _hitBox.EnableHitBox = false;

			// calculate Duration
			float distance = Vector3.Distance(_projectileStartPosition, _projectileTargetPosition);
			_totalDuration = distance > 0 ? (distance/projectileSpeed) : 0f;

			// Calculate height
			_projectileMaxRelativeHeight = distance*projectileMaxHeight;
		}
	}

	private void UpdateLinearMovement()
	{
		transform.position += _linearDirection*(_projectileSpeed*Time.deltaTime);
	}

	private void UpdateCurvedMovement()
	{
		// 1. Accumulate elapsed time and normalize it between 0.0 and 1.0
		_currentDuration += Time.deltaTime;
		float t = Mathf.Clamp01(_currentDuration/_totalDuration);

		// 2. Move the ROOT game object (Hitbox & Shadow) linearly
		transform.position = Vector3.Lerp(_projectileStartPosition, _projectileTargetPosition, t);

		//3. Move the projectile visual vertically
		if (_projectileVisual != null)
		{
			float heightOffset = _projectileCurve.Evaluate(t)*_projectileMaxRelativeHeight;
			_projectileVisual.localPosition = new Vector3(0, heightOffset, 0);
		}

		// 4. Destination reached logic
		if (t >= 1f)
			OnTargetReached();
	}

	private void FaceTargetDirection(Vector2 direction)
	{
		float angle = Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg;
		if (_projectileVisual != null)
		{
			_projectileVisual.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		} else // Fallback
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	private void OnTargetReached()
	{
		if (_movementType == MovementType.Curved)
			OnExpire();
	}

	private void OnExpire()
	{
		// 1. Disable the hitbox to prevent double execution
		if (_hitBox != null) _hitBox.EnableHitBox = false;

		// 2. Hide the projectile visual
		if (_projectileVisual != null) _projectileVisual.gameObject.SetActive(false);

		// 3. Build a payload from the projectiles current position
		EffectPayload expirePayload = new EffectPayload(
			user: _user,
			target: null,                            // No specific target was hit
			targetPosition: transform.position,      // Where the projectile expired
			hitDirection: (Vector2)_linearDirection, // Direction it was traveling
			hitImpactPoint: transform.position,      // Same as position for expire
			hitTargets: null                         // Fresh set — no memory chain
			);

		expirePayload.SkillDataInstance = _skillDataInstance;
		expirePayload.SkillDamageBase = _skillDamageBase;

		// 4. Execute all effects
		if (_effectsList != null)
			foreach (Effect effect in _effectsList)
				effect.Execute(expirePayload);

		// 5 Clean up
		Destroy(gameObject);
	}

	private enum MovementType { Linear, Curved }
}
