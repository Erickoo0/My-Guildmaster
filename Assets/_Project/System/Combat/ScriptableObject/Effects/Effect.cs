using UnityEngine;

[System.Serializable]
public abstract class Effect
{
    public abstract bool Execute(EffectPayload payload);
}

