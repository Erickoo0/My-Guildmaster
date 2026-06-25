using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds information of all Saved Data
/// </summary>
[System.Serializable]
public class SaveData
{
    //----Player Variables----
    public Vector3 _playerPosition;
    public float _hpMax;
    public float _hpCurrent;
    public float _mpMax;
    public float _mpCurrent;
    public int _lvlCurrent;
    public int _expCurrent;
    public int _goldCurrent;
    
    //----Skill Tree Variables----
    public PlayerSkillTreeLedger _playerSkillTreeLedger = new PlayerSkillTreeLedger();
    
    //----Location Variables----
    public string _locationCurrent;
    
    //----Inventory Vairables----
    public List<SavedSlot> _slotListSaved = new List<SavedSlot>();
    
    //----Environment Variables----
    public List<string> _chestsOpened = new List<string>();
    
    //----Quest Variables----
    public List<SavedQuest> _questsSaved = new List<SavedQuest>();
    public List<string> _questsCompleted = new List<string>();

}

/// <summary>
/// Holds the information of one Slot
/// </summary>
[System.Serializable]
public struct SavedSlot
{
    public int index; // Which Slot Index?
    public string itemID; 
    public int itemStackSize;
}

/// <summary>
/// Holds the information of one Quest
/// </summary>
[System.Serializable]
public struct SavedQuest
{
    public string questID;
    public int[] objectiveProgress;
    public bool isCompleted;
}