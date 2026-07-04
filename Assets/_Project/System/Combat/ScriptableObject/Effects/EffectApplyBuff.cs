using System;
using UnityEngine;
using Object = UnityEngine.Object;

public enum BuffType { Health, Mana, Damage, Shield, MoveSpeed }

[Serializable]
public class EffectApplyBuff : Effect
{
	[field: SerializeField] public GameObject Prefab { get; private set; }
	[field: SerializeField] public BuffType Type { get; private set; }
	[field: SerializeField] public float Amount { get; private set; }
	[field: SerializeField] public float Duration { get; private set; }

	public override bool Execute(EffectPayload payload)
	{
		// 1. Get buff target
		GameObject buffTarget = payload.Target != null ? payload.Target : payload.User;

		// 2. Check if buff target already has existing buff of same type
		Buff[] activeBuffs = buffTarget.GetComponentsInChildren<Buff>();
		foreach (Buff buff in activeBuffs)
		{
			if (buff.Type == Type)
			{
				buff.Refresh(Amount, Duration);
				return true;
			}
		}

		// 3. Create the buff prefab and Set it up
		GameObject buffInstance = Object.Instantiate(Prefab, buffTarget.transform);
		if (buffInstance.TryGetComponent(out Buff buffComponent))
		{
			buffComponent.Setup(buffTarget, Type, Amount, Duration);
			return true;
		}

		Debug.Log("Buff prefab does not have Buff component!");
		return false;
	}

	public override Effect Clone()
	{
		return new EffectApplyBuff
		{
			Prefab = Prefab,
			Type = Type,
			Amount = Amount,
			Duration = Duration
		};
	}
}
