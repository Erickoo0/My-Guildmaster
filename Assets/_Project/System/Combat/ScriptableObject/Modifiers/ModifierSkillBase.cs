using UnityEngine;

/// <summary>
/// Base class for all skill modifiers.
/// Modifiers mutate a SpellDataInstance during compilation.
/// </summary>
[System.Serializable]
public abstract class ModifierSkillBase
{
    public abstract void ApplyModifier(SkillDataInstance skillDataInstance);
}
