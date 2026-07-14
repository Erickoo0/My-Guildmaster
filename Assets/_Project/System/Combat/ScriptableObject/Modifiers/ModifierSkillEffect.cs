using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
/// <summary>
/// Modifies the already compiled EffectsList on a SkilLDataInstance,
/// finds the first effect matching the given type.
/// </summary>
[Serializable]
public class ModifierSkillEffect : ModifierSkillBase
{
	[Header("Effect Modification")]
	[Tooltip("The ID/Name of the effect class (e.g., 'BurnEffect').")]
	[SerializeField, EffectTypeSelector] private string _effectTypeID;

	[Tooltip("The variable name inside that effect to modify (e.g., '_damageAmount').")]
	[SerializeField, EffectFieldSelector("_effectTypeID")]
	private string _effectParameter;

	[SerializeField] private StatModificationOperation _operation;
	[Tooltip("For bool fields: value >= 1 is true, < 1 is false.")]
	[SerializeField] private float _value;

	[Header("Complex Object Modifications")]
	[Tooltip("Only used if the target field is an AnimationCurve.")]
	[SerializeField] private AnimationCurve _curveValue;
	private FieldInfo _cachedTargetField = null;

	// --- CACHE VARIABLES ---
	private Type _cachedTargetType = null;
	private bool _hasAttemptedCache = false;

	public override void ApplyModifier(SkillDataInstance skillDataInstance)
	{
		// Safety Check
		if (skillDataInstance?.EffectsList == null) return;
		if (string.IsNullOrWhiteSpace(_effectTypeID) || string.IsNullOrWhiteSpace(_effectParameter)) return;

		// 1. Try to cache the Reflection data if we haven't already
		if (!_hasAttemptedCache || _cachedTargetType == null || _cachedTargetField == null)
			InitializeReflectionCache();

		// If the cache failed to find the type or field, return
		if (_cachedTargetType == null || _cachedTargetField == null)
		{
			Debug.LogWarning($"ModifierSkillEffect: Failed to cache Reflection data for '{_effectTypeID}'.");
			return;
		}

		// 2. Find the actual EffectsList across SkillDataInstance and its nested effects
		// Searches the SkillDataInstance level, then the nested effects.
		Effect targetEffect = FindEffectRecurive(skillDataInstance.EffectsList, _cachedTargetType);
		if (targetEffect == null)
		{
			Debug.LogWarning($"ModifierSkillEffect: Skill does not have an effect of type '{_effectTypeID}'.");
			return;
		}

		// 3. Get the current value and inject the modified one based on field type
		object currentValue = _cachedTargetField.GetValue(targetEffect);

		// 4. Modify the value based on the operation and field type
		if (_cachedTargetField.FieldType == typeof(float))
		{
			float current = (float)currentValue;
			float newValue = _operation switch
			{
				StatModificationOperation.Set => _value,
				StatModificationOperation.Add => current + _value,
				StatModificationOperation.Multiply => current*_value,
				_ => current
			};
			_cachedTargetField.SetValue(targetEffect, newValue);
		} else if (_cachedTargetField.FieldType == typeof(int))
		{
			int current = (int)currentValue;
			int newValue = _operation switch
			{
				StatModificationOperation.Set => Mathf.RoundToInt(_value),
				StatModificationOperation.Add => current + Mathf.RoundToInt(_value),
				StatModificationOperation.Multiply => Mathf.RoundToInt(current*_value),
				_ => current
			};
			_cachedTargetField.SetValue(targetEffect, newValue);
		} else if (_cachedTargetField.FieldType == typeof(bool))
		{
			// Only Set is meaningful for bools
			bool newValue = _operation == StatModificationOperation.Set
				? _value >= 1f
				: (bool)currentValue;
			_cachedTargetField.SetValue(targetEffect, newValue);
		} else if (_cachedTargetField.FieldType.IsEnum)
		{
			// Cast the float value to the Enum's underlying integer value
			int intValue = Mathf.RoundToInt(_value);
			_cachedTargetField.SetValue(targetEffect, Enum.ToObject(_cachedTargetField.FieldType, intValue));
		} else if (_cachedTargetField.FieldType == typeof(AnimationCurve))
		{
			if (_operation == StatModificationOperation.Set && _curveValue != null)
			{
				// Assign a new instance of the curve so we don't accidentally link reference data
				_cachedTargetField.SetValue(targetEffect, new AnimationCurve(_curveValue.keys));
			}
		}
	}

	/// <summary>
	/// Performs the heavy Reflection work once and saves the results.
	/// </summary>
	private void InitializeReflectionCache()
	{
		_hasAttemptedCache = true;

		// Resolve the Type (Class blueprint)
		_cachedTargetType = ResolveType(_effectTypeID);
		if (_cachedTargetType == null)
		{
			Debug.LogError($"ModifierSkillEffect: Cannot find class named '{_effectTypeID}'. Make sure spelling is exact.");
			return;
		}

		// Resolve the Field (Variable blueprint)
		_cachedTargetField = FindField(_cachedTargetType, _effectParameter);
		if (_cachedTargetField == null)
		{
			Debug.LogError($"ModifierSkillEffect: Class '{_effectTypeID}' does not have a variable named '{_effectParameter}'.");
			return;
		}

// Ensure the variable is one of the supported types
		if (_cachedTargetField.FieldType != typeof(float) &&
			_cachedTargetField.FieldType != typeof(int) &&
			_cachedTargetField.FieldType != typeof(bool) &&
			!_cachedTargetField.FieldType.IsEnum &&
			_cachedTargetField.FieldType != typeof(AnimationCurve))
		{
			Debug.LogError($"ModifierSkillEffect: Variable '{_effectParameter}' is not a supported type (float, int, bool, Enum, AnimationCurve).");
			_cachedTargetField = null;
		}
	}

	//---- Helper Methods ----

	private static Effect FindEffectRecurive(List<Effect> effectsList, Type targetType)
	{
		if (effectsList == null)
			return null;

		foreach (Effect effect in effectsList)
		{
			if (effect == null) continue;

			// 1. If match is found at the SkillDataInstance level, return it
			if (effect.GetType() == targetType)
				return effect;

			// 2. If match is found at the nested effects level, return it
			Effect nestedEffect = FindEffectRecurive(effect.GetNestedEffects(), targetType);
			if (nestedEffect != null)
				return nestedEffect;

		}

		return null;
	}

	private static Type ResolveType(string typeName)
	{
		// Look in the current executing assembly first (where your game code lives)
		Type t = Assembly.GetExecutingAssembly().GetType(typeName);
		if (t != null) return t;

		// Fallback only if absolutely necessary
		return Type.GetType(typeName);
	}

	private static FieldInfo FindField(Type type, string fieldName)
	{
		// BindingFlags are the "search filters" for Reflection.
		// We tell it to look for both public and private variables that belong to an instance.
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		// This loop checks the class, and if it doesn't find the variable, 
		// it checks the class it inherited from (BaseType), all the way up.
		while (type != null && type != typeof(object))
		{
			FieldInfo field = type.GetField(fieldName, flags);
			if (field != null) return field;

			type = type.BaseType;
		}
		return null;
	}
}
