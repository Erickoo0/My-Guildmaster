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
    [SerializeReference, SubclassSelector] public List<Requirement> spellRequirements = new List<Requirement>();
    // Default Behavior (Projectiles control their own via EffectSpawnProjectile)
    public bool hitOncePerTarget = true;
    public bool destroyOnMaxHits = true;
    public int baseMaxEnemiesHit;

    [Header("Selection Settings")]
    public float selectionWeight = 10f;

    public bool CheckRequirementsMet(GameObject context)
    {
        // 1. No requirements means always true
        if (spellRequirements == null || spellRequirements.Count == 0) return true;
        
        // 2. Iterate through each requirement and check if met
        foreach (Requirement requirement in spellRequirements)
            if (!requirement.IsMet(context)) 
                return false;

        // If no requirement returns false, then return true
        return true;

    }
    
    
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
