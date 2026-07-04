using UnityEngine;
public class EntityStatProvider : MonoBehaviour, IStatProvider
{
	private EntityStats _entityStats;
	private Health _health;
	private Level _level;
	private Mana _mana;

	private void Awake()
	{
		_health = GetComponent<Health>();
		_mana = GetComponent<Mana>();
		_level = GetComponent<Level>();
		_entityStats = GetComponent<EntityStats>();
	}

	private void Start() => SyncStatsToLevel();

	private void OnEnable() => _level.OnLevelUpdated += SyncStatsToLevel;

	private void OnDisable() => _level.OnLevelUpdated -= SyncStatsToLevel;

	public Health EntityHealth => _health;
	public Mana EntityMana => _mana;
	public Level EntityLevel => _level;
	public EntityStats EntityStats => _entityStats;

	private void SyncStatsToLevel()
	{
		if (_level == null)
			return;

		if (_health != null)
			_health.RecalculateMaxHp(_level.LvlCurrent);

		if (_mana != null)
			_mana.RecalculateMaxMp(_level.LvlCurrent);

		if (_entityStats != null)
			_entityStats.RecalculateStats(_level.LvlCurrent);
	}
}
