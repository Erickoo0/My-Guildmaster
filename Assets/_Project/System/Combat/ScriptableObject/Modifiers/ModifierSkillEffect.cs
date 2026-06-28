using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// Modifies the already compiled EffectsList on a SkilLDataInstance,
/// finds the first effect matching the given type.
/// </summary>
[System.Serializable]
public class ModifierSkillEffect : ModifierSkillBase
{
    [Header("Effect Modification")]
    [Tooltip("The ID/Name of the effect class (e.g., 'BurnEffect').")]
    [SerializeField] private string _effectTypeID;
    
    [Tooltip("The variable name inside that effect to modify (e.g., '_damageAmount').")]
    [SerializeField] private string _effectParameter;
    
    [SerializeField] private StatModificationOperation _operation;
    [SerializeField] private float _value;

    // --- CACHE VARIABLES ---
    // We save these here so we don't have to do the slow Reflection search every time.
    private Type _cachedTargetType = null;
    private FieldInfo _cachedTargetField = null;
    private bool _hasAttemptedCache = false;

    public override void ApplyModifier(SkillDataInstance skillDataInstance)
    {
        // Standard safety checks
        if (skillDataInstance?.EffectsList == null) return;
        if (string.IsNullOrWhiteSpace(_effectTypeID) || string.IsNullOrWhiteSpace(_effectParameter)) return;
        
        // 1. Try to cache the Reflection data if we haven't already
        if (!_hasAttemptedCache)
        {
            InitializeReflectionCache();
        }

        // If the cache failed to find the type or field, we can't proceed.
        if (_cachedTargetType == null || _cachedTargetField == null) return;
        
        // 2. Find the actual effect object sitting inside the SkillDataInstance
        Effect targetEffect = FindFirstEffectOfType(skillDataInstance, _cachedTargetType);
        if (targetEffect == null)
        {
            Debug.LogWarning($"ModifierSkillEffect: Skill does not have an effect of type '{_effectTypeID}'.");
            return;
        }
        
        // 3. Get the current value from the target object
        // GetValue() requires an 'object' to pull the data from. We give it our targetEffect.
        float currentValue = (float)_cachedTargetField.GetValue(targetEffect);
        
        // 4. Calculate the new value
        float newValue = _operation switch
        {
            StatModificationOperation.Set => _value,
            StatModificationOperation.Add => currentValue + _value,
            StatModificationOperation.Multiply => currentValue * _value,
            _ => currentValue
        };
        
        // 5. Inject the new value back into the target object
        _cachedTargetField.SetValue(targetEffect, newValue);
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

        // Ensure the variable is actually a float, otherwise our math will crash.
        if (_cachedTargetField.FieldType != typeof(float))
        {
            Debug.LogError($"ModifierSkillEffect: Variable '{_effectParameter}' is not a float. Only floats are supported.");
            _cachedTargetField = null; // Clear it so we don't try to use it
        }
    }

    //---- Helper Methods ----

    private static Effect FindFirstEffectOfType(SkillDataInstance skillDataInstance, Type targetType)
    {
        foreach (Effect effect in skillDataInstance.EffectsList)
        {
            // GetType() gets the runtime type of the effect instance to compare against our blueprint
            if (effect != null && effect.GetType() == targetType)
                return effect;
        }
        return null;
    }

    private static Type ResolveType(string typeName)
    {
        // AppDomain.CurrentDomain.GetAssemblies() gets every block of compiled code in Unity.
        // This is a very slow operation, which is why caching is so important.
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = assembly.GetType(typeName);
            if (t != null) return t;
        }
        return null;
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
