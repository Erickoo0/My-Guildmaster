using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Modifies the top-level EffectsList of a SkillDataInstance.
/// Used to append, prepend, insert, replace, or clear effects.
/// Optionally, also uses ModifierSkillEffect to modify existing effects.
/// </summary>
[System.Serializable]
public class ModifierSkillBehavior : ModifierSkillBase
{
    [Header("Effect Data")]
    [SerializeField] private BehaviorOperation _operation;
    [SerializeReference, SubclassSelector] private List<Effect> _effectsList = new List<Effect>();
    [SerializeField] private int _insertIndex;

    [Header("Stack Behavior")]
    [SerializeField] private bool _stackOnExisting = false;
    [SerializeField] private string _stackParameter;
    [SerializeField] private float _stackValue;
    
    // Tracks how many times this modifier has been applied during a single compile pass.
    // Reset to 0 before each compile by SkillTreeCompiler.
    //[NonSerialized] private int _applyCount = 0;
    
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

        // 1. Check if the target effect already exists in the current compilation instance
        bool effectAlreadyExists = false;
        Type targetType = null;
        
        if (_stackOnExisting && _effectsList != null && _effectsList.Count > 0)
        {
            targetType = _effectsList[0].GetType();
            foreach (Effect effect in skillDataInstance.EffectsList)
            {
                if (effect != null && effect.GetType() == targetType)
                {
                    effectAlreadyExists = true;
                    break;
                }
            }
        }

        // 2. Route to the correct logic
        if (effectAlreadyExists)
            StackExistingEffect(skillDataInstance);
        else
            ApplyOperation(skillDataInstance);
        
        Debug.Log($"[Modifier Trace] Target Type: {targetType?.Name}. Stack Checkbox: {_stackOnExisting}. Effect Already Exists? {effectAlreadyExists}. Current Top-Level Effects: {skillDataInstance.EffectsList.Count}");

        if (effectAlreadyExists)
        {
            Debug.Log("[Modifier Trace] Routing to STACK.");
        }
        else
        {
            Debug.Log("[Modifier Trace] Routing to APPEND.");
        }
    }
    
    private void ApplyOperation(SkillDataInstance skillDataInstance)
    {
        switch (_operation)
        {
        case BehaviorOperation.Append:
            skillDataInstance.EffectsList.AddRange(ClonedEffects());
            break;

        case BehaviorOperation.Prepend:
            skillDataInstance.EffectsList.InsertRange(0, ClonedEffects());
            break;

        case BehaviorOperation.InsertAtIndex:
            int index = Mathf.Clamp(_insertIndex, 0, skillDataInstance.EffectsList.Count);
            skillDataInstance.EffectsList.InsertRange(index, ClonedEffects());
            break;

        case BehaviorOperation.ReplaceAll:
            skillDataInstance.EffectsList.Clear();
            skillDataInstance.EffectsList.AddRange(ClonedEffects());
            break;

        case BehaviorOperation.Clear:
            skillDataInstance.EffectsList.Clear();
            break;

        default:
            Debug.LogWarning($"ModifierSkillBehavior: Unhandled operation: {_operation}");
            break;
        }
    }

    private void StackExistingEffect(SkillDataInstance skillDataInstance)
    {
        // Safety Check
        if (string.IsNullOrWhiteSpace(_stackParameter) || _effectsList == null || _effectsList.Count == 0) return;

        // 1. Resolve the type of the first effect in our list
        Type targetType = _effectsList[0].GetType();
        if (targetType == null) return;
        
        // 2. Find the first existing effect instance in the compiled EffectsList
        Effect existingEffect = null;
        foreach (Effect effect in skillDataInstance.EffectsList)
            if (effect != null && effect.GetType() == targetType)
            {
                existingEffect = effect;
                break;
            }
        
        if (existingEffect == null)
        {
            Debug.LogWarning($"ModifierSkillBehavior: Stack mode could not find existing '{targetType.Name}' in EffectsList.");
            return;
        }
         
        // Cache the Field Info once initially
        if (!_hasAttemptedStackCache)
        {
            _hasAttemptedStackCache = true;
            _cachedStackField = FindField(targetType, _stackParameter);
            
            if (_cachedStackField == null)
            {
                Debug.LogWarning($"ModifierSkillBehavior: Stack mode could not find field '{_stackParameter}' in '{targetType.Name}'.");
                return;
            }
            else if (_cachedStackField.FieldType != typeof(float))
            {
                Debug.LogWarning($"ModifierSkillBehavior: Stack field: {_stackParameter} on {targetType.Name} is not of type float.");
                _cachedStackField = null;
            }
        }
        
        if (_cachedStackField == null) return;
        
        // 
        float currentValue = (float)_cachedStackField.GetValue(existingEffect);
        _cachedStackField.SetValue(existingEffect, currentValue + _stackValue);
    }

    private List<Effect> ClonedEffects()
    {
        List<Effect> effectsListCloned = new List<Effect>();
        
        if (_effectsList == null) 
            return effectsListCloned;

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
}
    

public enum BehaviorOperation
{
    Append,
    Prepend,
    InsertAtIndex,
    ReplaceAll,
    Clear
}