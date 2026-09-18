using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(DialogueNodeLinkAttribute))]
public class DialogueNodeLinkDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.propertyType != SerializedPropertyType.String)
		{
			EditorGUI.PropertyField(position, property, label);
			return;
		}

		EditorGUI.BeginProperty(position, label, property);

		float buttonWidth = 90f;
		Rect stringRect = new Rect(position.x, position.y, position.width - buttonWidth - 5f, position.height);
		Rect buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, position.height);

		EditorGUI.PropertyField(stringRect, property, label);

		string targetID = property.stringValue;
		bool hasID = !string.IsNullOrWhiteSpace(targetID);

		GUI.enabled = hasID;
		if (GUI.Button(buttonRect, "Find/Create"))
		{
			// Capture the SerializedObject before we leave the GUI loop
			SerializedObject serializedObject = property.serializedObject;

			// THE FIX: Delay the execution until AFTER Unity finishes its current GUI frame.
			// This prevents Unity from overwriting our Expand/Collapse commands.
			EditorApplication.delayCall += () =>
			{
				JumpToOrCreateNode(serializedObject, targetID);
			};
		}
		GUI.enabled = true;

		EditorGUI.EndProperty();
	}

	private void JumpToOrCreateNode(SerializedObject serializedObject, string targetID)
	{
		// Guard clause in case the object was destroyed between the click and the delay call
		if (serializedObject == null || serializedObject.targetObject == null) return;

		serializedObject.Update();

		SerializedProperty nodesProperty = serializedObject.FindProperty("dialogueNodes");
		if (nodesProperty == null) return;

		bool found = false;
		int targetIndex = -1;

		for (int i = 0; i < nodesProperty.arraySize; i++)
		{
			SerializedProperty nodeProp = nodesProperty.GetArrayElementAtIndex(i);
			SerializedProperty idProp = nodeProp.FindPropertyRelative("nodeID");

			if (idProp != null && idProp.stringValue == targetID)
			{
				found = true;
				targetIndex = i;
				break;
			}
		}

		if (!found)
		{
			nodesProperty.arraySize++;
			targetIndex = nodesProperty.arraySize - 1;

			SerializedProperty newNodeProp = nodesProperty.GetArrayElementAtIndex(targetIndex);
			newNodeProp.FindPropertyRelative("nodeID").stringValue = targetID;

			newNodeProp.FindPropertyRelative("dialogueLines").arraySize = 0;
			newNodeProp.FindPropertyRelative("dialogueOptions").arraySize = 0;
			newNodeProp.FindPropertyRelative("nodeEvents").arraySize = 0;
			newNodeProp.FindPropertyRelative("requirements").arraySize = 0;

			Debug.Log($"[Dialogue System] Created missing node: '{targetID}'");
		}

		nodesProperty.isExpanded = true;

		// Collapse everything EXCEPT the target
		for (int i = 0; i < nodesProperty.arraySize; i++)
		{
			SerializedProperty nodeProp = nodesProperty.GetArrayElementAtIndex(i);
			nodeProp.isExpanded = (i == targetIndex);
		}

		serializedObject.ApplyModifiedProperties();

		// THE FIX 2: This is the strongest method in the Unity API to force 
		// the Inspector to throw away its UI cache and completely rebuild itself.
		ActiveEditorTracker.sharedTracker.ForceRebuild();
	}
}
