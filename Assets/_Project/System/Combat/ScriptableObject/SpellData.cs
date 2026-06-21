using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Spell_Data_", menuName = "SpellData/BaseSpellData")]
public class SpellData : ScriptableObject
{
    [Header("References")] 
    public string spellID;
    public string spellName;
    public Sprite spellIcon;
    public GameObject spellPrefab;
    public AnimationBool spellAnimation;
    public int AnimationTag => Animator.StringToHash(spellAnimation.ToString());

    [Header("Base Stats")] 
    public float baseMpCost = 0; // Not needed for enemies
    public float baseCastTime;
    public bool displayCastBar = true;

    [Header("Behavior Settings")] 
    [SerializeReference, SubclassSelector] public List<Effect> spellEffects = new List<Effect>();
    // Default Behavior (Projectiles control their own via EffectSpawnProjectile)
    public bool hitOncePerTarget = true;
    public bool destroyOnMaxHits = true;
    public int baseMaxEnemiesHit;

    [Header("Selection Settings")]
    public float selectionWeight = 10f;
    
    protected virtual void OnValidate()
    {
        if (spellID != name)
        {
            spellID = name;
        #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
        #endif
        }
    }
}

public enum AnimationBool
{
    IsAttacking,
    IsAttackingStrong
}
