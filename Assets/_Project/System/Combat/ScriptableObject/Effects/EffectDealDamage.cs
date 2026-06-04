using UnityEngine;

[System.Serializable]
public class EffectDealDamage : Effect
{
    public float damageAmount;
    public DamageType damageType;
    public float knockbackForce = 30f;
    public float knockbackDuration = 0.35f;
    public float knockbackHeight = 1f;

    public override bool Execute(EffectPayload effectPayload)
    {
        if (effectPayload.Target == null) return false;

        if (effectPayload.Target.TryGetComponent(out IDamagable target))
        {
            DamageData data = new DamageData(
                damageAmount,
                effectPayload.HitDirection,   // Passed from Hitbox physics
                effectPayload.HitImpactPoint, // Passed from Hitbox physics
                knockbackForce,
                knockbackDuration,
                knockbackHeight,
                damageType,
                effectPayload.User
                ); 
            
            target.TakeDamage(data);
            return true;
        }
        
        return false;
    }
}
