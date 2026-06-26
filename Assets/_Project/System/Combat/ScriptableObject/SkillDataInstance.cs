using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A runtime instance of a SkillData. Holds mutable stats.
/// CombatSystem will modify stats and apply skill tree effects to this instance.
/// </summary>
public class SkillDataInstance
{
    [Header("References")]
    private SkillData SkillDataSource { get; set; }

    [Header("SkillData Stats")]
    // Dictionary of all stats that can be modified at runtime.
    private readonly Dictionary<SkillStat, float> _skillStateDictionary;
    public List<Effect> EffectsList { get; private set; }
    
    [Header("Accessors")]
    public float MpCost => GetStat(SkillStat.MpCost);
    public float CastTime => GetStat(SkillStat.CastTime);
    public int MaxEnemiesHit => Mathf.RoundToInt(GetStat(SkillStat.MaxEnemiesHit));
    public bool HitOncePerTarget => GetStat(SkillStat.HitOncePerTarget) > 0.5f;
    public bool DestroyOnMaxHits => GetStat(SkillStat.DestroyOnMaxHits) > 0.5f;
    public bool DisplayCastBar => GetStat(SkillStat.DisplayCastBar) > 0.5f;
    
    //Pass through to immutable Source data
    public string ID => SkillDataSource.ID;
    public string Name => SkillDataSource.Name;
    public Sprite Icon => SkillDataSource.Icon;
    public GameObject Prefab => SkillDataSource.Prefab;
    public int AnimationTag => SkillDataSource.AnimationTag;
    public AnimationBool Animation => SkillDataSource.Animation;
    public List<Requirement> RequirementsList => SkillDataSource.RequirementsList;
    public float SelectionWeight => SkillDataSource.SelectionWeight;
    
    public bool AreRequirementsMet(GameObject context) => SkillDataSource.AreRequirementsMet(context);
    
    // Constructor
    public SkillDataInstance(SkillData skillDataSource, Dictionary<SkillStat, float> skillStatDictionary, List<Effect> clonedEffectsList)
    {
        SkillDataSource = skillDataSource;
        _skillStateDictionary = skillStatDictionary;
        EffectsList = clonedEffectsList; 
    }
       
    //----STAT ACCESSORS----
    
    ///<summary> Gets the value of a stat, returning the provided fault if not present</summary>
    public float GetStat(SkillStat stat, float defaultValue = 0f) => _skillStateDictionary.GetValueOrDefault(stat, defaultValue);
    
    ///<summary> Sets a stat value directly </summary>
    public void SetStat(SkillStat stat, float value) => _skillStateDictionary[stat] = value;

    ///<summary> Adds value to an existing stat </summary>
    public void AddToStat(SkillStat stat, float value)
    {
        if (_skillStateDictionary.ContainsKey(stat))
            _skillStateDictionary[stat] += value;
        else
            _skillStateDictionary[stat] = value;
    }

    ///<summary> Multiplies an existing stat value </summary>
    public void MultiplyStat(SkillStat stat, float multiplier)
    {
        if (_skillStateDictionary.ContainsKey(stat))
            _skillStateDictionary[stat] *= multiplier;
    }
}


/// <summary>
/// Enumeration of all skill stats that can be modified at runtime.
/// </summary>
public enum SkillStat
{
    [Header("Core Stats")]
    MpCost,
    CastTime,
    
    [Header("Hit Behavior")]
    MaxEnemiesHit,
    HitOncePerTarget, // Treated as bool: > 0.5f = true
    DestroyOnMaxHits, // Treated as bool: > 0.5f = true
    DisplayCastBar,   // Treated as bool: > 0.5f = true
    
    // Future extensions
}
