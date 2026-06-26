using UnityEngine;

/// <summary>
/// Base class for all skill modifiers.
/// Modifiers mutate a SpellDat aInstance during compilation.
/// </summary>
public abstract class ModifierSkillBase : ScriptableObject
{
    [Header("Modifier Metadata")]
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [TextArea, SerializeField] private string _description;
    
    public string ID => _id;
    public string Name => _name;
    public string Description => _description;
    
    public abstract void ApplyModifier(SkillDataInstance skillDataInstance);
    
    protected virtual void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(_id))
            _id = name;
    }    
}
