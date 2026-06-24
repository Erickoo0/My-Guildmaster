using UnityEngine;

/// <summary>
/// Modifys top-level stats of a SpellDataInstance. (E.g. MpCost, CastTime, MaxEnemiesHit, DisplayCastBar)
/// </summary>
[CreateAssetMenu(fileName = "Spell_Modifier_Stat_", menuName = "Spell Modifiers/Stat Modifier")]
public class ModifierSpellStat : ModifierBaseSpell
{
    [Header("Stat Modification")]
    [SerializeField] private SpellStat spellStat;
    [SerializeField] private StatModificationOperation operation;
    [SerializeField] private float value;

    public SpellStat SpellStat => spellStat;
    public StatModificationOperation Operation => operation;
    public float Value => value;

    public override void ApplyModifier(SpellDataInstance spellDataInstance)
    {
        if (spellDataInstance == null)
        {
            Debug.LogWarning($"{name}: Cannot apply stat modifier because spellDataInstance is null.");
            return;
        }

        switch (operation)
        {
            case StatModificationOperation.Set:
                spellDataInstance.SetStat(spellStat, value);
                break;
            case StatModificationOperation.Add:
                spellDataInstance.AddToStat(spellStat, value);
                break;
            case StatModificationOperation.Multiply:
                spellDataInstance.MultiplyStat(spellStat, value);
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
