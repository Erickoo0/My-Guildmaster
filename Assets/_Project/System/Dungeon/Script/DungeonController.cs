using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DungeonController : MonoBehaviour
{

	[Header("Dungeon Database")]
	[Tooltip("A list of all biomes/dungeons.")]
	[SerializeField] private List<DungeonData> dungeonLibrary;

	private DungeonData _currentDungeon;
	private int _currentRound;
	private DungeonZone _currentZone;
	private DungeonEnemyTracker _dungeonEnemyTracker;
	[Header("References")]
	private DungeonSpawner _dungeonSpawner;
	private bool _isDungeonActive = false;

	private void Awake()
	{
		_dungeonSpawner = GetComponent<DungeonSpawner>();
		_dungeonEnemyTracker = GetComponent<DungeonEnemyTracker>();
	}

	private void Start()
	{
		if (LocationManager.Instance == null)
			return;

		LocationManager.Instance.OnLocationChanged += CheckIfDungeon;

		if (_dungeonEnemyTracker != null)
			_dungeonEnemyTracker.OnAllEnemiesCleared += StartNextRound;

		// Force a check on startup in case we are already in the dungeon when hitting 'Play'
		CheckIfDungeon(LocationManager.Instance.CurrentLocation);
	}

	private void OnDisable()
	{
		if (LocationManager.Instance != null)
			LocationManager.Instance.OnLocationChanged -= CheckIfDungeon;

		if (_dungeonEnemyTracker != null)
			_dungeonEnemyTracker.OnAllEnemiesCleared -= StartNextRound;
	}

	public event Action OnDungeonStarted;
	public event Action OnDungeonEnded;
	public event Action<int> OnRoundStarted;

	private void CheckIfDungeon(GameLocation newLocation)
	{
		if (dungeonLibrary == null || dungeonLibrary.Count == 0)
			return;


		// 1. Checks if the new location is a dungeon by checking the dungeon library
		DungeonData foundDungeon = dungeonLibrary.Find(d => d.dungeonLocation == newLocation);

		// 2. If a dungeon is found, check if the associated zone (GameObject) is registered and exists
		if (foundDungeon != null)
		{
			if (DungeonZone.Registry.TryGetValue(newLocation, out DungeonZone foundZone))
			{
				// 3. If both are found, start the dungeon
				StartDungeon(foundDungeon, foundZone);
			} else
			{
				Debug.Log($"DungeonController: {newLocation} is a dungeon, but the associated zone is not registered.");
			}
		} else if (_isDungeonActive)
			StopAndResetDungeon();
	}

	private void StartDungeon(DungeonData dungeon, DungeonZone zone)
	{
		_currentDungeon = dungeon;
		_currentZone = zone;
		_currentRound = 1;
		_isDungeonActive = true;

		StartCoroutine(BeginRoundRoutine());
		OnRoundStarted?.Invoke(_currentRound);
		OnDungeonStarted?.Invoke();
	}

	private void StopAndResetDungeon()
	{
		StopAllCoroutines();

		_isDungeonActive = false;
		_currentRound = 0;
		_currentDungeon = null;
		_currentZone = null;

		_dungeonSpawner.StopSpawning();
		_dungeonEnemyTracker.ClearDungeon();
		OnDungeonEnded?.Invoke();
	}

	private void StartNextRound()
	{
		if (!_isDungeonActive) return;

		_currentRound++;
		Debug.Log($"[DungeonController] All enemies cleared. Starting Round {_currentRound}!");
		StartCoroutine(BeginRoundRoutine());
		OnRoundStarted?.Invoke(_currentRound);
	}

	private IEnumerator BeginRoundRoutine()
	{
		yield return new WaitForSeconds(_currentDungeon.delayBetweenRounds);

		int totalToSpawn = _currentDungeon.baseEnemyCount + ((_currentRound - 1)*_currentDungeon.enemiesPerRoundScaling);

		_dungeonSpawner.StartSpawning(_currentDungeon, _currentZone, _currentRound, totalToSpawn);
	}
}
