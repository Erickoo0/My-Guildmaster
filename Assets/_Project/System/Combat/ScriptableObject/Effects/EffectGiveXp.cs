using System;
using UnityEngine;
[Serializable]
public class EffectGiveXp : Effect
{
	[field: SerializeField] public int Amount { get; private set; }

	public override bool Execute(EffectPayload payload)
	{
		GameObject target = payload.Target != null ? payload.Target : payload.User;

		if (target.TryGetComponent(out IStatProvider statProvider))
		{
			Level level = statProvider.EntityLevel;
			if (level == null)
				return false;

			level.AddExperience(Amount);
			return true;
		}

		return false;
	}

	public override Effect Clone()
	{
		return new EffectGiveXp
		{
			Amount = Amount
		};
	}
}
