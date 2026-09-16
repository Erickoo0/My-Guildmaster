using TMPro;
using UnityEngine;
/// <summary>
/// Handles the UI of a QuestActive
/// </summary>
public class QuestUI : MonoBehaviour
{
	[Header("Component References")]
	[SerializeField] private TextMeshProUGUI questName;
	[SerializeField] private TextMeshProUGUI questDescription;
	[SerializeField] private TextMeshProUGUI questProgress;

	public string QuestID { get; private set; }

	public string QuestName => questName.text;
	public string QuestDescription => questDescription.text;
	public string QuestProgress => questProgress.text;

	public void Setup(QuestActive quest)
	{
		QuestID = quest.QuestData.QuestID;
		questName.text = quest.QuestData.QuestName;
		questDescription.text = quest.QuestData.QuestDescription;

		UpdateProgressText(quest);
	}

	public void UpdateProgressText(QuestActive quest)
	{
		// Clear the text
		questProgress.text = "";

		// Loop through all objectives and write them out
		for (int i = 0; i < quest.QuestData.QuestObjectives.Count; i++)
		{
			QuestObjectiveBase objectiveData = quest.QuestData.QuestObjectives[i];
			int currentProgress = quest.ObjectiveProgress[i];

			// 1. Handle Count-Based Objectives
			if (objectiveData.IsCountBased)
				questProgress.text += $"- {objectiveData.ObjectiveTitle}: {currentProgress} / {objectiveData.RequiredAmount}\n";

			// 2. Handle State-Based Objectives
			else if (objectiveData is QuestObjectiveState stateObjective)
			{
				// 3. Check if the objective is a State-Based Objective
				if (stateObjective.requirement is RequirementGameStat statRequirement)
				{
					int liveStatValue = GameFlagManager.Instance.GetGameStat(statRequirement.requiredGameStat);
					questProgress.text += $"- {objectiveData.ObjectiveTitle}: {liveStatValue} / {statRequirement.requiredMinimumValue}\n";
				} else // 4. Check if its tracking a GameFlag or something else
				{
					string status = stateObjective.IsConditionMet() ? "Finished" : "InComplete";
					questProgress.text += $"- {objectiveData.ObjectiveTitle}: {status}\n";
				}
			}
		}

		if (quest.IsCompleted)
		{
			questProgress.text = "<color=green>Ready to Turn In!</color>";
		}
	}
}
