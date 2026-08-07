using UnityEngine;
public class Ailment : MonoBehaviour
{

	[Header("Tick Settings")]
	private readonly float _tickRate = 1f;
	private ControllerBase _controller;
	private float _duration;
	private EffectPayload _effectPayload; // For damage ailments

	[Header("References")]
	private IStatProvider _statProvider;
	private GameObject _target;
	private float _tickTimer;
	public AilmentType Type { get; private set; }
	public float Potency { get; private set; }

	private void Update()
	{
		_duration -= Time.deltaTime;
		if (_duration <= 0f)
		{
			Destroy(gameObject);
			return;
		}

		if (Type == AilmentType.Burn)
			ProcessBurnTick();
	}

	private void OnDestroy() => RevertInstantEffects();

	public void Setup(AilmentType type, float potency, float duration, GameObject target, EffectPayload effectPayload)
	{
		Type = type;
		Potency = potency;
		_duration = duration;
		_target = target;
		_effectPayload = effectPayload;
		_statProvider = _target?.GetComponent<IStatProvider>();
		_controller = _target?.GetComponent<ControllerBase>();

		ApplyInstantEffects();
	}

	public void RefreshAilment(float potency, float duration)
	{
		_duration = duration;

		// Overwrite potency if new potency is higher
		if (potency > Potency)
		{
			RevertInstantEffects();
			Potency = potency;
			ApplyInstantEffects();
		}
	}

	public void StackPotency(float addedPotency, float duration)
	{
		_duration += (duration*0.25f);

		RevertInstantEffects();
		Potency += addedPotency;
		ApplyInstantEffects();
	}

	private void ApplyInstantEffects()
	{
		switch (Type)
		{
		case AilmentType.Freeze:
			// Implement later
			break;
		case AilmentType.Chill:
			if (_statProvider != null)
				_statProvider.EntityStats.MoveSpeedMultiplier *= Potency;
			break;
		case AilmentType.Shock:
			if (_statProvider != null)
				_statProvider.EntityStats.DamageTakenMultiplier *= Potency;
			break;
		case AilmentType.Slow:
			if (_statProvider != null)
				_statProvider.EntityStats.MoveSpeedMultiplier *= Potency;
			break;
		}
	}

	private void RevertInstantEffects()
	{
		switch (Type)
		{
		case AilmentType.Freeze:
			// Implement later
			break;
		case AilmentType.Chill:
			if (_statProvider != null)
				_statProvider.EntityStats.MoveSpeedMultiplier /= Potency;
			break;
		case AilmentType.Shock:
			if (_statProvider != null)
				_statProvider.EntityStats.DamageTakenMultiplier /= Potency;
			break;
		case AilmentType.Slow:
			if (_statProvider != null)
				_statProvider.EntityStats.MoveSpeedMultiplier /= Potency;
			break;
		}
	}

	private void ProcessBurnTick()
	{
		_tickTimer -= Time.deltaTime;
		if (_tickTimer <= 0)
		{
			_tickTimer = _tickRate;

			if (_statProvider != null)
				_statProvider.EntityHealth.HpCurrent -= Potency;
		}
	}
}
