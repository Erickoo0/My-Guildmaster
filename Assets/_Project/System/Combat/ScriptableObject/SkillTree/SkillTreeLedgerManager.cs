using UnityEngine;

/// <summary>
/// Runtime owner and save bridge for PlayerSkillTreeLedger.
/// This MonoBehavior exists solely to bridge the gap between the Player's SkillTreeLedger and the SaveManager.'
/// </summary>
public class SkillTreeLedgerManager : MonoBehaviour, ISaveable
{
    public static SkillTreeLedgerManager Instance { get; private set; }
    
    [SerializeField] private PlayerSkillTreeLedger _playerSkillTreeLedger = new PlayerSkillTreeLedger();
    
    public PlayerSkillTreeLedger PlayerSkillTreeLedger => _playerSkillTreeLedger;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PopulateSaveData(SaveData saveData) => saveData._playerSkillTreeLedger = _playerSkillTreeLedger;

    public void LoadFromSaveData(SaveData saveData) => _playerSkillTreeLedger = saveData._playerSkillTreeLedger ?? new PlayerSkillTreeLedger();
}
