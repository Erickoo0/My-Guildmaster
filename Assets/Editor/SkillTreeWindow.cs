using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class SkillTreeWindow : EditorWindow
{
	private readonly Vector2 CANVAS_SIZE = new Vector2(1300f, 585f);
	private readonly float GRID_SNAP = 10f;
	// --- UI DIMENSION SETTINGS ---
	private readonly Vector2 NODE_SIZE = new Vector2(60f, 60f);
	// -----------------------------

	private SkillTree _currentTree;

	private int _draggingIndex = -1;
	private Vector2 _dragLocalOffset;
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
			EditorGUILayout.HelpBox("Select a Skill Tree ScriptableObject in the Project window to edit it.", MessageType.Warning);
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
		DrawNodes(); // Updated method

		// Draw HUD over top (unaffected by zoom)
		GUI.Label(new Rect(10, 10, 400, 20), $"Currently Editing: {_currentTree.name}", EditorStyles.boldLabel);
		GUI.Label(new Rect(10, 30, 400, 20), $"Zoom: {Mathf.RoundToInt(_zoom*100)}% (Scroll to zoom, Middle-click/Alt to pan)");

		_serializedTree.ApplyModifiedProperties();
	}

	[MenuItem("Window/Skill Tree Visualizer")]
	public static void ShowWindow()
	{
		GetWindow<SkillTreeWindow>("Skill Tree Editor");
	}

	private void OnSelectionChanged()
	{
		if (Selection.activeObject is SkillTree tree)
		{
			_currentTree = tree;
			_serializedTree = new SerializedObject(_currentTree);
			_nodesProp = _serializedTree.FindProperty("_skillNodes");
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
		Vector2 topLeftLocal = new Vector2(-CANVAS_SIZE.x/2f, -CANVAS_SIZE.y/2f);
		Vector2 screenTopLeft = LocalToScreen(topLeftLocal);
		Vector2 scaledSize = CANVAS_SIZE*_zoom;

		Rect bgRect = new Rect(screenTopLeft.x, screenTopLeft.y, scaledSize.x, scaledSize.y);

		EditorGUI.DrawRect(bgRect, new Color(0.15f, 0.15f, 0.15f, 1f));

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
			Vector2 pos = node.FindPropertyRelative("_uiPosition").vector2Value;
			nodePositions[id] = pos;
		}

		Handles.color = Color.cyan;

		for (int i = 0; i < _nodesProp.arraySize; i++)
		{
			SerializedProperty node = _nodesProp.GetArrayElementAtIndex(i);
			Vector2 pos = node.FindPropertyRelative("_uiPosition").vector2Value;
			SerializedProperty prereqs = node.FindPropertyRelative("_prerequisites");

			Vector2 myScreenCenter = LocalToScreen(new Vector2(pos.x, -pos.y));

			for (int j = 0; j < prereqs.arraySize; j++)
			{
				string reqID = prereqs.GetArrayElementAtIndex(j).FindPropertyRelative("_requiredSkillNodeID").stringValue;

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

		// Custom style specifically for the text (No background, forced white, bold)
		GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel)
		{
			alignment = TextAnchor.MiddleCenter,
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
			SerializedProperty posProp = node.FindPropertyRelative("_uiPosition");
			SerializedProperty idProp = node.FindPropertyRelative("_id");

			Vector2 pos = posProp.vector2Value;
			Vector2 screenCenter = LocalToScreen(new Vector2(pos.x, -pos.y));

			// The exact pixel-perfect rect for the node
			Rect nodeRect = new Rect(screenCenter.x - (scaledSize.x/2f), screenCenter.y - (scaledSize.y/2f), scaledSize.x, scaledSize.y);

			// The slightly larger rect for the border outline
			Rect borderRect = new Rect(nodeRect.x - 2, nodeRect.y - 2, nodeRect.width + 4, nodeRect.height + 4);

			// 1. Draw Visual Boxes based on state
			if (_draggingIndex == i)
			{
				// Dragging: Gold outline, Lighter Blue fill
				EditorGUI.DrawRect(borderRect, new Color(1.0f, 0.7f, 0.2f, 1f));
				EditorGUI.DrawRect(nodeRect, new Color(0.35f, 0.45f, 0.6f, 1f));
			} else
			{
				// Normal: Black outline, Slate Blue fill
				EditorGUI.DrawRect(borderRect, new Color(0.05f, 0.05f, 0.05f, 1f));
				EditorGUI.DrawRect(nodeRect, new Color(0.25f, 0.3f, 0.4f, 1f));
			}

			// 2. Draw the Text over the top
			string displayName = string.IsNullOrWhiteSpace(idProp.stringValue) ? "Unnamed" : idProp.stringValue;
			GUI.Label(nodeRect, displayName, textStyle);
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
					Vector2 pos = node.FindPropertyRelative("_uiPosition").vector2Value;

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
				SerializedProperty posProp = node.FindPropertyRelative("_uiPosition");

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
