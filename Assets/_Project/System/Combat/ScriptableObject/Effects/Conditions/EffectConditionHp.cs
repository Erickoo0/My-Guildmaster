using UnityEngine;
/// <summary>
/// Checks the HP of the User or Target against a flat value or percentage
/// </summary>
public class EffectConditionHp : EffectCondition
{
	public enum HpContext { User, Target }

	public enum HpValueMode { Flat, Percentage }

	[field: SerializeField] public HpContext Context { get; private set; } = HpContext.Target;
	[field: SerializeField] public HpValueMode ValueMode { get; private set; } = HpValueMode.Percentage;
	[field: SerializeField] public ComparisonOperation Operation { get; private set; } = ComparisonOperation.LessThan;
	[field: SerializeField] public float Value { get; private set; } = 0.5f;

	public override bool Evaluate(EffectPayload payload)
	{
		// 1. Set the subject to either the target or user based on the context
		GameObject subject = Context == HpContext.Target ? payload.Target : payload.User;
		if (subject == null)
			return false;

		// 2. Get the stat components from the subject
		if (!subject.TryGetComponent(out IStatProvider statProvider))
			return false;

		// 3. Compare the values against the Operation
		float currentValue = ValueMode == HpValueMode.Flat
			? statProvider.EntityHealth.HpCurrent
			: statProvider.EntityHealth.HpCurrent/statProvider.EntityHealth.HpMax;

		return Compare(currentValue, Value, Operation);
	}

	public override EffectCondition Clone()
	{
		return new EffectConditionHp
		{
			Context = Context,
			ValueMode = ValueMode,
			Operation = Operation,
			Value = Value
		};
	}
}
