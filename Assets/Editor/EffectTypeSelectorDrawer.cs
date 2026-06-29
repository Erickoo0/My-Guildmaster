using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomPropertyDrawer(typeof(EffectTypeSelectorAttribute))]
public class EffectTypeSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "[EffectTypeSelector] requires a string field.");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        string current = property.stringValue;
        string buttonLabel = string.IsNullOrEmpty(current) ? "— Select Effect Type —" : current;

        if (GUI.Button(position, buttonLabel, EditorStyles.popup))
        {
            List<string> typeNames = TypeCache.GetTypesDerivedFrom<Effect>()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Select(t => t.Name)
                .OrderBy(n => n)
                .ToList();

            string propertyPath = property.propertyPath;
            UnityEngine.Object targetObject = property.serializedObject.targetObject;

            var dropdown = new StringSearchDropdown(
                new AdvancedDropdownState(),
                "Effect Types",
                typeNames,
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

        EditorGUI.EndProperty();
    }
}