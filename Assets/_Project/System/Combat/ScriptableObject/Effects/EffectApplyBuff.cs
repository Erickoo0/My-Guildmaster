using UnityEngine;

[System.Serializable]
public class EffectApplyBuff : Effect
{ 
    public GameObject buffPrefab;
    public BuffSpellData.BuffType buffType;
    public float amount;
    public float duration;

    public override bool Execute(EffectPayload payload)
    {
        // 1. Get buff target
        GameObject buffTarget = payload.Target != null ? payload.Target : payload.User;

        // 2. Check if buff target already has existing buff of same type
        Buff[] activeBuffs = buffTarget.GetComponentsInChildren<Buff>();
        foreach (Buff buff in activeBuffs)
        {
            if (buff.BuffType == buffType)
            {
                Debug.Log("Buff already exists!");
                return false;
            }
        }
        
        // 3. Create the buff prefab and Set it up
        GameObject buffInstance = Object.Instantiate(buffPrefab, buffTarget.transform);
        if (buffInstance.TryGetComponent(out Buff buffComponent))
        {
            buffComponent.Setup(buffTarget, buffType, amount, duration);
            return true;
        }

        Debug.Log("Buff prefab does not have Buff component!");
        return false;
    }
}
