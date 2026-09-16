using UnityEngine;
public class Projectile : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private Transform _projectileVisual;
	private CombatContext _combatContext;

	private float _currentDuration; // Accumulated duration for curved movement
	private HitBox _hitBox;
	private HitBoxSettings _hitBoxSettings;
	private Vector3 _linearDirection;

	[Header("Flight Settings")]
	private MovementType _movementType;
	private ProjectileFlightData _projectileFlightData;
	private float _projectileMaxRelativeHeight;
	private Vector3 _projectileStartPosition; // Cache the start position
	private float _totalDuration;

	private void Update()
	{
		if (_movementType == MovementType.Linear)
			UpdateLinearMovement();
		else if (_movementType == MovementType.Curved)
			UpdateCurvedMovement();

	}

	public void Setup(CombatContext combatContext, HitBoxSettings hitBoxSettings, ProjectileFlightData projectileFlightData)
	{
		_combatContext = combatContext;
		_hitBoxSettings = hitBoxSettings;
		_projectileFlightData = projectileFlightData;

		_projectileStartPosition = transform.position;

		// 1. Pass the Data to the hitbox
		if (TryGetComponent(out _hitBox))
			_hitBox.Setup(combatContext, hitBoxSettings);

		// 2. Set the timer
		Invoke(nameof(OnExpire), _projectileFlightData.Duration);

		// 3. Set the travel direction
		_linearDirection = (_projectileFlightData.TargetPosition - _projectileStartPosition).normalized;
		if (_linearDirection == Vector3.zero) _linearDirection = Vector3.right;
		FaceTargetDirection(_linearDirection);

		// 4. Set the projectile type
		if (_projectileFlightData.MaxHeight <= 0f)
		{
			_movementType = MovementType.Linear;
			if (_hitBox != null)
				_hitBox.EnableHitBox = true;
		} else
		{
			_movementType = MovementType.Curved;
			_currentDuration = 0f;
			if (_hitBox != null)
				_hitBox.EnableHitBox = false;

			float distance = Vector3.Distance(_projectileStartPosition, projectileFlightData.TargetPosition);
			_totalDuration = distance > 0 ? (distance/projectileFlightData.Speed) : 0f;
			_projectileMaxRelativeHeight = distance*projectileFlightData.MaxHeight;

		}


	}

	private void UpdateLinearMovement()
	{
		transform.position += _linearDirection*(_projectileFlightData.Speed*Time.deltaTime);
	}

	private void UpdateCurvedMovement()
	{
		// 1. Accumulate elapsed time and normalize it between 0.0 and 1.0
		_currentDuration += Time.deltaTime;
		float t = Mathf.Clamp01(_currentDuration/_totalDuration);

		// 2. Move the ROOT game object (Hitbox & Shadow) linearly
		transform.position = Vector3.Lerp(_projectileStartPosition, _projectileFlightData.TargetPosition, t);

		//3. Move the projectile visual vertically
		if (_projectileVisual != null)
		{
			float heightOffset = _projectileFlightData.Curve.Evaluate(t)*_projectileMaxRelativeHeight;
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
		if (_hitBox != null) _hitBox.EnableHitBox = false;
		if (_projectileVisual != null) _projectileVisual.gameObject.SetActive(false);

		// 1. Construct the Payload
		EffectPayload expirePayload = new EffectPayload(_combatContext.User)
		{
			Target = null,
			TargetPosition = transform.position,
			HitDirection = _linearDirection,
			HitImpactPoint = transform.position,
			SkillDataInstance = _combatContext.SkillDataInstance,
			SkillDamageBase = _combatContext.SkillDamageBase
		};

		// 2. Execute Effects
		if (_combatContext.EffectsList?.Count > 0)
			foreach (Effect effect in _combatContext.EffectsList)
				effect.Execute(expirePayload);

		Destroy(gameObject);
	}

	private enum MovementType { Linear, Curved }
}
