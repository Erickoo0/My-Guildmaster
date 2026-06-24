using UnityEngine;

/// <summary>
/// Base class for all spell modifiers.
/// Modifiers mutate a SpellDat aInstance during compilation.
/// </summary>
public abstract class ModifierBaseSpell : ScriptableObject
{
    [Header("Modifier Metadata")]
    [SerializeField] private string modifierID;
    [SerializeField] private string modifierName;
    [TextArea, SerializeField] private string modifierDescription;
    
    public string ModifierID => modifierID;
    public string ModifierName => modifierName;
    public string ModifierDescription => modifierDescription;
    
    public abstract void ApplyModifier(SpellDataInstance instance);
    
    protected virtual void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(modifierID))
            modifierID = name;
    }    
}
