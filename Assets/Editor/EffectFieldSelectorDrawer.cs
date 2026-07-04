using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;
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

		EffectFieldSelectorAttribute attr = (EffectFieldSelectorAttribute)attribute;
		string siblingPath = GetSiblingPath(property.propertyPath, attr.TypeFieldName);
		SerializedProperty typeNameProp = property.serializedObject.FindProperty(siblingPath);

		string typeName = typeNameProp?.stringValue ?? "";
		Type targetType = string.IsNullOrEmpty(typeName) ? null : ResolveType(typeName);

		// Gather float, int, and bool fields from the resolved type
		List<string> candidateFields = new List<string>();
		if (targetType != null)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			Type t = targetType;
			while (t != null && t != typeof(object))
			{
				foreach (FieldInfo f in t.GetFields(flags))
				{
					if ((f.FieldType == typeof(float) ||
						f.FieldType == typeof(int) ||
						f.FieldType == typeof(bool)) &&
						!candidateFields.Contains(f.Name))
					{
						candidateFields.Add(f.Name);
					}
				}
				t = t.BaseType;
			}
			candidateFields.Sort();
		}

		string current = property.stringValue;
		string buttonLabel = string.IsNullOrEmpty(current)
			? (targetType == null ? "— Select Effect Type First —" : "— Select Field —")
			: current;

		bool noFields = candidateFields.Count == 0;
		EditorGUI.BeginDisabledGroup(noFields);

		if (GUI.Button(position, buttonLabel, EditorStyles.popup) && !noFields)
		{
			string propertyPath = property.propertyPath;
			Object targetObject = property.serializedObject.targetObject;

			var dropdown = new StringSearchDropdown(
				new AdvancedDropdownState(),
				$"Fields on {typeName}",
				candidateFields,
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
