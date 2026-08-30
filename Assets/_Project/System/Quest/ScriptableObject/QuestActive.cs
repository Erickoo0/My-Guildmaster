using System;
using UnityEngine;
[Serializable]
public class QuestActive
{

	// Constructor that setups the quest
	public QuestActive(QuestSo questData)
	{
		QuestData = questData;
		// Setup the objective progress array with the required Amount for each objective
		ObjectiveProgress = new int[QuestData.QuestObjectives.Count];
		IsCompleted = false;
	}

	// Constructor that loads a quest
	public QuestActive(QuestSo questData, int[] objectiveProgress, bool isCompleted)
	{
		QuestData = questData;
		ObjectiveProgress = new int[QuestData.QuestObjectives.Count];

		if (objectiveProgress != null)
		{
			// Set the objective progress to the loaded progress, but not more than the required Amount
			int count = Mathf.Min(objectiveProgress.Length, ObjectiveProgress.Length);
			for (int i = 0; i < count; i++)
				ObjectiveProgress[i] = objectiveProgress[i];
		}

		IsCompleted = isCompleted;
	}
	public QuestSo QuestData { get; private set; }
	public int[] ObjectiveProgress { get; private set; }
	public bool IsCompleted { get; private set; }

	public void CheckQuestCompletion()
	{
		if (IsCompleted) return; // Skip if the quest is already completed

		int completedCount = 0; // Counts how many objectives are completed

		// Loop through every quest objective
		for (int i = 0; i < QuestData.QuestObjectives.Count; i++)
		{
			// Instance of quest objective
			QuestObjectiveBase objective = QuestData.QuestObjectives[i];

			// If the objective is State-Based, check if the condition is met
			if (!objective.IsCountBased)
			{
				ObjectiveProgress[i] = objective.IsConditionMet() ? 1 : 0;
				if (ObjectiveProgress[i] == 1) completedCount++;
			} else if (objective.IsCountBased)
				if (ObjectiveProgress[i] >= objective.RequiredAmount)
					completedCount++;
		}

		// If all objectives are completed, mark the quest as completed
		if (completedCount >= QuestData.QuestObjectives.Count)
		{
			IsCompleted = true;
			Debug.Log($"Quest {QuestData.QuestName}: Completed!");
		}
	}

	// Method to add progress to an objective (called by Source -> EventBus -> QuestManager)
	public void AddObjectiveProgress(int objectiveIndex, int progress)
	{
		// Only increment count-based objectives
		if (QuestData.QuestObjectives[objectiveIndex].IsCountBased)
			ObjectiveProgress[objectiveIndex] += progress;

		CheckQuestCompletion();
	}
}
