using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class QuestManager : MonoBehaviour, ISaveable
{

	//[SerializeField] private QuestUI questUI;
	[Header("Databases")]
	[SerializeField] private QuestDatabase questDatabase;

	// 0(1) Lookup Data Structure
	private Dictionary<string, QuestActive> _activeQuestDictionary = new Dictionary<string, QuestActive>();
	private HashSet<string> _completedQuestHashSet = new HashSet<string>();
	public static QuestManager Instance { get; private set; }
	public IReadOnlyDictionary<string, QuestActive> ActiveQuestDictionary => _activeQuestDictionary;
	public IReadOnlyCollection<string> CompletedQuestHashSet => _completedQuestHashSet;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			Debug.unityLogger.Log("Multiple QuestManagers detected. Disabling script.");
			return;
		}

		Instance = this;

		if (questDatabase != null) questDatabase.SetupDictionary();
	}

	private void OnEnable()
	{
		EventBus.OnUpdateQuestObjectiveRequested += HandleObjectiveUpdate;
		EventBus.OnEntityDeathRequested += HandleEntityDeath;

		EventBus.OnGameFlagChanged += HandleFlagAndStateObjective;
		EventBus.OnGameStatChanged += HandleFlagAndStateObjective;
		EventBus.OnWorldTimeChanged += HandleTimeObjective;
	}

	private void OnDisable()
	{
		EventBus.OnUpdateQuestObjectiveRequested -= HandleObjectiveUpdate;
		EventBus.OnEntityDeathRequested -= HandleEntityDeath;

		EventBus.OnGameFlagChanged -= HandleFlagAndStateObjective;
		EventBus.OnGameStatChanged -= HandleFlagAndStateObjective;
		EventBus.OnWorldTimeChanged -= HandleTimeObjective;
	}

	//----Save Methods----
	public void PopulateSaveData(SaveData saveData)
	{
		// Reset the SavedQuestsList list
		saveData.SavedQuestsList.Clear();

		// Loop through all active quests
		foreach (QuestActive questActive in _activeQuestDictionary.Values)
		{
			// Safety Check
			if (questActive == null || questActive.QuestData == null) continue;

			// Create a new SavedQuest object 
			SavedQuest savedQuest = new SavedQuest
			{
				questID = questActive.QuestData.QuestID,
				objectiveProgress = questActive.ObjectiveProgress,
				isCompleted = questActive.IsCompleted
			};

			// Add the SavedQuest to the list
			saveData.SavedQuestsList.Add(savedQuest);
		}

		// Add the completed quest list to the save data
		saveData.CompletedQuestsList = _completedQuestHashSet.ToList();
	}

	public void LoadFromSaveData(SaveData saveData)
	{
		// Reset the SavedQuestsList list just incase
		_activeQuestDictionary.Clear();
		_completedQuestHashSet.Clear();

		// Load completed quest
		if (saveData.CompletedQuestsList != null)
			foreach (string completedQuestID in saveData.CompletedQuestsList)
				_completedQuestHashSet.Add(completedQuestID);

		// Load active quests
		foreach (SavedQuest savedQuest in saveData.SavedQuestsList)
		{
			QuestSo questData = questDatabase.GetQuestByID(savedQuest.questID);
			if (questData != null)
			{
				QuestActive questActive = new QuestActive(questData, savedQuest.objectiveProgress, savedQuest.isCompleted);
				_activeQuestDictionary.Add(questActive.QuestData.QuestID, questActive);
			} else
				Debug.LogWarning($"[QuestManager] Could not find quest with ID: {savedQuest.questID} in database.");

			EventBus.RequestUpdateQuest();
		}
	}

	private void HandleEntityDeath(GameObject entityRoot)
	{
		if (entityRoot.TryGetComponent(out ControllerBase entityController))
		{
			string targetID = entityController.GetTargetID();

			if (!string.IsNullOrEmpty(targetID))
				HandleObjectiveUpdate(targetID, 1);
		}
	}

	private void HandleObjectiveUpdate(string targetID, int amount)
	{
		bool questUpdated = false;

		foreach (QuestActive questActive in _activeQuestDictionary.Values)
		{
			if (questActive.IsCompleted) continue; // Skip the quest if its already completed

			// Look through every objective in the quest
			for (int i = 0; i < questActive.QuestData.QuestObjectives.Count; i++)
			{
				QuestObjectiveBase objective = questActive.QuestData.QuestObjectives[i];

				// Check if the objective is count-based & has same targetID as the objective
				if (objective.IsCountBased && objective.TargetID == targetID)
				{
					//questUI.UpdateQuestUI(questActive);
					questActive.AddObjectiveProgress(i, amount);
					questUpdated = true;
				}
			}
		}
		// Signal the event once per update batch
		if (questUpdated) EventBus.RequestUpdateQuest();
	}

	// Triggered automatically whenever a Game Flag or Game Stat updates via the EventBus
	private void HandleFlagAndStateObjective(FlagKeys.GameFlag flag, bool state) => UpdateFlagAndStateObjectives();
	private void HandleFlagAndStateObjective(FlagKeys.GameStat stat, int value) => UpdateFlagAndStateObjectives();
	// Triggered automatically whenever World Time moves forward on the EventBus
	private void HandleTimeObjective(object sender, TimeSpan currentTime) => UpdateFlagAndStateObjectives();

	private void UpdateFlagAndStateObjectives()
	{
		//bool anyQuestUpdated = false;

		foreach (QuestActive questActive in _activeQuestDictionary.Values)
			if (!questActive.IsCompleted)
			{
				int completedStatusBeforeCheck = questActive.IsCompleted ? 1 : 0;
				questActive.CheckQuestCompletion();
				int completedStatusAfterCheck = questActive.IsCompleted ? 1 : 0;

				// If the quest status changed, Send an event
				if (completedStatusBeforeCheck != completedStatusAfterCheck)
				{
					//anyQuestUpdated = true;
				}
			}

		EventBus.RequestUpdateQuest();
	}

	public void AcceptQuest(object questData)
	{
		// 1. Pattern Match: Try to treat questData as a string. 
		// If it is a string, assign it to the variable 'questID'.
		if (questData is string questID)
		{
			// Check if we already have or completed this quest
			if (_activeQuestDictionary.ContainsKey(questID) || _completedQuestHashSet.Contains(questID)) return;

			// Look it up in the database
			QuestSo questDataSo = questDatabase.GetQuestByID(questID);
			if (questDataSo != null)
			{
				QuestActive newQuest = new QuestActive(questDataSo);
				_activeQuestDictionary.Add(questID, newQuest);
				newQuest.CheckQuestCompletion();
				//questUI.AddQuestUI(_questList[^1]); // [^1] is shorthand for 'last index'

				// Scan Inventory to immediately update item related quest progress
				for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
					HandleObjectiveUpdate(ItemStoragePlayer.Instance.GetItem(i)?.DataSo.ItemID, ItemStoragePlayer.Instance.GetItem(i)?.stackSize ?? 0);

				EventBus.RequestUpdateQuest();
			} else
			{
				Debug.LogError($"Quest ID {questID} not found in database!");
			}
		}
	}

	public void CompleteQuest(object questData)
	{
		// 1. Pattern Match
		if (questData is string questID)
		{
			// 3. Find the active quest in the current quest list
			if (_activeQuestDictionary.TryGetValue(questID, out QuestActive quest))
			{
				if (quest.IsCompleted)
				{
					// 5. Remove the quest from the active quest list and add it to the completed quest list
					_activeQuestDictionary.Remove(questID);
					_completedQuestHashSet.Add(questID);
					EventBus.RequestUpdateQuest();
				} else Debug.LogWarning($"Attempted to complete quest {questID}, but it is not completed.");
			}

			//else Debug.LogWarning($"Attempted to turn in quest {questID}, but it is either not active or not completed.");
		}
	}

	public bool TryGetActiveQuest(string questID, out QuestActive questActive)
	{
		return _activeQuestDictionary.TryGetValue(questID, out questActive);
	}

	public bool CheckQuestCompletion(string questID)
	{
		return _completedQuestHashSet.Contains(questID);
	}
}
