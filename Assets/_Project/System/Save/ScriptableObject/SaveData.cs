using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Holds information of all Saved Data
/// </summary>
[Serializable]
public class SaveData
{
	//----SkillControllerPlayer Variables----
	public Vector3 PlayerPosition;
	public float HpMax;
	public float HpCurrent;
	public float MpMax;
	public float MpCurrent;
	public int LvlCurrent;
	public int ExpCurrent;
	public int GoldCurrent;

	//----Skill Tree Variables----
	public List<SkillTreeLedger> SkillTreeLedgers;

	//----Location Variables----
	public string _locationCurrent;

	//----Inventory Vairables----
	public List<SavedSlot> SavedSlotList = new List<SavedSlot>();

	//----Environment Variables----
	public List<string> ChestsOpenedList = new List<string>();

	//----QuestUI Variables----
	public List<SavedQuest> SavedQuestsList = new List<SavedQuest>();
	public List<string> CompletedQuestsList = new List<string>();
}

/// <summary>
/// Holds the information of one Slot
/// </summary>
[Serializable]
public struct SavedSlot
{
	public int index; // Which Slot SlotIndex?
	public string itemID;
	public int itemStackSize;
}

/// <summary>
/// Holds the information of one QuestUI
/// </summary>
[Serializable]
public struct SavedQuest
{
	public string questID;
	public int[] objectiveProgress;
	public bool isCompleted;
}
