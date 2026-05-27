using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class ItemDataSo : ScriptableObject
{
    [Header("Item ID")] 
    [SerializeField] private string itemID;
    public string ItemID => itemID;

    [Header("Item Visuals")] 
    [SerializeField] private Sprite[] itemIcon;
    public Sprite[] ItemIcon => itemIcon;
    public bool IsAnimated => itemIcon != null && itemIcon.Length > 1;

    [SerializeField] private string itemName;
    public string ItemName => itemName;

    [SerializeField, TextArea] private string itemDescription;
    public string ItemDescription => itemDescription;

    [Header("Economics")]
    [SerializeField] private int itemValue; 
    public int ItemValue => itemValue;

    [SerializeField] private GameObject itemObject; 
    public GameObject ItemObject => itemObject;

    [Header("Item Properties")] 
    [SerializeField] private bool isStackable;
    public bool IsStackable => isStackable;

    [SerializeField] private int maxStackSize = 60;
    public int MaxStackSize => maxStackSize;
    
    [SerializeField] private bool isUsable;
    public bool IsUsable => isUsable;

    public virtual bool Use(ItemInstance itemInstance, GameObject target = null)
    {
        Debug.Log($"Using {itemName}");
        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Automatically sync ID with File Name
        string path = AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (itemID != fileName)
            {
                itemID = fileName;
                // Mark as dirty to ensure the change is saved
                EditorUtility.SetDirty(this);
            }
        }

        // Logic safety: Ensure stack size is at least 1 if stackable
        if (isStackable && maxStackSize < 1)
        {
            maxStackSize = 1;
        }
    }
#endif
}