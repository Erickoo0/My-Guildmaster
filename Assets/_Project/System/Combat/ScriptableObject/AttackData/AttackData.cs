using UnityEngine;

public abstract class AttackData : ScriptableObject
{
    [Header("IDs")] 
    [Tooltip("This ID is automatically set to the filename of this ScriptableObject.")]
    public string attackID;
    
    public GameObject attackPrefab;
    public DamageData damageData;
    
    protected virtual void OnValidate()
    {
        // 'name' is a built-in Unity property that returns the filename 
        // of the ScriptableObject 
        if (attackID != name)
        {
            attackID = name;
            
            // Marks the object as 'dirty' so Unity knows to save the change
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
