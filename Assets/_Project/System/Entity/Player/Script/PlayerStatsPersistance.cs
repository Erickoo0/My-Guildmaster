using UnityEngine;
public class PlayerStatsPersistance : MonoBehaviour, ISaveable
{
	private Health _healthComponent;
	private Level _levelComponent;
	private Mana _manaComponent;
	private GameObject _player;

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
		saveData.PlayerPosition = transform.position;
		saveData.HpCurrent = _healthComponent.HpCurrent;
		saveData.HpMax = _healthComponent.HpMax;
		saveData.MpCurrent = _manaComponent.MpCurrent;
		saveData.MpMax = _manaComponent.MpMax;
		saveData.LvlCurrent = _levelComponent.LvlCurrent;
		saveData.ExpCurrent = _levelComponent.ExpCurrent;
	}

	public void LoadFromSaveData(SaveData saveData)
	{
		_player.transform.position = saveData.PlayerPosition;
		_healthComponent.HpCurrent = saveData.HpCurrent;
		_manaComponent.MpCurrent = saveData.MpCurrent;
		_levelComponent.LvlCurrent = saveData.LvlCurrent;
		_levelComponent.ExpCurrent = saveData.ExpCurrent;
	}
}
