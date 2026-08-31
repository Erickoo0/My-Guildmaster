#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class BatchPrefabVariantMaker : EditorWindow
{
	private GameObject basePrefab;

	private void OnGUI()
	{
		GUILayout.Label("1. Assign your Base Prefab:", EditorStyles.boldLabel);
		basePrefab = (GameObject)EditorGUILayout.ObjectField(basePrefab, typeof(GameObject), false);

		GUILayout.Space(10);
		if (GUILayout.Button("Convert Selected to Variants"))
		{
			if (basePrefab == null) return;
			ConvertSelectedToVariants();
		}
	}

	[MenuItem("Tools/Batch Convert to Variants")]
	public static void ShowWindow()
	{
		GetWindow<BatchPrefabVariantMaker>("Make Variants");
	}

	private void ConvertSelectedToVariants()
	{
		GameObject[] selectedObjects = Selection.gameObjects;
		int count = 0;

		// 1. MEMORIZE POSITIONS: Find all instances in the open scene
		Dictionary<GameObject, TransformData> sceneInstances = new Dictionary<GameObject, TransformData>();
		GameObject[] allSceneObjects = FindObjectsOfType<GameObject>(true);

		foreach (GameObject sceneObj in allSceneObjects)
		{
			if (PrefabUtility.IsPartOfPrefabInstance(sceneObj))
			{
				GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(sceneObj);

				// If this scene object is one of the prefabs we are about to change
				if (Array.Exists(selectedObjects, element => element == prefabAsset))
				{
					// Only save the root of the prefab
					if (PrefabUtility.GetOutermostPrefabInstanceRoot(sceneObj) == sceneObj)
					{
						sceneInstances.Add(sceneObj, new TransformData
						{
							position = sceneObj.transform.position,
							rotation = sceneObj.transform.rotation,
							localScale = sceneObj.transform.localScale
						});
					}
				}
			}
		}

		// 2. CONVERT TO VARIANTS
		foreach (GameObject obj in selectedObjects)
		{
			if (obj == basePrefab) continue;

			if (PrefabUtility.IsPartOfPrefabAsset(obj))
			{
				string assetPath = AssetDatabase.GetAssetPath(obj);

				GameObject variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
				GameObject oldInstance = (GameObject)PrefabUtility.InstantiatePrefab(obj);

				SpriteRenderer oldSprite = oldInstance.GetComponentInChildren<SpriteRenderer>();
				if (oldSprite != null)
				{
					SpriteRenderer variantSprite = variantInstance.GetComponentInChildren<SpriteRenderer>();
					if (variantSprite == null) variantSprite = variantInstance.AddComponent<SpriteRenderer>();

					variantSprite.sprite = oldSprite.sprite;
					variantSprite.color = oldSprite.color;
					variantSprite.flipX = oldSprite.flipX;
					variantSprite.flipY = oldSprite.flipY;
				}

				PrefabUtility.SaveAsPrefabAsset(variantInstance, assetPath);

				DestroyImmediate(variantInstance);
				DestroyImmediate(oldInstance);
				count++;
			}
		}

		// 3. RESTORE POSITIONS: Re-apply the memorized transforms to the scene objects
		foreach (var kvp in sceneInstances)
		{
			if (kvp.Key != null)
			{
				kvp.Key.transform.position = kvp.Value.position;
				kvp.Key.transform.rotation = kvp.Value.rotation;
				kvp.Key.transform.localScale = kvp.Value.localScale;

				// Tell Unity to save these restored positions as intentional Scene Overrides
				PrefabUtility.RecordPrefabInstancePropertyModifications(kvp.Key.transform);
			}
		}

		AssetDatabase.SaveAssets();
		Debug.Log($"Converted {count} prefabs and successfully restored positions for {sceneInstances.Count} scene objects!");
	}

	// Struct to hold our scene overrides
	struct TransformData
	{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 localScale;
	}
}
#endif
