using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomPropertyDrawer(typeof(EffectFieldSelectorAttribute))]
public class EffectFieldSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "[EffectFieldSelector] requires a string field.");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        // 1. Find the sibling type-name field to know which type we're targeting
        EffectFieldSelectorAttribute attr = (EffectFieldSelectorAttribute)attribute;
        string siblingPath = GetSiblingPath(property.propertyPath, attr.TypeFieldName);
        SerializedProperty typeNameProp = property.serializedObject.FindProperty(siblingPath);

        string typeName = typeNameProp?.stringValue ?? "";
        Type targetType = string.IsNullOrEmpty(typeName) ? null : ResolveType(typeName);

        // 2. Gather float fields from the resolved type
        List<string> floatFields = new List<string>();
        if (targetType != null)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = targetType;
            while (t != null && t != typeof(object))
            {
                foreach (FieldInfo f in t.GetFields(flags))
                    if (f.FieldType == typeof(float) && !floatFields.Contains(f.Name))
                        floatFields.Add(f.Name);
                t = t.BaseType;
            }
            floatFields.Sort();
        }

        // 3. Draw the button
        string current = property.stringValue;
        string buttonLabel = string.IsNullOrEmpty(current)
            ? (targetType == null ? "— Select Effect Type First —" : "— Select Field —")
            : current;

        bool noFields = floatFields.Count == 0;
        EditorGUI.BeginDisabledGroup(noFields);

        if (GUI.Button(position, buttonLabel, EditorStyles.popup) && !noFields)
        {
            string propertyPath = property.propertyPath;
            UnityEngine.Object targetObject = property.serializedObject.targetObject;

            var dropdown = new StringSearchDropdown(
                new AdvancedDropdownState(),
                $"Float Fields on {typeName}",
                floatFields,
                selected =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (targetObject == null) return;
                        SerializedObject so = new SerializedObject(targetObject);
                        SerializedProperty prop = so.FindProperty(propertyPath);
                        if (prop != null)
                        {
                            prop.stringValue = selected;
                            so.ApplyModifiedProperties();
                        }
                    };
                });

            dropdown.Show(position);
        }

        EditorGUI.EndDisabledGroup();
        EditorGUI.EndProperty();
    }

    private static string GetSiblingPath(string propertyPath, string siblingFieldName)
    {
        int lastDot = propertyPath.LastIndexOf('.');
        return lastDot >= 0
            ? propertyPath[..lastDot] + "." + siblingFieldName
            : siblingFieldName;
    }

    private static Type ResolveType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = assembly.GetType(typeName);
            if (t != null) return t;
        }
        return null;
    }
}