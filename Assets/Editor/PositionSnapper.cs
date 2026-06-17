using UnityEngine;
using UnityEditor;

public class PositionSnapper : Editor
{
	// This creates a custom menu option at the top of Unity
	[MenuItem("Tools/Force Snap Selection")] 
	public static void SnapSelectedObjects()
	{
		// Set this to your grid size. 
		// Use 1.0f if 1 Unity Unit = 1 Tile.
		float gridUnit = 1.0f; 
		int snappedCount = 0;

		foreach (GameObject obj in Selection.gameObjects)
		{
			// This lets you press Cmd+Z to undo if anything goes wrong
			Undo.RecordObject(obj.transform, "Force Snap to Grid");

			Vector3 currentPos = obj.transform.position;
            
			// Mathematically round the float positions to the nearest whole grid unit
			currentPos.x = Mathf.Round(currentPos.x / gridUnit) * gridUnit;
			currentPos.y = Mathf.Round(currentPos.y / gridUnit) * gridUnit;
			currentPos.z = Mathf.Round(currentPos.z / gridUnit) * gridUnit;

			obj.transform.position = currentPos;
			snappedCount++;
		}

		Debug.Log($"Successfully force-snapped {snappedCount} objects to the {gridUnit} unit grid!");
	}
}
