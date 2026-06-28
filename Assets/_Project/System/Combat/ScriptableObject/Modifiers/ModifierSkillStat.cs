using UnityEngine;

/// <summary>
/// Modifies top-level stats of a SkillDataInstance. (E.g. MpCost, CastTime, MaxEnemiesHit, DisplayCastBar)
/// </summary>
[System.Serializable]
public class ModifierSkillStat : ModifierSkillBase
{
    [Header("Stat Modification")]
    [SerializeField] private SkillStat _skillStat;
    [SerializeField] private StatModificationOperation _operation;
    [SerializeField] private float _value;

    public override void ApplyModifier(SkillDataInstance skillDataInstance)
    {
        if (skillDataInstance == null)
        {
            Debug.LogWarning($"ModifierSkillStat: Stat modification failed. SkillDataInstance is null.");
            return;
        }

        switch (_operation)
        {
            case StatModificationOperation.Set:
                skillDataInstance.SetStat(_skillStat, _value);
                break;
            case StatModificationOperation.Add:
                skillDataInstance.AddToStat(_skillStat, _value);
                break;
            case StatModificationOperation.Multiply:
                skillDataInstance.MultiplyStat(_skillStat, _value);
                break;
            default:
                Debug.LogWarning($"ModifierSkillStat: Invalid StatModificationOperation.");
                break;
        }
    }
}

public enum StatModificationOperation 
{ 
    Set,
    Add, 
    Multiply 
}
