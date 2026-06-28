using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class Effect
{
    public abstract bool Execute(EffectPayload payload);
    
    /// <summary>
    /// Clone the effects so that modifiers can mutate or extend the effects
    /// within the SkillDataInstance without affecting the original
    /// </summary>
    public abstract Effect Clone();

    /// <summary>
    /// Returns the nested EffectsList from this effect (e.g EffectSpawnProjectile)
    /// Used by ModifierSkillEffect to modify nested effects.
    /// </summary>
    public virtual List<Effect> GetNestedEffects() => null;
}

