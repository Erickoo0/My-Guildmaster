using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles the dictionary database of QuestSo's
/// </summary>
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "QuestUI/QuestUI Database", order = 0)]
public class QuestDatabase : ScriptableObject
{
	[SerializeField] private List<QuestSo> questDatabase = new List<QuestSo>();

	private Dictionary<string, QuestSo> _questDictionary;

	public void SetupDictionary()
	{
		if (_questDictionary != null) return;

		_questDictionary = new Dictionary<string, QuestSo>();

		foreach (QuestSo quest in questDatabase)
		{
			if (quest == null)
			{
				Debug.LogWarning("[QuestDatabase] QuestSo is null!");
				continue;
			}

			if (!_questDictionary.TryAdd(quest.QuestID, quest))
			{
				Debug.LogWarning($"[QuestDatabase] Duplicate QuestUI ID found: {quest.QuestID}. IDs must be unique!");
			}
		}
	}

	public QuestSo GetQuestByID(string questID)
	{
		if (_questDictionary == null) SetupDictionary();

		if (_questDictionary.TryGetValue(questID, out QuestSo questData))
			return questData;

		Debug.LogWarning($"QuestUI Database: Could not find questID with ID '{questID}'");
		return null;
	}

    #if UNITY_EDITOR
	[ContextMenu("Refresh Database")]
	public void AutoPopulateDatabase()
	{
		questDatabase.Clear();

		// Find every asset of type QuestSo
		string[] guids = AssetDatabase.FindAssets($"t:{nameof(QuestSo)}");

		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			QuestSo quest = AssetDatabase.LoadAssetAtPath<QuestSo>(path);

			if (quest != null && !questDatabase.Contains(quest))
			{
				questDatabase.Add(quest);
			}
		}

		// Tell Unity we changed this file so it remembers to save the new list
		EditorUtility.SetDirty(this);
		Debug.Log($"[QuestDatabase] Successfully auto-populated {questDatabase.Count} quests.");
	}
    #endif
}
