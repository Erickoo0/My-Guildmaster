using UnityEngine;
/// <summary>
/// Checks the MP of the User or Target against a flat value or percentage
/// </summary>
public class EffectConditionMp : EffectCondition
{
	public enum MpContext { User, Target }

	public enum MpValueMode { Flat, Percentage }

	[field: SerializeField] public MpContext Context { get; private set; } = MpContext.Target;
	[field: SerializeField] public MpValueMode ValueMode { get; private set; } = MpValueMode.Percentage;
	[field: SerializeField] public ComparisonOperation Operation { get; private set; } = ComparisonOperation.LessThan;
	[field: SerializeField] public float Value { get; private set; } = 0.5f;

	public override bool Evaluate(EffectPayload payload)
	{
		// 1. Set the subject to either the target or user based on the context
		GameObject subject = Context == MpContext.Target ? payload.Target : payload.User;
		if (subject == null)
			return false;

		// 2. Get the stat components from the subject
		if (!subject.TryGetComponent(out IStatProvider statProvider))
			return false;

		// 3. Compare the values against the Operation
		float currentValue = ValueMode == MpValueMode.Flat
			? statProvider.EntityMana.MpCurrent
			: statProvider.EntityMana.MpCurrent/statProvider.EntityMana.MpMax;

		return Compare(currentValue, Value, Operation);
	}

	public override EffectCondition Clone()
	{
		return new EffectConditionMp
		{
			Context = Context,
			ValueMode = ValueMode,
			Operation = Operation,
			Value = Value
		};
	}
}
