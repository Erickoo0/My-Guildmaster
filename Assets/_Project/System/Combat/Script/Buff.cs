using System.Reflection;
using UnityEngine;
public class Buff : MonoBehaviour
{

	[Header("Buff Settings")]
	private readonly float _tickRate = 1f;
	private float _amount;
	private float _amountPerTick;
	private float _duration;
	private float _durationTimer;
	private FieldInfo _fieldInfo;

	// Reflection target — cached once in HandleBuffType for EntityStats-based buffs
	private object _fieldOwner;
	private Health _health;

	[Header("Behavior Settings")]
	private bool _isInstant;
	private Mana _mana;
	[Header("References")]
	private GameObject _receiver;
	private float _tickTimer;
	public BuffType Type { get; private set; }

	private void Update()
	{
		// Handle duration
		_durationTimer -= Time.deltaTime;
		if (_durationTimer <= 0)
		{
			Destroy(gameObject);
			return;
		}

		// Per-tick buffs
		if (!_isInstant)
		{
			_tickTimer += Time.deltaTime;
			if (_tickTimer >= _tickRate)
			{
				ApplyTick();
				_tickTimer = 0;
			}
		}
	}

	private void OnDestroy() => RevertInstant();

	public void Setup(GameObject receiver, BuffType type, float amount, float duration)
	{
		_receiver = receiver;
		Type = type;
		_duration = duration;
		_amount = amount;
		_durationTimer = duration;

		// Calculate total ticks and Amount per tick
		int totalTicks = Mathf.CeilToInt(_duration/_tickRate);
		_amountPerTick = _amount/totalTicks;

		HandleBuffType();

		if (_isInstant)
			ApplyInstant();
	}

	/// <summary>
	/// Refreshes the buff with new duration, and overwrites old amount if new amount is more potent
	/// </summary>
	public void Refresh(float newAmount, float newDuration)
	{
		// 1. Refresh the duration
		_durationTimer = newDuration;

		// 2. Overwrite old amount if new amount is more potent
		if (newAmount > _amount)
		{
			if (_isInstant)
			{
				RevertInstant();     // Take away the old stats
				_amount = newAmount; // Apply the new amount
				ApplyInstant();      // Apply the new stats
			} else
			{
				_amount = newAmount;

				// 3. Recalculate ticks
				int totalTicks = Mathf.CeilToInt(_durationTimer/_tickRate);
				_amountPerTick = _amount/totalTicks;
			}
		}
	}

	private void HandleBuffType()
	{
		// 1. Get stats from the IStatProvider interface (if it exists)
		if (_receiver.TryGetComponent(out IStatProvider statProvider))
		{
			switch (Type)
			{
			case BuffType.Health:
				_health = statProvider.EntityHealth;
				break;
			case BuffType.Mana:
				_mana = statProvider.EntityMana;
				break;
			case BuffType.Damage:
				ResolveStatField(statProvider.EntityStats, nameof(EntityStats.DamageMultiplier));
				break;
			case BuffType.Shield:
				_health = statProvider.EntityHealth;
				break;
			case BuffType.MoveSpeed:
				ResolveStatField(statProvider.EntityStats, nameof(EntityStats.MoveSpeedMultiplier));
				break;
			}
		} else // 2. If there is no IStatProvider, get the components from the entity directly
		{
			switch (Type)
			{
			case BuffType.Health:
				_health = _receiver.GetComponent<Health>();
				break;
			case BuffType.Mana:
				_mana = _receiver.GetComponent<Mana>();
				break;
			case BuffType.Damage:
				ResolveStatField(_receiver.GetComponent<EntityStats>(), nameof(EntityStats.DamageMultiplier));
				break;
			case BuffType.Shield:
				_health = _receiver.GetComponent<Health>();
				_isInstant = true;
				break;
			case BuffType.MoveSpeed:
				ResolveStatField(_receiver.GetComponent<EntityStats>(), nameof(EntityStats.MoveSpeedMultiplier));
				break;
			}
		}

		// Safety Warnings
		if (Type == BuffType.Health && _health == null)
			Debug.LogWarning($"Buff tried to heal, but {_receiver.name} has no Health reference.");
		if (Type == BuffType.Mana && _mana == null)
			Debug.LogWarning($"Buff tried to restore MP, but {_receiver.name} has no Mana reference.");
		if (_isInstant && Type != BuffType.Shield && _fieldOwner == null)
			Debug.LogWarning($"Buff '{Type}' could not find EntityStats on {_receiver.name}.");
	}

	/// <summary>
	/// Caches the component instance and its FieldInfo for reflection stat modification
	/// </summary>
	private void ResolveStatField(EntityStats entityStats, string fieldName)
	{
		// Buffs which call this method are always instant effects
		_isInstant = true;

		if (entityStats == null)
			return;

		_fieldOwner = entityStats;
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		_fieldInfo = entityStats.GetType().GetField(fieldName, flags);

		if (_fieldInfo == null || _fieldInfo.FieldType != typeof(float))
		{
			Debug.LogWarning($"Buff could not resolve float field '{fieldName}' on EntityStats.");
			_fieldInfo = null;
		}
	}

	private void ApplyTick()
	{
		switch (Type)
		{
		case BuffType.Health:
			if (_health != null) _health.HpHealInstant(_amountPerTick);
			break;
		case BuffType.Mana:
			if (_mana != null) _mana.MpHealInstant(_amountPerTick);
			break;
		}
	}

	private void ApplyInstant()
	{
		switch (Type)
		{
		case BuffType.Shield:
			if (_health != null) _health.HpMax += _amount;
			break;
		case BuffType.MoveSpeed:
		case BuffType.Damage:
			if (_fieldInfo != null)
				_fieldInfo.SetValue(_fieldOwner, (float)_fieldInfo.GetValue(_fieldOwner)*_amount);
			break;
		}
	}

	private void RevertInstant()
	{
		switch (Type)
		{
		case BuffType.Shield:
			if (_health != null)
			{
				// Take away the shield
				_health.HpMax -= _amount;
				// Clamp CurrentHp to new max (unless currentHp is less than the new max)
				_health.HpCurrent = Mathf.Min(_health.HpCurrent, _health.HpMax);
			}
			break;
		case BuffType.MoveSpeed:
		case BuffType.Damage:
			if (_fieldInfo != null)
				_fieldInfo.SetValue(_fieldOwner, (float)_fieldInfo.GetValue(_fieldOwner)/_amount);
			break;
		}
	}
}
