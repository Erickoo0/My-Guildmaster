using System;
using UnityEngine;
/// <summary>
/// The Operation to perform when comparing a current value to a threshold
/// </summary>
public enum ComparisonOperation
{
	LessThan,
	LessThanOrEqual,
	Equal,
	NotEqual,
	GreaterThanOrEqual,
	GreaterThan
}

/// <summary>
/// Which GameObject in the EffectPayload provides the context for the requirement condition
/// </summary>
public enum EffectConditionContext
{
	User,
	Target
}

[Serializable]
public abstract class EffectCondition
{
	public abstract bool Evaluate(EffectPayload payload);

	/// <summary>
	/// Clone the condition so nested condition data is not mutated at runtime
	/// </summary>
	public abstract EffectCondition Clone();

	/// <summary>
	/// Compare a current value to a threshold using the specified Operation
	/// </summary>
	protected bool Compare(float current, float threshold, ComparisonOperation operation)
	{
		return operation switch
		{
			ComparisonOperation.LessThan => current < threshold,
			ComparisonOperation.LessThanOrEqual => current <= threshold,
			ComparisonOperation.Equal => Mathf.Approximately(current, threshold),
			ComparisonOperation.GreaterThanOrEqual => current >= threshold,
			ComparisonOperation.GreaterThan => current > threshold,
			_ => false
		};
	}
}

/// <summary>
/// Optional bridge: reuses existing Requirement subclasses as effect conditions.
/// Limitation: Requirements only see one GameObject, so they cannot compare User vs Target.
/// </summary>
public class EffectConditionRequirement : EffectCondition
{

	[SerializeReference, SubclassSelector] public Requirement Requirement;
	[field: SerializeField] public EffectConditionContext Context { get; private set; } = EffectConditionContext.User;

	/// <summary>
	/// Evaluate the requirement condition using the specified context from the payload
	/// </summary>
	public override bool Evaluate(EffectPayload payload)
	{
		GameObject context = Context == EffectConditionContext.User ? payload.User : payload.Target;
		if (context == null || Requirement == null)
			return false;

		return Requirement.IsMet(context);
	}

	public override EffectCondition Clone()
	{
		return new EffectConditionRequirement
		{
			Context = Context,
			Requirement = Requirement
		};
	}
}
