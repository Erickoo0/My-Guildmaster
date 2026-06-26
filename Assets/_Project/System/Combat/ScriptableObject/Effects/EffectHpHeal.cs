using UnityEngine;

[System.Serializable]
public class EffectHpHeal : Effect
{
    public float HpHealBase;

    public override bool Execute(EffectPayload payload)
    {
        GameObject healTarget = payload.Target != null ? payload.Target : payload.User;

        if (healTarget.TryGetComponent(out IStatProvider statProvider))
        {
            // Safety Check
            Health health = statProvider.EntityHealth != null ? statProvider.EntityHealth : null;
            if (health == null) return false;
            
            // Calculate final value
            float totalHeal = HpHealBase;
            
            // Healing Logic
            if (totalHeal > 0)
            {
                if (health.HpCurrent >= health.HpMax) return false;
                health.HpHealInstant(totalHeal);
                return true;
            }

            // Damage Logic
            if (totalHeal < 0)
            {
                if (health.HpCurrent <= 0) return false;
                health.HpHealInstant(totalHeal);
                return true;
            }
        }
        
        return false;
    }

    public override Effect Clone()
    {
        return new EffectHpHeal
        {
            HpHealBase = HpHealBase
        };
    }
}
