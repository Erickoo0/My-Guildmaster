using System;
using UnityEngine;
using Object = UnityEngine.Object;
public enum AilmentType
{
	Burn, Freeze, Chill, Shock, Slow
}

[Serializable]
public class EffectApplyAilment : Effect
{
	[field: SerializeField] public GameObject Prefab { get; private set; }
	[field: SerializeField] public AilmentType Type { get; private set; }

	[Tooltip("For Burn: Damage per tick. For Chill/Shock/Slow: The multiplier. For Freeze: Unused.")]
	[field: SerializeField] public float Potency { get; private set; }
	[field: SerializeField] public float PotencyStack { get; private set; } = 0f;
	[field: SerializeField] public float Duration { get; private set; }

	public override bool Execute(EffectPayload effectPayload)
	{
		Debug.Log("Applying Ailment");
		// 1. Get ailment target
		GameObject target = effectPayload.Target != null ? effectPayload.Target : effectPayload.User;

		// 2. Check for existing ailments of the same type
		Ailment[] activeAilmentsList = target.GetComponentsInChildren<Ailment>();
		foreach (Ailment ailment in activeAilmentsList)
		{
			if (ailment.Type == Type)
			{
				if (PotencyStack > 0f)
					ailment.StackPotency(PotencyStack, Duration);
				else
					ailment.RefreshAilment(Potency, Duration);

				return true;
			}
		}

		// 3. Create new ailment prefab and set it up
		GameObject ailmentInstance = Object.Instantiate(Prefab, target.transform);
		if (ailmentInstance.TryGetComponent(out Ailment ailmentComponent))
		{
			ailmentComponent.Setup(Type, Potency, Duration, target, effectPayload);
			return true;
		}

		// 4. Return false if ailment prefab does not have Ailment component
		Debug.LogError($"EffectApplyAilment: Ailment prefab does not have Ailment component");
		return false;
	}

	public override Effect Clone()
	{
		return new EffectApplyAilment
		{
			Prefab = Prefab,
			Type = Type,
			Potency = Potency,
			Duration = Duration,
			PotencyStack = PotencyStack
		};
	}
}
