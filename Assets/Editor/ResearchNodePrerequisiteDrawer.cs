using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Targets your specific prerequisite class
[CustomPropertyDrawer(typeof(ResearchNodePrerequisite))]
public class ResearchNodePrerequisiteDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		// 1. Fetch the serialized properties
		SerializedProperty idProp = property.FindPropertyRelative("_requiredResearchNodeID");

		// 2. Draw the main prefix label (e.g., "Element 0") and get the remaining space
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

		// 3. Search the parent ResearchTree object for all existing Node IDs
		List<string> availableIDs = new List<string>
		{
			"<None>"
		};

		// This targets the List<ResearchNode> _researchNodesList in ResearchTree
		SerializedProperty treeNodesProp = property.serializedObject.FindProperty("_researchNodesList");

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

		// 4. Determine which index is currently selected
		string currentID = idProp.stringValue;
		int selectedIndex = 0;

		if (!string.IsNullOrEmpty(currentID))
		{
			selectedIndex = availableIDs.IndexOf(currentID);
			if (selectedIndex == -1)
			{
				// Edge case: Preserves broken links visually if an ID is renamed/deleted
				availableIDs.Add(currentID + " (Missing)");
				selectedIndex = availableIDs.Count - 1;
			}
		}

		// 5. Draw the Dropdown Popup in the remaining Rect space
		int newIndex = EditorGUI.Popup(position, selectedIndex, availableIDs.ToArray());

		if (newIndex == 0)
		{
			idProp.stringValue = ""; // "<None>" selected
		} else if (newIndex != selectedIndex && !availableIDs[newIndex].EndsWith(" (Missing)"))
		{
			idProp.stringValue = availableIDs[newIndex];
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight;
	}
}
