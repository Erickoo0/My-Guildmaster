using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Item/Consumable")]
public class ItemDataSoConsumable : ItemDataSo
{
    [SerializeField] private GameObject buffPrefab;
    
    [Header("Hp Restoration")]
    public float hpHealAmount;
    public float hpHealDuration;

    [Header("Mp Restoration")]
    public float mpHealAmount;
    public float mpHealDuration;
    
    public override bool Use(ItemInstance itemInstance, GameObject target = null)
    {
        // Fallback if no target is specified
        if (target == null) target = PlayerStatsManager.Instance.gameObject;
        
        bool hpRestored = HpHealCheck(target);
        bool mpRestored = MpHealCheck(target);
            
        return hpRestored || mpRestored;
    }
    
    private bool HpHealCheck(GameObject target)
    {
        // Safety Check
        if (!target.TryGetComponent(out IStatProvider statProvider)) return false;
        
        // 1. Get component
        Health health = statProvider.EntityHealth;
        if (health == null || hpHealAmount == 0) return false;
        
        // 2. Check if hp is full
        if (health.HpCurrent >= health.HpMax && hpHealAmount > 0)
        {
            Debug.Log("HP is full, item not used!");
            return false;
        }
        
        // 3. Check if heal over time
        if (hpHealDuration > 0)
        {
            // If there is already a active same type buff, then return false
            if (IsBuffTypeActive(target, BuffSpellData.BuffType.Health))
            {
                Debug.Log("HP is already healing! Wait for it to finish.");
                return false;
            }
            
            // 4. Create Buff Prefab & Pass data
            GameObject potionBuff = Instantiate(buffPrefab, target.transform);
            
            if (potionBuff.TryGetComponent(out Buff potionBuffComponent))
                potionBuffComponent.Setup(target, BuffSpellData.BuffType.Health, hpHealAmount, hpHealDuration);
        }
        else // 5. If not heal over time, then instantly.
        {
            health.HpHealInstant(hpHealAmount);
        }
        
        return true; // Successfully healed hp
    }
    
    private bool MpHealCheck(GameObject target)
    {
        if (!target.TryGetComponent(out IStatProvider statProvider)) return false;
        
        Mana mana = statProvider.EntityMana;
        if (mana == null || mpHealAmount == 0) return false;

        if (mana.MpCurrent >= mana.mpMax && mpHealAmount > 0)
        {
            Debug.Log("MP is full, item not used!");
            return false;
        }
        
        // Check if heal over time
        if (mpHealDuration > 0)
        {
            if (IsBuffTypeActive(target, BuffSpellData.BuffType.Mana))
            {
                Debug.Log("MP is already restoring! Wait for it to finish.");
                return false;
            }

            GameObject potionBuff = Instantiate(buffPrefab, target.transform);
            
            if (potionBuff.TryGetComponent(out Buff potionBuffComponent))
                potionBuffComponent.Setup(target, BuffSpellData.BuffType.Mana, mpHealAmount, mpHealDuration);
        }
        // If not heal over time, then instant
        else
        {
            mana.MpHealInstant(mpHealAmount);
        }
        
        return true; // Successfully healed mp
    }

    // Currently not valid since we use prefab and not components on game object
    private bool IsBuffTypeActive(GameObject target, BuffSpellData.BuffType buffType)
    {
        Buff[] activeBuffs = target.GetComponents<Buff>();
        foreach (Buff buff in activeBuffs)
        {
            if (buff.BuffType == buffType) return true;
        }
        
        return false;
    }
}

