using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Executes a nested list of effects only if every attached condition is met.
/// Checks through custom EffectCondition subclass.
/// </summary>
[Serializable]
public class EffectConditional : Effect
{
	[Header("ConditionsList")]
	[SerializeReference, SubclassSelector] public List<EffectCondition> ConditionsList = new List<EffectCondition>();

	[Header("Effects")]
	[SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();

	/// <summary>
	/// Exposes nested effects so skill modifiers can reach inside the conditional.
	/// </summary>
	public override List<Effect> GetNestedEffects() => EffectsList;

	public override bool Execute(EffectPayload payload)
	{
		// 1. Fail if any condition is not met
		if (ConditionsList != null)
		{
			foreach (EffectCondition condition in ConditionsList)
			{
				if (condition == null)
					continue;
				if (!condition.Evaluate(payload))
					return false;
			}
		}

		// 2. Execute nested effects
		bool anyEffectSucceeded = false;
		if (EffectsList != null)
		{
			foreach (Effect effect in EffectsList)
			{
				if (effect != null && effect.Execute(payload))
					anyEffectSucceeded = true;
			}
		}

		return anyEffectSucceeded;
	}

	public override Effect Clone()
	{
		// 1. Create an empty list for the cloned conditions
		List<EffectCondition> clonedConditionsList = new List<EffectCondition>();

		// 2. Loop through the conditions list and add the cloned conditions to the new list
		if (ConditionsList != null)
			foreach (EffectCondition condition in ConditionsList)
				if (condition != null)
					clonedConditionsList.Add(condition.Clone());

		// 3. Create an empty list for the cloned effects
		List<Effect> clonedEffectsList = new List<Effect>();

		// 4. Loop through the effects list and add the cloned effects to the new list
		if (EffectsList != null)
			foreach (Effect effect in EffectsList)
				if (effect != null)
					clonedEffectsList.Add(effect.Clone());

		// 5. Return a new instance of EffectConditional with the cloned conditions and effects lists
		return new EffectConditional
		{
			ConditionsList = clonedConditionsList,
			EffectsList = clonedEffectsList
		};
	}
}
