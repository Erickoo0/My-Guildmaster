using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Modify the top-level behavior of a spell. (E.g. Effects)
/// </summary>
[CreateAssetMenu(fileName = "Spell_Modifier_Behavior_", menuName = "Spell Modifiers/Behavior Modifier")]
public class ModifierSpellBehavior : ModifierBaseSpell
{
    [Header("Behavior Modification")]
    [SerializeField] private BehaviorOperation operation;
    
    [Header("Effect Data")]
    [SerializeReference, SubclassSelector] private List<Effect> effects = new List<Effect>();
    [SerializeField] private int insertIndex;
    
    public BehaviorOperation Operation => operation;
    public IReadOnlyList<Effect> Effects => effects;
    public int InsertIndex => insertIndex;

    public override void ApplyModifier(SpellDataInstance spellDataInstance)
    {
        // Safety Checks
        if (spellDataInstance == null)
        {
            Debug.LogWarning($"{name}: Cannot apply effect pipeline modifier because spellInstance is null.");
            return;
        }

        if (spellDataInstance.Effects == null)
        {
            Debug.LogWarning($"{name}: Cannot apply effect pipeline modifier because spellInstance.Effects is null.");
            return;
        }

        switch (operation)
        {
        case BehaviorOperation.Append:
            AppendEffects(spellDataInstance);
            break;

        case BehaviorOperation.Prepend:
            PrependEffects(spellDataInstance);
            break;

        case BehaviorOperation.InsertAtIndex:
            InsertEffectsAtIndex(spellDataInstance);
            break;

        case BehaviorOperation.ReplaceAll:
            ReplaceAllEffects(spellDataInstance);
            break;

        case BehaviorOperation.Clear:
            spellDataInstance.Effects.Clear();
            break;

        default:
            Debug.LogWarning($"{name}: Unhandled effect pipeline operation: {operation}");
            break;
        }
    }

    /// <summary>
    /// Appends the effects to the spellDataInstance's effect list.'
    /// </summary>
    private void AppendEffects(SpellDataInstance spellDataInstance)
    {
        foreach (Effect effect in ClonedModifiedEffects())
            spellDataInstance.Effects.Add(effect);
    }

    /// <summary>
    /// Inserts the effects at the start of the spellDataInstance's effect list.'
    /// </summary>
    private void PrependEffects(SpellDataInstance spellDataInstance)
    {
        List<Effect> clonedEffects = ClonedModifiedEffects();
        spellDataInstance.Effects.InsertRange(0, clonedEffects);
    }

    private void InsertEffectsAtIndex(SpellDataInstance spellDataInstance)
    {
        List<Effect> clonedEffects = ClonedModifiedEffects();
        int clampedIndex = Mathf.Clamp(insertIndex, 0, spellDataInstance.Effects.Count);
        spellDataInstance.Effects.InsertRange(clampedIndex, clonedEffects);
    }

    private void ReplaceAllEffects(SpellDataInstance spellDataInstance)
    {
        spellDataInstance.Effects.Clear();
        
        foreach (Effect effect in ClonedModifiedEffects())
            spellDataInstance.Effects.Add(effect);
    }

    private List<Effect> ClonedModifiedEffects()
    {
        List<Effect> clonedEffects = new List<Effect>();
        if (effects == null) return clonedEffects; // If there is no effects, return an empty list
        
        foreach (Effect effect in effects)
            if (effect != null) 
                clonedEffects.Add(effect.Clone());
        
        return clonedEffects;
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
