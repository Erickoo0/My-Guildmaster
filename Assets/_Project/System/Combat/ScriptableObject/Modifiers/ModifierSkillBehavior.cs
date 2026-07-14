using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public enum BehaviorOperation
{
	Append,
	Prepend,
	InsertAtIndex,
	ReplaceAll,
	Clear
}

/// <summary>
/// Modifies the top-level EffectsList of a SkillDataInstance, or targets a nested list.
/// Used to append, prepend, insert, replace, or clear effects.
/// </summary>
[Serializable]
public class ModifierSkillBehavior : ModifierSkillBase
{
	[Header("Effect Data")]
	[SerializeField] private BehaviorOperation _operation;
	[SerializeReference, SubclassSelector] private List<Effect> _effectsList = new List<Effect>();
	[SerializeField] private int _insertIndex;

	[Header("Stack Behavior")]
	[SerializeField] private bool _stackOnExisting = false;
	[SerializeField, EffectFieldSelector("_effectTypeFromList")]
	private string _stackParameter;
	[SerializeField] private float _stackValue;

	[Header("Targeting")]
	[Tooltip("If true, targets a nested effect list (like On-Hit effects) instead of the root skill cast.")]
	[SerializeField] private bool _targetNestedEffect = false;
	[SerializeField, EffectTypeSelector] private string _targetEffectTypeID;

	// Reflection cache
	[NonSerialized] private FieldInfo _cachedStackField = null;
	[NonSerialized] private bool _hasAttemptedStackCache = false;

	public override void ApplyModifier(SkillDataInstance skillDataInstance)
	{
		// Safety Checks
		if (skillDataInstance?.EffectsList == null)
		{
			Debug.LogWarning("ModifierSkillBehavior: Cannot apply modifier. SkillDataInstance or its EffectsList is null.");
			return;
		}

		// Default target is the root EffectsList
		List<Effect> targetList = skillDataInstance.EffectsList;

		// 1. If targeting a nested effect, find it and swap our target list pointer
		if (_targetNestedEffect && !string.IsNullOrWhiteSpace(_targetEffectTypeID))
		{
			Type parentType = ResolveType(_targetEffectTypeID);
			if (parentType != null)
			{
				Effect parentEffect = FindEffectRecursive(skillDataInstance.EffectsList, parentType);
				if (parentEffect != null && parentEffect.GetNestedEffects() != null)
				{
					targetList = parentEffect.GetNestedEffects();
				} else
				{
					Debug.LogWarning($"ModifierSkillBehavior: Could not find nested list for {_targetEffectTypeID}");
					return; // Abort if we couldn't find the target node
				}
			}
		}

		// 2. Check if the target effect already exists in the *targeted* list
		bool effectAlreadyExists = false;
		Type targetType = null;

		if (_stackOnExisting && _effectsList != null && _effectsList.Count > 0)
		{
			targetType = _effectsList[0].GetType();
			foreach (Effect effect in targetList)
			{
				if (effect != null && effect.GetType() == targetType)
				{
					effectAlreadyExists = true;
					break;
				}
			}
		}

		// 3. Route to the correct logic
		if (effectAlreadyExists)
			StackExistingEffect(targetList);
		else
			ApplyOperation(targetList);
	}

	private void ApplyOperation(List<Effect> targetList)
	{
		switch (_operation)
		{
		case BehaviorOperation.Append:
			targetList.AddRange(ClonedEffects());
			break;
		case BehaviorOperation.Prepend:
			targetList.InsertRange(0, ClonedEffects());
			break;
		case BehaviorOperation.InsertAtIndex:
			int index = Mathf.Clamp(_insertIndex, 0, targetList.Count);
			targetList.InsertRange(index, ClonedEffects());
			break;
		case BehaviorOperation.ReplaceAll:
			targetList.Clear();
			targetList.AddRange(ClonedEffects());
			break;
		case BehaviorOperation.Clear:
			targetList.Clear();
			break;
		default:
			Debug.LogWarning($"ModifierSkillBehavior: Unhandled Operation: {_operation}");
			break;
		}
	}

	private void StackExistingEffect(List<Effect> targetList)
	{
		// Safety Check
		if (string.IsNullOrWhiteSpace(_stackParameter) || _effectsList == null || _effectsList.Count == 0) return;

		Type targetType = _effectsList[0].GetType();
		if (targetType == null) return;

		Effect existingEffect = null;
		foreach (Effect effect in targetList)
		{
			if (effect != null && effect.GetType() == targetType)
			{
				existingEffect = effect;
				break;
			}
		}

		if (existingEffect == null)
		{
			Debug.LogWarning($"ModifierSkillBehavior: Stack mode could not find existing '{targetType.Name}'.");
			return;
		}

		if (!_hasAttemptedStackCache)
		{
			_hasAttemptedStackCache = true;
			_cachedStackField = FindField(targetType, _stackParameter);

			if (_cachedStackField == null)
			{
				Debug.LogWarning($"ModifierSkillBehavior: Stack mode could not find field '{_stackParameter}'.");
				return;
			} else if (_cachedStackField.FieldType != typeof(float))
			{
				Debug.LogWarning($"ModifierSkillBehavior: Stack field is not of type float.");
				_cachedStackField = null;
			}
		}

		if (_cachedStackField == null) return;

		float currentValue = (float)_cachedStackField.GetValue(existingEffect);
		_cachedStackField.SetValue(existingEffect, currentValue + _stackValue);
	}

	private List<Effect> ClonedEffects()
	{
		List<Effect> effectsListCloned = new List<Effect>();
		if (_effectsList == null) return effectsListCloned;

		foreach (Effect effect in _effectsList)
		{
			if (effect != null)
				effectsListCloned.Add(effect.Clone());
		}
		return effectsListCloned;
	}

	private static FieldInfo FindField(Type targetType, string parameterName)
	{
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		while (targetType != null && targetType != typeof(object))
		{
			FieldInfo f = targetType.GetField(parameterName, flags);
			if (f != null) return f;
			targetType = targetType.BaseType;
		}
		return null;
	}

	// ---- NEW HELPER METHODS ----
	private static Type ResolveType(string typeName)
	{
		Type t = Assembly.GetExecutingAssembly().GetType(typeName);
		if (t != null) return t;
		return Type.GetType(typeName);
	}

	private static Effect FindEffectRecursive(List<Effect> effectsList, Type targetType)
	{
		if (effectsList == null) return null;

		foreach (Effect effect in effectsList)
		{
			if (effect == null) continue;

			if (effect.GetType() == targetType) return effect;

			Effect nestedEffect = FindEffectRecursive(effect.GetNestedEffects(), targetType);
			if (nestedEffect != null) return nestedEffect;
		}
		return null;
	}
}
