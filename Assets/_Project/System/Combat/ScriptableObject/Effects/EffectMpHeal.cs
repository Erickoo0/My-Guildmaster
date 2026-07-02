using System;
using UnityEngine;
[Serializable]
public class EffectMpHeal : Effect
{
	[field: SerializeField] public float MpHealBase { get; private set; }

	public override bool Execute(EffectPayload payload)
	{
		GameObject healTarget = payload.Target != null ? payload.Target : payload.User;

		if (healTarget.TryGetComponent(out IStatProvider statProvider))
		{
			// Safety Check
			Mana mana = statProvider.EntityMana != null ? statProvider.EntityMana : null;
			if (mana == null) return false;

			// Calculate final value
			float totalHeal = MpHealBase;

			if (totalHeal > 0)
			{
				if (mana.MpCurrent >= mana.MpMax) return false;
				mana.MpHealInstant(totalHeal);
				return true;
			}

			// Damage Logic
			if (totalHeal < 0)
			{
				if (mana.MpCurrent <= 0) return false;
				mana.MpHealInstant(totalHeal);
				return true;
			}
			return true;
		}

		return false;
	}

	public override Effect Clone()
	{
		return new EffectMpHeal
		{
			MpHealBase = MpHealBase
		};
	}
}
