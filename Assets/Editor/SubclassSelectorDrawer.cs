using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.LabelField(position, label.text, "Use [SerializeReference] with this.");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // 1. Draw the foldout (the little arrow) and label
        Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        // 2. Draw the Dropdown Button
        Rect dropdownRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        
        string typeName = property.managedReferenceFullTypename.Split(' ').LastOrDefault();
        if (string.IsNullOrEmpty(typeName)) typeName = "None (null)";
        else typeName = typeName.Split('.').Last();

        if (GUI.Button(dropdownRect, typeName, EditorStyles.popup))
        {
            ShowTypeMenu(property);
        }

        // 3. THE FIX: Safely draw the child variables only if expanded, preventing the infinite loop!
        if (property.isExpanded && !string.IsNullOrEmpty(property.managedReferenceFullTypename))
        {
            EditorGUI.indentLevel++;
            
            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();

            float yOffset = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Iterate through every variable inside your state and draw it
            if (child.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;
                    
                    float childHeight = EditorGUI.GetPropertyHeight(child, true);
                    Rect childRect = new Rect(position.x, yOffset, position.width, childHeight);
                    
                    EditorGUI.PropertyField(childRect, child, true);
                    
                    yOffset += childHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                while (child.NextVisible(false));
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void ShowTypeMenu(SerializedProperty property)
    {
        var menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(property.managedReferenceFullTypename), () => {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        });

        // Correctly identify the type whether it's a single variable, a List, or an Array
        Type targetType = fieldInfo.FieldType;
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
        {
            targetType = targetType.GetGenericArguments()[0];
        }
        else if (targetType.IsArray)
        {
            targetType = targetType.GetElementType();
        }

        var types = TypeCache.GetTypesDerivedFrom(targetType)
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var type in types)
        {
            menu.AddItem(new GUIContent(type.Name), false, () => {
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            });
        }
        
        menu.ShowAsContext();
    }

    // THE FIX PART 2: We must manually calculate the height of all children in the list
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // SAFETY GATE: If Unity is restructuring the array mid-frame, the property type 
        // might momentarily slip. If it's not a ManagedReference anymore, drop out instantly.
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        float height = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded && !string.IsNullOrEmpty(property.managedReferenceFullTypename))
        {
            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();

            if (child.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;
                
                    // SAFETY GATE: If Unity's internal serialization stream returns a junk 
                    // height during array deletion transitions, handle it gracefully.
                    try
                    {
                        height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    catch
                    {
                        break;
                    }
                }
                while (child.NextVisible(false));
            }
        }

        return height;
    }
}