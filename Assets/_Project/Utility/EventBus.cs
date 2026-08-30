using System;
using UnityEngine;
public static class EventBus
{

	public static EventHandler<TimeSpan> OnDayChanged;

	//-----------------------Dialogue Events--------------------------
	//Signals when a dialogue option is selected that has an action
	public static Action<string, object> OnDialogueEventRequested;


	//-----------------------Time Events--------------------------
	public static event EventHandler<TimeSpan> OnWorldTimeChanged;
	public static void RequestUpdateWorldTime(object sender, TimeSpan time) => OnWorldTimeChanged?.Invoke(sender, time);
	public static void RequestDayChanged(object sender, TimeSpan time) => OnDayChanged?.Invoke(sender, time);
	public static void RequestDialogueEvent(string dialogueEvent, object data)
	{
		OnDialogueEventRequested?.Invoke(dialogueEvent, data);
	}

	//-----------------------Stat/Flag Events--------------------------
	public static event Action<FlagKeys.GameFlag, bool> OnGameFlagChanged;
	public static void RequestGameFlagChanged(FlagKeys.GameFlag flag, bool state) => OnGameFlagChanged?.Invoke(flag, state);

	public static event Action<FlagKeys.GameStat, int> OnGameStatChanged;
	public static void RequestGameStatChanged(FlagKeys.GameStat stat, int value) => OnGameStatChanged?.Invoke(stat, value);

	//-----------------------UI Events--------------------------
	public static event Action<GameObject> OnMenuOpenRequested;
	public static void RequestOpenMenu(GameObject menu) => OnMenuOpenRequested?.Invoke(menu);

	public static event Action<GameObject> OnMenuCloseRequested;
	public static void RequestCloseMenu(GameObject menu = null) => OnMenuCloseRequested?.Invoke(menu);

	public static event Action<GameObject> OnMenuClosed;
	public static void NotifyMenuClosed(GameObject menu) => OnMenuClosed?.Invoke(menu);

	public static event Action<bool> OnPlayerMovementToggleRequested;
	public static void RequestPlayerMovementToggle(bool canMove) => OnPlayerMovementToggleRequested?.Invoke(canMove);

	public static event Action<IItemStorage> OnStorageOpenRequested;
	public static void RequestOpenStorage(IItemStorage storage) => OnStorageOpenRequested?.Invoke(storage);


	//--------------------------Quest Events-------------------------
	public static event Action<string, int> OnUpdateQuestObjectiveRequested;
	public static void RequestUpdateQuestObjective(string targetID, int number) => OnUpdateQuestObjectiveRequested?.Invoke(targetID, number);

	public static event Action OnUpdateQuestRequested;
	public static void RequestUpdateQuest() => OnUpdateQuestRequested?.Invoke();

	public static event Action<string> OnQuestProgressCompleted;
	public static void RequestQuestProgressCompleted(string questID) => OnQuestProgressCompleted?.Invoke(questID);

	//--------------------------Combat Events-------------------------
	public static event Action<int, Vector3> OnFloatingTextRequested;
	public static void RequestFloatingText(int amount, Vector3 position) => OnFloatingTextRequested?.Invoke(amount, position);

	public static event Action<GameObject> OnEntityDeathRequested;
	public static void RequestEntityDeathUpdate(GameObject entity) => OnEntityDeathRequested?.Invoke(entity);

	//-------------------------Skill Tree Events-------------------------
	public static event Action<string> OnSkillTreeLedgerChanged;
	public static void RequestSkillTreeLedgerChanged(string skillDataID) => OnSkillTreeLedgerChanged?.Invoke(skillDataID);

	//-------------------------Hit Impact Events-------------------------
	public static event Action<float, Vector3> OnHitImpactRequested;
	public static void RequestHitImpact(float hitImpact, Vector3 position) => OnHitImpactRequested?.Invoke(hitImpact, position);

//-------------------------Worker/Sustenance Events-------------------------
	public static event Action<int> OnTotalSustenanceChanged;
	public static void RequestTotalSustenanceChanged(int newTotal) => OnTotalSustenanceChanged?.Invoke(newTotal);

//-------------------------Crafting Events-------------------------
	public static event Action<ItemDataSo> OnCraftItemRequested;
	public static void RequestCraftItem(ItemDataSo itemToCraft) => OnCraftItemRequested?.Invoke(itemToCraft);

	public static event Action<ItemDataSo> OnRecipeUnlocked;
	public static void RequestRecipeUnlocked(ItemDataSo unlockedItem) => OnRecipeUnlocked?.Invoke(unlockedItem);
}
