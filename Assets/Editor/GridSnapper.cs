#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public class GridSnapper : MonoBehaviour
{
	// Starting the path with "GameObject/" puts it in the right-click menu.
	// The '0' priority puts it near the very top of the list for easy access.
	[MenuItem("GameObject/Snap To Placement Grid", false, 0)]
	public static void SnapSelectedObjects()
	{
		// Safety check
		if (Selection.gameObjects.Length == 0) return;

		int count = 0;

		foreach (GameObject obj in Selection.gameObjects)
		{
			Undo.RecordObject(obj.transform, "Snap to Grid");

			Vector3 pos = obj.transform.position;

			// Applies your specific PlacementManager math (X on the half, Y on the whole)
			pos.x = Mathf.Floor(pos.x) + 0.5f;
			pos.y = Mathf.Round(pos.y);

			obj.transform.position = pos;
			count++;
		}

		Debug.Log($"Successfully snapped {count} objects!");
	}
}
#endif
