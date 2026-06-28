using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Modifies the top-level EffectsList of a SkillDataInstance.
/// Used to append, prepend, insert, replace, or clear effects.
/// </summary>
[System.Serializable]
public class ModifierSkillBehavior : ModifierSkillBase
{
    [Header("Effect Data")]
    [SerializeField] private BehaviorOperation _operation;
    [SerializeReference, SubclassSelector] private List<Effect> _effectsList = new List<Effect>();
    [SerializeField] private int _insertIndex;

    public override void ApplyModifier(SkillDataInstance skillDataInstance)
    {
        // Safety Checks
        if (skillDataInstance?.EffectsList == null)
        {
            Debug.LogWarning("ModifierSkillBehavior: Cannot apply modifier. SkillDataInstance or its EffectsList is null.");
            return;
        }

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
}

public enum BehaviorOperation
{
    Append,
    Prepend,
    InsertAtIndex,
    ReplaceAll,
    Clear
}