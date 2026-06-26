using UnityEngine;

[System.Serializable]
public abstract class Effect
{
    public abstract bool Execute(EffectPayload payload);
    
    /// <summary>
    /// Clone the effects so that modifiers can mutate or extend the effects
    /// within the SkillDataInstance without affecting the original
    /// </summary>
    public abstract Effect Clone();
}

