using UnityEngine;

public abstract class SpellData : ScriptableObject
{
    [Header("References")] 
    [Tooltip("This ID is automatically set to the filename of this ScriptableObject.")]
    public string spellID;
    public string spellName;
    public Sprite spellIcon;
    public GameObject spellPrefab;
    public AnimationBool spellAnimation;
    public int AnimationTag => Animator.StringToHash(spellAnimation.ToString());

    [Header("Base Stats")] 
    public float baseDamage;
    //public float baseMpCost; // Not needed for enemies
    public float baseCastTime;

    [Header("Behavior Settings")] 
    public bool hitOncePerTarget = true;
    public bool destroyOnMaxHits = true;
    public int baseMaxEnemiesHit;
    public float spellScale = 1;
    
    [Header("Knockback & Type")]
    public DamageType damageType;
    public float knockbackForce = 30f;
    public float knockbackDuration = 0.35f;
    public float knockbackHeight = 1f;
    
    protected virtual void OnValidate()
    {
        // 'name' is a built-in Unity property that returns the filename 
        // of the ScriptableObject 
        if (spellID != name)
        {
            spellID = name;
            
            // Marks the object as 'dirty' so Unity knows to save the change
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    // Helper function to take the Base Stats from the SO and turn them into a DamageData struct
    // This DamageData gets passed from State -> Hitbox
    // Then the Hitbox modifies that damage data before passing to the Hurtbox
    public DamageData CreateDamageData(GameObject attacker)
    {
        return new DamageData(
            baseDamage,
            Vector2.zero, // Calculated in Hitbox
            Vector2.zero, // Calculated in Hitbox
            knockbackForce,
            knockbackDuration,
            knockbackHeight,
            damageType,
            attacker,
            baseMaxEnemiesHit,
            hitOncePerTarget,
            destroyOnMaxHits
        );
    }
}

public enum AnimationBool
{
    IsAttacking,
    IsAttackingStrong
}
