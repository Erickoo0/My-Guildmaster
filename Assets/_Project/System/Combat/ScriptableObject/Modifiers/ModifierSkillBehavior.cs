using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Modify the top-level behavior of a skill. (E.g. EffectsList)
/// </summary>
[CreateAssetMenu(fileName = "Skill_Modifier_Behavior_", menuName = "Skills/Modifiers/Behavior Modifier")]
public class ModifierSkillBehavior : ModifierSkillBase
{
    [Header("Behavior Modification")]
    [SerializeField] private BehaviorOperation _operation;
    
    [Header("Effect Data")]
    [SerializeReference, SubclassSelector] private List<Effect> _effectList = new List<Effect>();
    [SerializeField] private int _insertIndex;
    
    public BehaviorOperation Operation => _operation;
    public IReadOnlyList<Effect> EffectList => _effectList;
    public int InsertIndex => _insertIndex;

    public override void ApplyModifier(SkillDataInstance skillDataInstance)
    {
        // Safety Checks
        if (skillDataInstance == null)
        {
            Debug.LogWarning($"{name}: Cannot apply effect pipeline modifier because spellInstance is null.");
            return;
        }

        if (skillDataInstance.EffectsList == null)
        {
            Debug.LogWarning($"{name}: Cannot apply effect pipeline modifier because spellInstance.EffectsList is null.");
            return;
        }

        switch (_operation)
        {
        case BehaviorOperation.Append:
            AppendEffects(skillDataInstance);
            break;

        case BehaviorOperation.Prepend:
            PrependEffects(skillDataInstance);
            break;

        case BehaviorOperation.InsertAtIndex:
            InsertEffectsAtIndex(skillDataInstance);
            break;

        case BehaviorOperation.ReplaceAll:
            ReplaceAllEffects(skillDataInstance);
            break;

        case BehaviorOperation.Clear:
            skillDataInstance.EffectsList.Clear();
            break;

        default:
            Debug.LogWarning($"{name}: Unhandled effect pipeline _operation: {_operation}");
            break;
        }
    }

    /// <summary>
    /// Appends the _effectList to the SkillDataInstance's effect list.'
    /// </summary>
    private void AppendEffects(SkillDataInstance skillDataInstance)
    {
        foreach (Effect effect in ModifiedEffectsCloned())
            skillDataInstance.EffectsList.Add(effect);
    }

    /// <summary>
    /// Inserts the _effectList at the start of the SkillDataInstance's effect list.'
    /// </summary>
    private void PrependEffects(SkillDataInstance skillDataInstance)
    {
        List<Effect> clonedEffects = ModifiedEffectsCloned();
        skillDataInstance.EffectsList.InsertRange(0, clonedEffects);
    }

    private void InsertEffectsAtIndex(SkillDataInstance skillDataInstance)
    {
        List<Effect> effectsClonedList = ModifiedEffectsCloned();
        int clampedIndex = Mathf.Clamp(_insertIndex, 0, skillDataInstance.EffectsList.Count);
        skillDataInstance.EffectsList.InsertRange(clampedIndex, effectsClonedList);
    }

    private void ReplaceAllEffects(SkillDataInstance skillDataInstance)
    {
        skillDataInstance.EffectsList.Clear();
        
        foreach (Effect effect in ModifiedEffectsCloned())
            skillDataInstance.EffectsList.Add(effect);
    }

    private List<Effect> ModifiedEffectsCloned()
    {
        List<Effect> effectsClonedList = new List<Effect>();
        if (_effectList == null) return effectsClonedList; // If there is no _effectList, return an empty list
        
        foreach (Effect effect in _effectList)
            if (effect != null) 
                effectsClonedList.Add(effect.Clone());
        
        return effectsClonedList;
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
