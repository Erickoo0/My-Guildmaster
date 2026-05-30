using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls; // Required for AdvancedDropdown
using System.Linq;
using System.Collections.Generic;
using System;

[CustomPropertyDrawer(typeof(HierarchySelectorAttribute))]
public class HierarchySelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use [HierarchySelector] only on strings!");
            return;
        }

        label = EditorGUI.BeginProperty(position, label, property);
        
        HierarchySelectorAttribute selectorAttribute = (HierarchySelectorAttribute)attribute;
        System.Type typeToFind = selectorAttribute.TargetType;
        
        UnityEngine.Object[] foundObjects = UnityEngine.Object.FindObjectsByType(typeToFind, FindObjectsInactive.Exclude, FindObjectsSortMode.None);    
        
        List<string> objectNames = foundObjects
            .Select(obj => obj.name)
            .OrderBy(name => name)
            .Distinct() 
            .ToList();
        
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        string buttonText = string.IsNullOrEmpty(property.stringValue) ? "None / Empty" : property.stringValue;
        
        if (GUI.Button(position, buttonText, EditorStyles.popup))
        {
            // Capture the exact serialized path and the target component reference safely
            string propertyPath = property.propertyPath;
            UnityEngine.Object targetObject = property.serializedObject.targetObject;

            var dropdown = new SearchableHierarchyDropdown(new AdvancedDropdownState(), objectNames, (selectedValue) =>
            {
                // DELAY CALL: Pushes the data assignment to a safe, synchronous main-thread frame tick.
                // This guarantees permanent serialization and completely avoids the C++ vector desync crash.
                EditorApplication.delayCall += () =>
                {
                    if (targetObject == null) return;
                    
                    SerializedObject serializedObject = new SerializedObject(targetObject);
                    SerializedProperty targetProperty = serializedObject.FindProperty(propertyPath);
                    
                    if (targetProperty != null)
                    {
                        targetProperty.stringValue = selectedValue;
                        serializedObject.ApplyModifiedProperties();
                    }
                };
            });

            dropdown.Show(position);
        }

        EditorGUI.EndProperty();
    }
}

// Seamless Search Window Implementation
public class SearchableHierarchyDropdown : AdvancedDropdown
{
    private readonly List<string> _items;
    private readonly Action<string> _onItemSelected;

    public SearchableHierarchyDropdown(AdvancedDropdownState state, List<string> items, Action<string> onItemSelected) : base(state)
    {
        _items = items;
        _onItemSelected = onItemSelected;
        this.minimumSize = new Vector2(250, 300);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Scene Objects");
        root.AddChild(new AdvancedDropdownItem("None / Empty"));

        foreach (string itemName in _items)
        {
            root.AddChild(new AdvancedDropdownItem(itemName));
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        base.ItemSelected(item);
        _onItemSelected?.Invoke(item.name == "None / Empty" ? "" : item.name);
    }
}