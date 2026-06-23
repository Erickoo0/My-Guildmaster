using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// Base Level 1 Spell Data Blueprint.
/// </summary>
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

    /// <summary>
    /// Create a fresh SpellDataInstance from this blueprint.
    /// With copied stats and effects ready for modification.
    /// </summary>
    public SpellDataInstance CreateSpellDataInstance()
    {
        // 1. Build the base stats dictionary of the spell
        Dictionary<SpellStat, float> baseStats = new Dictionary<SpellStat, float>
        {
            {SpellStat.MpCost, baseMpCost},
            {SpellStat.CastTime, baseCastTime},
            {SpellStat.MaxEnemiesHit, baseMaxEnemiesHit},
            {SpellStat.HitOncePerTarget, hitOncePerTarget ? 1f : 0f},
            {SpellStat.DestroyOnMaxHits, destroyOnMaxHits ? 1f : 0f},
            {SpellStat.DisplayCastBar, displayCastBar ? 1f : 0f}
        };
        
        // 2. Clone the effects list
        List<Effect> clonedEffects = new List<Effect>();
        if (spellEffects != null)
            foreach (Effect effect in spellEffects)
                if (effect != null)
                    clonedEffects.Add(effect.Clone());
        
        // 3. Construct and return the SpellDataInstance
        return new SpellDataInstance(this, baseStats, clonedEffects);
            
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
