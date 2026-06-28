using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// Static Blueprint for a base level 1 skill.
/// </summary>
[CreateAssetMenu(fileName = "Skill_Data_", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("References")] 
    public string ID;
    public string Name;
    public Sprite Icon;
    public GameObject Prefab;
    public AnimationBool Animation;
    public int AnimationTag => Animator.StringToHash(Animation.ToString());

    [Header("Base Stats")] 
    public float MpCostBase = 0; // Not needed for enemies
    public float CastTimeBase;
    public bool CastBarDisplay = true;

    [Header("Behavior Settings")] 
    [SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();
    [SerializeReference, SubclassSelector] public List<Requirement> RequirementsList = new List<Requirement>();
    // Default Behavior (Projectiles control their own via EffectSpawnProjectile)
    public bool HitOncePerTarget = true;
    public bool DestroyOnMaxHits = true;
    public int MaxEnemiesHitBase;

    [Header("Selection Settings")]
    public float SelectionWeight = 10f;

    public bool AreRequirementsMet(GameObject context)
    {
        // 1. No requirements means always true
        if (RequirementsList == null || RequirementsList.Count == 0) return true;
        
        // 2. Iterate through each requirement and check if met
        foreach (Requirement requirement in RequirementsList)
            if (!requirement.IsMet(context)) 
                return false;

        // If no requirement returns false, then return true
        return true;
    }

    /// <summary>
    /// Create a fresh SkillDataInstance from this blueprint.
    /// With copied stats and effects ready for modification.
    /// </summary>
    public SkillDataInstance CreateSkillDataInstance()
    {
        // 1. Build the base stats dictionary of the skill
        Dictionary<SkillStat, float> statsBase = new Dictionary<SkillStat, float>
        {
            {SkillStat.MpCost, MpCostBase},
            {SkillStat.CastTime, CastTimeBase},
            {SkillStat.MaxEnemiesHit, MaxEnemiesHitBase},
            {SkillStat.HitOncePerTarget, HitOncePerTarget ? 1f : 0f},
            {SkillStat.DestroyOnMaxHits, DestroyOnMaxHits ? 1f : 0f},
            {SkillStat.DisplayCastBar, CastBarDisplay ? 1f : 0f}
        };
        
        // 2. Clone the effects list
        List<Effect> effectsClonedList = new List<Effect>();
        if (EffectsList != null)
            foreach (Effect effect in EffectsList)
                if (effect != null)
                    effectsClonedList.Add(effect.Clone());
        
        // 3. Construct and return the SkillDataInstance
        return new SkillDataInstance(this, statsBase, effectsClonedList);
            
    }
    
    
    
    
    protected void OnValidate()
    {
        if (ID != name)
        {
            ID = name;
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
