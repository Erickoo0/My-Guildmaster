using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public abstract class Effect
{
	public abstract bool Execute(EffectPayload payload);

	/// <summary>
	/// Clone the effects so that modifiers can mutate or extend the effects
	/// within the SkillDataInstance without affecting the original
	/// </summary>
	public abstract Effect Clone();

	/// <summary>
	/// Returns the nested EffectsList from this effect (e.g EffectSpawnProjectile)
	/// Used by ModifierSkillEffect to modify nested effects.
	/// </summary>
	public virtual List<Effect> GetNestedEffects() => null;

	public static IEnumerator RunEffectSequence(int count, float delay, Action<int> effectAction)
	{
		for (int i = 0; i < count; i++)
		{
			effectAction.Invoke(i);

			if (i < count - 1)
				yield return new WaitForSeconds(delay);
		}
	}
}
