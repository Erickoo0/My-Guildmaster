using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Represents an NPC in the world. Handles routing the correct
/// dialogue and speech bubble based on the current state and daily schedule
/// </summary>
public class Npc : MonoBehaviour, IInteractable, ISpeaker, IShopKeeper
{

	[Header("Dialogue Data")]
	[SerializeField] private string dialogueName;
	[SerializeField] private Sprite dialoguePortrait;
	[SerializeField] private NPCDialogueData npcDialogueData;
	[SerializeField] private NPCSpeechBubbleData speechBubbleData;

	[Header("Shop Data")]
	[SerializeField] private ItemDataSo[] shopList;

	[Header("Cached Daily Dialogue")]
	private DialogueGroup _dailyDefaultGroup;
	private DialogueGroup _dailyHobbyGroup;
	private DialogueGroup _dailyHomeGroup;
	private DialogueGroup _dailySleepGroup;
	private DialogueGroup _dailyWorkGroup;
	[Header("References")]
	private NPCController _npcController;
	public string[] CurrentSpeechBubble => GetSpeechBubbleForCurrentState();

	private void Start()
	{
		_npcController = GetComponent<NPCController>();

		// Evaluate the daily dialogue immediately for the first time
		EvaluateDailyDialogue(this, TimeSpan.Zero);
	}

	private void OnEnable()
	{
		EventBus.OnDayChanged += EvaluateDailyDialogue;
	}

	private void OnDisable()
	{
		EventBus.OnDayChanged -= EvaluateDailyDialogue;
	}

	public bool CanInteract()
	{
		if (_npcController != null)
			return _npcController.IsInteractable;

		return true;
	}

	public void Interact(ControllerPlayer controllerPlayer)
	{
		if (!CanInteract())
			return;

		DialogueManager.Instance.StartDialogue(this, controllerPlayer);
	}

	public ItemDataSo[] ShopList => shopList;
	public string DialogueName => dialogueName;
	public Sprite DialoguePortrait => dialoguePortrait;

	public DialogueGroup CurrentDialogueGroup => GetDialogueForCurrentState();

	private void EvaluateDailyDialogue(object sender, TimeSpan time)
	{
		// Pass the DialogueGroup Array to the SelectGroup method for each state
		_dailyDefaultGroup = SelectGroup(npcDialogueData.DefaultDialogueNode);
		_dailyHomeGroup = SelectGroup(npcDialogueData.HomeDialogueNode);
		_dailySleepGroup = SelectGroup(npcDialogueData.SleepDialogueNode);
		_dailyHobbyGroup = SelectGroup(npcDialogueData.HobbyDialogueNode);
		_dailyWorkGroup = SelectGroup(npcDialogueData.WorkDialogueNode);
	}
	/// <summary>
	/// Checks requirements and selects a single valid DialogueGroup from an array of options.
	/// Prioritizes 'important' nodes, then selects among remaining valid options using weighted probabilities.
	/// </summary>
	private DialogueGroup SelectGroup(DialogueGroup[] nodeGroup)
	{
		// Safety Check
		if (nodeGroup == null || nodeGroup.Length == 0) return null;

		// 1. Create a list to hold valid DialogueGroups
		List<DialogueGroup> validGroups = new List<DialogueGroup>();

		// 2. Iterate through each DialogueGroup in the array
		foreach (DialogueGroup dialogueGroup in nodeGroup)
		{
			// Safety Check
			if (dialogueGroup == null) continue;

			// 3. Get the starting node of the DialogueGroup
			DialogueNode startNode = dialogueGroup.GetStartingNode();

			// 4. Check if all requirements are met and add to the valid groups list
			if (startNode != null && startNode.requirements.All(r => r.IsMet()))
				validGroups.Add(dialogueGroup);
		}

		// 5. If no valid node groups were found, return null
		if (validGroups.Count == 0) return null;

		// 6. Check for any important nodeGroup
		DialogueGroup importantGroup = validGroups.FirstOrDefault(n => n.GetStartingNode().isImportant);
		// If there is, return it
		if (importantGroup != null) return importantGroup;

		// 7. Otherwise, return a random node through weighted probabilities
		float totalWeight = validGroups.Sum(n => n.GetStartingNode().selectionWeight);
		float randomValue = Random.Range(0, totalWeight);
		float currentWeight = 0f;
		foreach (DialogueGroup dialogueGroup in validGroups)
		{
			currentWeight += dialogueGroup.GetStartingNode().selectionWeight;
			if (randomValue <= currentWeight) return dialogueGroup;
		}

		// Fallback if random group was not selected for some reason
		return validGroups[0];
	}

	/// <summary>
	/// Determines the appropriate dialogue group to load right now based on the NPC's active state or schedule.
	/// </summary>
	private DialogueGroup GetDialogueForCurrentState()
	{
		// 1. Check for global override DialogueGroups (ex: quest turn in)
		DialogueGroup globalPriorityGroup = SelectGroup(npcDialogueData.GlobalDialogueNodes);
		if (globalPriorityGroup != null) return globalPriorityGroup;

		// 2. Check if in Override State and use its DialogueNode
		if (_npcController.IsOverrideState && _npcController.OverrideState is IStateOverrider stateOverrider)
			return stateOverrider.GetDialogueGroup();

		// 3. Get the Schedule ControllerEntity
		var scheduleController = GetComponent<NPCScheduleController>();
		if (scheduleController == null) return _dailyDefaultGroup;

		var scheduledState = scheduleController.CurrentScheduledState;

		// 4. Switch based on the scheduled state reference
		if (scheduledState == _npcController.WorkState) return _dailyWorkGroup ?? _dailyDefaultGroup;
		if (scheduledState == _npcController.HomeState) return _dailyHomeGroup ?? _dailyDefaultGroup;
		if (scheduledState == _npcController.HobbyState) return _dailyHobbyGroup ?? _dailyDefaultGroup;
		if (scheduledState == _npcController.SleepState) return _dailySleepGroup ?? _dailyDefaultGroup;

		return _dailyDefaultGroup;

	}

	/// <summary>
	/// Retrieves passive speech bubble strings appropriate for the NPC's current schedule state.
	/// </summary>
	private string[] GetSpeechBubbleForCurrentState()
	{
		// 1. Get the Schedule ControllerEntity
		var scheduleController = GetComponent<NPCScheduleController>();
		if (scheduleController == null) return speechBubbleData.DefaultSpeechBubbles;

		var scheduledState = scheduleController.CurrentScheduledState;

		if (scheduledState == _npcController.WorkState) return speechBubbleData.WorkSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
		if (scheduledState == _npcController.HomeState) return speechBubbleData.HomeSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
		if (scheduledState == _npcController.HobbyState) return speechBubbleData.HobbySpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
		if (scheduledState == _npcController.SleepState) return speechBubbleData.SleepSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;

		return speechBubbleData.DefaultSpeechBubbles;
	}
}

/// <summary>
/// A serialized container for an NPC's dialogue trees, categorized by different daily states or global overrides.
/// </summary>
[Serializable]
public class NPCDialogueData
{
	[Header("Global Override Dialogue")]
	public DialogueGroup[] GlobalDialogueNodes;

	[Header("Daily Schedule Dialogue")]
	public DialogueGroup[] DefaultDialogueNode;
	public DialogueGroup[] HomeDialogueNode;
	public DialogueGroup[] SleepDialogueNode;
	public DialogueGroup[] HobbyDialogueNode;
	public DialogueGroup[] WorkDialogueNode;
}

/// <summary>
/// A serialized container for arrays of passive speech bubble text, categorized by the NPC's daily schedule states.
/// </summary>
[Serializable]
public class NPCSpeechBubbleData
{
	public string[] DefaultSpeechBubbles;
	public string[] HomeSpeechBubbles;
	public string[] SleepSpeechBubbles;
	public string[] HobbySpeechBubbles;
	public string[] WorkSpeechBubbles;
}
