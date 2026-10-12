using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class ResearchTreeWindow : EditorWindow
{

	private readonly Vector2 CANVAS_SIZE = new Vector2(1300f, 585f);
	private readonly float GRID_SNAP = 10f;
	// --- UI DIMENSION SETTINGS ---
	private readonly Vector2 NODE_SIZE = new Vector2(40f, 40f); // Widened slightly to fit Research names
	private GUIStyle _bgStyle;
	// -----------------------------

	private ResearchTree _currentTree;
	private Vector2 _dragLocalOffset;

	private int _draggingIndex = -1;
	private bool _isInitialized = false;
	private SerializedProperty _nodesProp;

	// Navigation & Interaction states
	private Vector2 _panOffset;
	private SerializedObject _serializedTree;
	private float _zoom = 1f;

	private void OnEnable()
	{
		Selection.selectionChanged += OnSelectionChanged;
		OnSelectionChanged();
	}

	private void OnDisable()
	{
		Selection.selectionChanged -= OnSelectionChanged;
	}

	private void OnGUI()
	{
		if (_currentTree == null || _serializedTree == null)
		{
			EditorGUILayout.HelpBox("Select a Research Tree ScriptableObject in the Project window to edit it.", MessageType.Warning);
			return;
		}

		_serializedTree.Update();

		if (!_isInitialized)
		{
			_panOffset = new Vector2(position.width/2f, position.height/2f);
			_isInitialized = true;
		}

		Event e = Event.current;
		HandleCanvasNavigation(e);

		DrawCanvasBoundary();
		DrawPrerequisiteLines();
		HandleEvents(e);
		DrawNodes();

		// Draw HUD over top (unaffected by zoom)
		GUI.Label(new Rect(10, 10, 400, 20), $"Currently Editing: {_currentTree.name}", EditorStyles.boldLabel);
		GUI.Label(new Rect(10, 30, 400, 20), $"Zoom: {Mathf.RoundToInt(_zoom*100)}% (Scroll to zoom, Middle-click/Alt to pan)");

		_serializedTree.ApplyModifiedProperties();
	}

	[MenuItem("Window/Research Tree Visualizer")]
	public static void ShowWindow()
	{
		GetWindow<ResearchTreeWindow>("Research Tree Editor");
	}

	private void OnSelectionChanged()
	{
		if (Selection.activeObject is ResearchTree tree)
		{
			_currentTree = tree;
			_serializedTree = new SerializedObject(_currentTree);
			_nodesProp = _serializedTree.FindProperty("_researchNodesList");
			Repaint();
		}
	}

	// --- COORDINATE MATH TRANSLATORS ---
	private Vector2 LocalToScreen(Vector2 localPosition) => (localPosition*_zoom) + _panOffset;
	private Vector2 ScreenToLocal(Vector2 screenPosition) => (screenPosition - _panOffset)/_zoom;

	private void HandleCanvasNavigation(Event e)
	{
		if (e.type == EventType.ScrollWheel)
		{
			float oldZoom = _zoom;
			_zoom -= e.delta.y*0.05f;
			_zoom = Mathf.Clamp(_zoom, 0.2f, 3f);

			Vector2 mousePos = e.mousePosition;
			Vector2 localPos = (mousePos - _panOffset)/oldZoom;
			_panOffset = mousePos - (localPos*_zoom);

			e.Use();
			Repaint();
		}

		if (e.type == EventType.MouseDrag && (e.button == 2 || (e.button == 0 && e.alt)))
		{
			_panOffset += e.delta;
			e.Use();
			Repaint();
		}
	}

	private void DrawCanvasBoundary()
	{
		// 1. Use the manual dimensions you set in the ScriptableObject
		Vector2 currentCanvasSize = _currentTree != null ? _currentTree.BackgroundDimensions : CANVAS_SIZE;

		// 2. Calculate the screen rect with zoom and pan applied
		Vector2 topLeftLocal = new Vector2(-currentCanvasSize.x/2f, -currentCanvasSize.y/2f);
		Vector2 screenTopLeft = LocalToScreen(topLeftLocal);
		Vector2 scaledSize = currentCanvasSize*_zoom;

		Rect bgRect = new Rect(screenTopLeft.x, screenTopLeft.y, scaledSize.x, scaledSize.y);

		// 3. Draw a solid dark background first (in case your UI sprite is partially transparent)
		EditorGUI.DrawRect(bgRect, new Color(0.15f, 0.15f, 0.15f, 1f));

		// 4. Draw the 9-Sliced Sprite
		if (_currentTree != null && _currentTree.BackgroundImage != null)
		{
			Sprite sprite = _currentTree.BackgroundImage;

			// Rebuild the GUIStyle if it hasn't been created or if you swapped the sprite
			if (_bgStyle == null || _bgStyle.normal.background != sprite.texture)
			{
				_bgStyle = new GUIStyle();
				_bgStyle.normal.background = sprite.texture;

				// Map the Sprite's 9-slice border (X=Left, Y=Bottom, Z=Right, W=Top) 
				// into the Editor GUI RectOffset format (Left, Right, Top, Bottom)
				_bgStyle.border = new RectOffset(
					Mathf.RoundToInt(sprite.border.x),
					Mathf.RoundToInt(sprite.border.z),
					Mathf.RoundToInt(sprite.border.w),
					Mathf.RoundToInt(sprite.border.y)
					);
			}

			// GUI.Box uses the GUIStyle borders to 9-slice the texture automatically
			GUI.Box(bgRect, GUIContent.none, _bgStyle);
		}

		// 5. Draw the center axis lines
		Vector2 screenCenter = LocalToScreen(Vector2.zero);
		Handles.color = new Color(1f, 1f, 1f, 0.15f);
		Handles.DrawLine(new Vector2(bgRect.x, screenCenter.y), new Vector2(bgRect.xMax, screenCenter.y));
		Handles.DrawLine(new Vector2(screenCenter.x, bgRect.y), new Vector2(screenCenter.x, bgRect.yMax));
		Handles.color = Color.white;
	}

	private void DrawPrerequisiteLines()
	{
		if (_nodesProp == null) return;

		Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();
		for (int i = 0; i < _nodesProp.arraySize; i++)
		{
			SerializedProperty node = _nodesProp.GetArrayElementAtIndex(i);
			string id = node.FindPropertyRelative("_id").stringValue;
			Vector2 pos = node.FindPropertyRelative("_position").vector2Value;
			nodePositions[id] = pos;
		}

		Handles.color = Color.cyan;

		for (int i = 0; i < _nodesProp.arraySize; i++)
		{
			SerializedProperty node = _nodesProp.GetArrayElementAtIndex(i);
			Vector2 pos = node.FindPropertyRelative("_position").vector2Value;
			SerializedProperty prereqs = node.FindPropertyRelative("_prerequisitesList");

			Vector2 myScreenCenter = LocalToScreen(new Vector2(pos.x, -pos.y));

			for (int j = 0; j < prereqs.arraySize; j++)
			{
				string reqID = prereqs.GetArrayElementAtIndex(j).FindPropertyRelative("_requiredResearchNodeID").stringValue;

				if (nodePositions.TryGetValue(reqID, out Vector2 reqPos))
				{
					Vector2 reqScreenCenter = LocalToScreen(new Vector2(reqPos.x, -reqPos.y));
					Handles.DrawDottedLine(myScreenCenter, reqScreenCenter, 4f);
				}
			}
		}
		Handles.color = Color.white;
	}

	private void DrawNodes()
	{
		if (_nodesProp == null) return;

		// Notice alignment is now UpperCenter so it hangs top-down below the node
		GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel)
		{
			alignment = TextAnchor.UpperCenter,
			wordWrap = true,
			fontSize = Mathf.Max(8, Mathf.RoundToInt(11*_zoom)),
			normal =
			{
				textColor = Color.white
			}
		};

		Vector2 scaledSize = NODE_SIZE*_zoom;

		for (int i = 0; i < _nodesProp.arraySize; i++)
		{
			SerializedProperty node = _nodesProp.GetArrayElementAtIndex(i);
			SerializedProperty posProp = node.FindPropertyRelative("_position");
			SerializedProperty idProp = node.FindPropertyRelative("_id");
			SerializedProperty iconProp = node.FindPropertyRelative("_icon");
			SerializedProperty nameProp = node.FindPropertyRelative("_displayName");

			Vector2 pos = posProp.vector2Value;
			Vector2 screenCenter = LocalToScreen(new Vector2(pos.x, -pos.y));

			Rect nodeRect = new Rect(screenCenter.x - (scaledSize.x/2f), screenCenter.y - (scaledSize.y/2f), scaledSize.x, scaledSize.y);
			Rect borderRect = new Rect(nodeRect.x - 2, nodeRect.y - 2, nodeRect.width + 4, nodeRect.height + 4);

			// 1. Draw the Background Boxes
			if (_draggingIndex == i)
			{
				EditorGUI.DrawRect(borderRect, new Color(1.0f, 0.7f, 0.2f, 1f));
				EditorGUI.DrawRect(nodeRect, new Color(0.35f, 0.45f, 0.6f, 1f));
			} else
			{
				EditorGUI.DrawRect(borderRect, new Color(0.05f, 0.05f, 0.05f, 1f));
				EditorGUI.DrawRect(nodeRect, new Color(0.25f, 0.3f, 0.4f, 1f));
			}

			// 2. Draw the Icon Sprite (now fills the box with a small padding margin)
			Sprite iconSprite = iconProp.objectReferenceValue as Sprite;
			if (iconSprite != null && iconSprite.texture != null)
			{
				float padding = 6f*_zoom;
				Rect iconRect = new Rect(
					nodeRect.x + padding,
					nodeRect.y + padding,
					nodeRect.width - (padding*2),
					nodeRect.height - (padding*2)
					);

				Rect texCoords = new Rect(
					iconSprite.rect.x/iconSprite.texture.width,
					iconSprite.rect.y/iconSprite.texture.height,
					iconSprite.rect.width/iconSprite.texture.width,
					iconSprite.rect.height/iconSprite.texture.height
					);

				GUI.DrawTextureWithTexCoords(iconRect, iconSprite.texture, texCoords);
			}

			// 3. Draw the Display Name below the node
			string displayName = string.IsNullOrWhiteSpace(nameProp.stringValue)
				? (string.IsNullOrWhiteSpace(idProp.stringValue) ? "Unnamed" : idProp.stringValue)
				: nameProp.stringValue;

			// Force the text width to be much wider than the node itself (e.g. 140px base width)
			float textWidth = 140f*_zoom;

			// Start the Y position just below the bottom of the node (nodeRect.yMax) plus a 5px gap
			Rect textRect = new Rect(
				screenCenter.x - (textWidth/2f),
				nodeRect.yMax + (5f*_zoom),
				textWidth,
				40f*_zoom
				);

			// Optionally add a subtle shadow for better readability over complex backgrounds
			GUIStyle shadowStyle = new GUIStyle(textStyle);
			shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.7f);
			Rect shadowRect = new Rect(textRect.x + 1, textRect.y + 1, textRect.width, textRect.height);
			GUI.Label(shadowRect, displayName, shadowStyle);

			// Draw main text
			GUI.Label(textRect, displayName, textStyle);
		}
	}

	private void HandleEvents(Event e)
	{
		if (_nodesProp == null) return;

		Vector2 scaledSize = NODE_SIZE*_zoom;

		switch (e.type)
		{
		case EventType.MouseDown:
			if (e.button == 0 && !e.alt)
			{
				for (int i = _nodesProp.arraySize - 1; i >= 0; i--)
				{
					SerializedProperty node = _nodesProp.GetArrayElementAtIndex(i);
					Vector2 pos = node.FindPropertyRelative("_position").vector2Value;

					Vector2 screenCenter = LocalToScreen(new Vector2(pos.x, -pos.y));
					Rect nodeRect = new Rect(screenCenter.x - (scaledSize.x/2f), screenCenter.y - (scaledSize.y/2f), scaledSize.x, scaledSize.y);

					if (nodeRect.Contains(e.mousePosition))
					{
						_draggingIndex = i;
						_dragLocalOffset = ScreenToLocal(e.mousePosition) - new Vector2(pos.x, -pos.y);
						e.Use();
						break;
					}
				}
			}
			break;

		case EventType.MouseDrag:
			if (_draggingIndex != -1)
			{
				SerializedProperty node = _nodesProp.GetArrayElementAtIndex(_draggingIndex);
				SerializedProperty posProp = node.FindPropertyRelative("_position");

				Vector2 targetLocalCenter = ScreenToLocal(e.mousePosition) - _dragLocalOffset;

				posProp.vector2Value = new Vector2(
					Mathf.Round(targetLocalCenter.x/GRID_SNAP)*GRID_SNAP,
					Mathf.Round(-targetLocalCenter.y/GRID_SNAP)*GRID_SNAP
					);

				e.Use();
				Repaint();
			}
			break;

		case EventType.MouseUp:
			if (e.button == 0 && _draggingIndex != -1)
			{
				_draggingIndex = -1;
				e.Use();
			}
			break;
		}
	}
}
