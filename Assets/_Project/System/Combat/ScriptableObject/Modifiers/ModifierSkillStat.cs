using UnityEngine;

/// <summary>
/// Modifys top-level stats of a SkillDataInstance. (E.g. MpCost, CastTime, MaxEnemiesHit, DisplayCastBar)
/// </summary>
[CreateAssetMenu(fileName = "Spell_Modifier_Stat_", menuName = "Spell Modifiers/Stat Modifier")]
public class ModifierSkillStat : ModifierSkillBase
{
    [Header("Stat Modification")]
    [SerializeField] private SkillStat _skillStat;
    [SerializeField] private StatModificationOperation operation;
    [SerializeField] private float value;

    public SkillStat SkillStat => _skillStat;
    public StatModificationOperation Operation => operation;
    public float Value => value;

    public override void ApplyModifier(SkillDataInstance skillDataInstance)
    {
        if (skillDataInstance == null)
        {
            Debug.LogWarning($"{name}: Cannot apply stat modifier because SkillDataInstance is null.");
            return;
        }

        switch (operation)
        {
            case StatModificationOperation.Set:
                skillDataInstance.SetStat(_skillStat, value);
                break;
            case StatModificationOperation.Add:
                skillDataInstance.AddToStat(_skillStat, value);
                break;
            case StatModificationOperation.Multiply:
                skillDataInstance.MultiplyStat(_skillStat, value);
                break;
            default:
                Debug.LogWarning($"{name}: Invalid StatModificationOperation.");
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
