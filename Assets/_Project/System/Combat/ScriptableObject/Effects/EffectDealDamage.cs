using UnityEngine;

[System.Serializable]
public class EffectDealDamage : Effect
{
    public float Amount;
    public DamageType Type;
    public float KnockbackForce = 10f;
    public float KnockbackDuration = 0.2f;
    public float KnockbackHeight = 0.1f;

    public override bool Execute(EffectPayload effectPayload)
    {
        if (effectPayload.Target == null) return false;

        if (effectPayload.Target.TryGetComponent(out IDamagable target))
        {
            DamageData data = new DamageData(
                Amount,
                effectPayload.HitDirection,   // Passed from Hitbox physics
                effectPayload.HitImpactPoint, // Passed from Hitbox physics
                KnockbackForce,
                KnockbackDuration,
                KnockbackHeight,
                Type,
                effectPayload.User
                ); 
            
            target.TakeDamage(data);
            return true;
        }
        
        return false;
    }

    public override Effect Clone()
    {
        return new EffectDealDamage
        {
            Amount = Amount,
            Type = Type,
            KnockbackForce = KnockbackForce,
            KnockbackDuration = KnockbackDuration,
            KnockbackHeight = KnockbackHeight
        };
    }
}
