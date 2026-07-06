using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Targets your specific prerequisite class
[CustomPropertyDrawer(typeof(SkillNodePrerequisite))]
public class SkillNodePrerequisiteDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		// 1. Fetch the serialized properties
		SerializedProperty idProp = property.FindPropertyRelative("_requiredSkillNodeID");
		SerializedProperty pointsProp = property.FindPropertyRelative("_requiredSkillPoints");

		// 2. Draw the main prefix label (e.g., "Element 0") and get the remaining space
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

		// 3. Calculate rects to display the Dropdown and Points field side-by-side on one line
		Rect popupRect = new Rect(position.x, position.y, position.width*0.65f - 5, position.height);
		Rect pointsRect = new Rect(position.x + position.width*0.65f, position.y, position.width*0.35f, position.height);

		// 4. Search the parent SkillTree object for all existing Node IDs
		List<string> availableIDs = new List<string>
		{
			"<None>"
		};
		SerializedProperty treeNodesProp = property.serializedObject.FindProperty("_skillNodes");

		if (treeNodesProp != null)
		{
			for (int i = 0; i < treeNodesProp.arraySize; i++)
			{
				SerializedProperty nodeProp = treeNodesProp.GetArrayElementAtIndex(i);
				SerializedProperty nodeIDProp = nodeProp.FindPropertyRelative("_id");

				// Only add valid, non-empty IDs to the dropdown
				if (nodeIDProp != null && !string.IsNullOrWhiteSpace(nodeIDProp.stringValue))
				{
					availableIDs.Add(nodeIDProp.stringValue);
				}
			}
		}

		// 5. Determine which index is currently selected
		string currentID = idProp.stringValue;
		int selectedIndex = 0;

		if (!string.IsNullOrEmpty(currentID))
		{
			selectedIndex = availableIDs.IndexOf(currentID);
			if (selectedIndex == -1)
			{
				// Edge case: If you rename an ID in the tree, this preserves the old string 
				// and flags it as missing so you know it's broken, rather than silently deleting it.
				availableIDs.Add(currentID + " (Missing)");
				selectedIndex = availableIDs.Count - 1;
			}
		}

		// 6. Draw the Dropdown Popup
		int newIndex = EditorGUI.Popup(popupRect, selectedIndex, availableIDs.ToArray());
		if (newIndex == 0)
		{
			idProp.stringValue = ""; // "<None>" selected
		} else if (newIndex != selectedIndex && !availableIDs[newIndex].EndsWith(" (Missing)"))
		{
			idProp.stringValue = availableIDs[newIndex];
		}

		// 7. Draw the Skill Points Int field
		float originalLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 25f; // Shrink the label width temporarily for the "Pts" text
		EditorGUI.PropertyField(pointsRect, pointsProp, new GUIContent("Pts"));
		EditorGUIUtility.labelWidth = originalLabelWidth;

		EditorGUI.EndProperty();
	}

	// Forces the drawer to only take up a single clean line in the inspector
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight;
	}
}
