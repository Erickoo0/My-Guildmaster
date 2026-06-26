using UnityEngine;

public enum BuffType { Health, Mana }

[System.Serializable]
public class EffectApplyBuff : Effect
{ 
    public GameObject Prefab;
    public BuffType Type;
    public float Amount;
    public float Duration;

    public override bool Execute(EffectPayload payload)
    {
        // 1. Get buff target
        GameObject buffTarget = payload.Target != null ? payload.Target : payload.User;

        // 2. Check if buff target already has existing buff of same type
        Buff[] activeBuffs = buffTarget.GetComponentsInChildren<Buff>();
        foreach (Buff buff in activeBuffs)
        {
            if (buff.Type == Type)
            {
                Debug.Log("Buff already exists!");
                return false;
            }
        }
        
        // 3. Create the buff prefab and Set it up
        GameObject buffInstance = Object.Instantiate(Prefab, buffTarget.transform);
        if (buffInstance.TryGetComponent(out Buff buffComponent))
        {
            buffComponent.Setup(buffTarget, Type, Amount, Duration);
            return true;
        }

        Debug.Log("Buff prefab does not have Buff component!");
        return false;
    }

    public override Effect Clone()
    {
        return new EffectApplyBuff
        {
            Prefab = Prefab,
            Type = Type,
            Amount = Amount,
            Duration = Duration
        };
    }
}
