using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A runtime instance of a SpellData. Holds mutable stats.
/// CombatSystem will modify stats and apply skill tree effects to this instance.
/// </summary>
public class SpellDataInstance
{
    [Header("References")]
    public SpellData SpellDataSource { get; private set; }

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
    public string SpellID => SpellDataSource.spellID;
    public string SpellName => SpellDataSource.spellName;
    public Sprite SpellIcon => SpellDataSource.spellIcon;
    public GameObject SpellPrefab => SpellDataSource.spellPrefab;
    public int AnimationTag => SpellDataSource.AnimationTag;
    public AnimationBool SpellAnimation => SpellDataSource.spellAnimation;
    public List<Requirement> SpellRequirements => SpellDataSource.spellRequirements;
    public float SelectionWeight => SpellDataSource.selectionWeight;
    
    public bool CheckRequirementsMet(GameObject context) => SpellDataSource.CheckRequirementsMet(context);
    
    // Constructor
    public SpellDataInstance(SpellData spellDataSource, Dictionary<SpellStat, float> baseStats, List<Effect> clonedEffects)
    {
        SpellDataSource = spellDataSource;
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
