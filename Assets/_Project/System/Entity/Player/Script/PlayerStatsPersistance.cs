using UnityEngine;

public class PlayerStatsPersistance : MonoBehaviour, ISaveable
{
    private GameObject _player;
    private Health _healthComponent;
    private Mana _manaComponent;
    private Level _levelComponent;
    
    private void Awake()
    {
        // Find the player game object
        _player = GameObject.FindGameObjectWithTag("Player");
        _healthComponent = GetComponent<Health>();
        _manaComponent = GetComponent<Mana>();
        _levelComponent = GetComponent<Level>();
    }

    public void PopulateSaveData(SaveData saveData)
    {
        saveData._playerPosition = transform.position;
        saveData._hpCurrent = _healthComponent.HpCurrent;
        saveData._hpMax = _healthComponent.HpMax;
        saveData._mpCurrent = _manaComponent.MpCurrent;
        saveData._mpMax = _manaComponent.mpMax;
        saveData._lvlCurrent = _levelComponent.LvlCurrent;
        saveData._expCurrent = _levelComponent.ExpCurrent;
    }
    
    public void LoadFromSaveData(SaveData saveData)
    {
        _player.transform.position = saveData._playerPosition;
        _healthComponent.HpCurrent = saveData._hpCurrent;
        _manaComponent.MpCurrent = saveData._mpCurrent;
        _levelComponent.LvlCurrent = saveData._lvlCurrent;
        _levelComponent.ExpCurrent = saveData._expCurrent;
    }
}
