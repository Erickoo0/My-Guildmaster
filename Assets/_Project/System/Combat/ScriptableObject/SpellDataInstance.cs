using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A runtime instance of a SpellData. Holds mutable stats.
/// CombatSystem will modify stats and apply skill tree effects to this instance.
/// </summary>
public class SpellDataInstance
{
    [Header("References")]
    public SpellData SourceSpellData { get; private set; }

    [Header("SpellData Stats")]
    ///<summary> Dictionary of all stats that can be modified at runtime. </summary>
    private Dictionary<SpellStat, float> _stats = new Dictionary<SpellStat, float>();
    public List<Effect> Effects { get; private set; }
    
    [Header("Accessors")]
    public float MpCost => GetStat(SpellStat.MpCost);
    public float CastTime => GetStat(SpellStat.CastTime);
    public int MaxEnemiesHit => Mathf.RoundToInt(GetStat(SpellStat.MaxEnemiesHit));
    public bool HitOncePerTarget => GetStat(SpellStat.HitOncePerTarget) > 0.5f;
    public bool DestroyOnMaxHits => GetStat(SpellStat.DestroyOnMaxHits) > 0.5f;
    public bool DisplayCastBar => GetStat(SpellStat.DisplayCastBar) > 0.5f;
    
    //Pass through to immutable source data
    public string SpellID => SourceSpellData.spellID;
    public string SpellName => SourceSpellData.spellName;
    public Sprite SpellIcon => SourceSpellData.spellIcon;
    public GameObject SpellPrefab => SourceSpellData.spellPrefab;
    public int AnimationTag => SourceSpellData.AnimationTag;
    public AnimationBool SpellAnimation => SourceSpellData.spellAnimation;
    public List<Requirement> SpellRequirements => SourceSpellData.spellRequirements;
    public float SelectionWeight => SourceSpellData.selectionWeight;
    
    public bool CheckRequirementsMet(GameObject context) => SourceSpellData.CheckRequirementsMet(context);
    
    // Constructor
    public SpellDataInstance(SpellData sourceSpellData, Dictionary<SpellStat, float> baseStats, List<Effect> clonedEffects)
    {
        SourceSpellData = sourceSpellData;
        _stats = baseStats;
        Effects = clonedEffects; 
    }
       
    //----STAT ACCESSORS----
    
    ///<summary> Gets the value of a stat, returning the provided fault if not present</summary>
    public float GetStat(SpellStat stat, float defaultValue = 0f) => _stats.TryGetValue(stat, out float value) ? value : defaultValue;
    
    ///<summary> Sets a stat value directly </summary>
    public void SetStat(SpellStat stat, float value) => _stats[stat] = value;

    ///<summary> Adds value to an existing stat </summary>
    public void AddToStat(SpellStat stat, float value)
    {
        if (_stats.ContainsKey(stat))
            _stats[stat] += value;
        else
            _stats[stat] = value;
    }

    ///<summary> Multiplies an existing stat value </summary>
    public void MultiplyStat(SpellStat stat, float multiplier)
    {
        if (_stats.ContainsKey(stat))
            _stats[stat] *= multiplier;
    }
}


/// <summary>
/// Enumeration of all spell stats that can be modified at runtime.
/// </summary>
public enum SpellStat
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
