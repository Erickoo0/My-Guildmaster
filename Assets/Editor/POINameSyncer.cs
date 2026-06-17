using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class POINameSyncer
{
	// InitializeOnLoad ensures this registers as soon as Unity opens
	static POINameSyncer()
	{
		EditorApplication.hierarchyChanged += OnHierarchyChanged;
	}

	private static void OnHierarchyChanged()
	{
		// Find every PointOfInterest currently loaded in the scene
		PointOfInterest[] pois = Object.FindObjectsByType<PointOfInterest>(FindObjectsSortMode.None);

		foreach (var poi in pois)
		{
			// If the ID doesn't match the GameObject name, update it
			if (poi.ID != poi.gameObject.name)
			{
				// Record the object state so Ctrl+Z (Undo) still works perfectly
				Undo.RecordObject(poi, "Sync POI ID with Gameobject Name");
                
				poi.ID = poi.gameObject.name;

				// Tell Unity that the data changed so it saves the scene correctly
				PrefabUtility.RecordPrefabInstancePropertyModifications(poi);
			}
		}
	}
}
