using UnityEngine;

/// <summary>
/// Runtime owner and save bridge for SkillTreeLedgerContainer.
/// This MonoBehavior exists solely to bridge the gap between the SpellControllerPlayer's SkillTreeLedger and the SaveManager.'
/// </summary>
public class SkillTreeLedgerManager : MonoBehaviour, ISaveable
{
    public static SkillTreeLedgerManager Instance { get; private set; }
    
    [SerializeField] private SkillTreeLedgerContainer _skillTreeLedgerContainer = new SkillTreeLedgerContainer();
    
    public SkillTreeLedgerContainer SkillTreeLedgerContainer => _skillTreeLedgerContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PopulateSaveData(SaveData saveData) => saveData.SkillTreeLedgerContainer = _skillTreeLedgerContainer;

    public void LoadFromSaveData(SaveData saveData) => _skillTreeLedgerContainer = saveData.SkillTreeLedgerContainer ?? new SkillTreeLedgerContainer();
}
