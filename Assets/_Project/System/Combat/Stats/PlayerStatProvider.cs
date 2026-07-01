using UnityEngine;
public class PlayerStatProvider : MonoBehaviour, IStatProvider
{

	private void Start()
	{
		EntityLevel.OnLevelUpdated += UpdateGameStat;
	}

	private void OnDisable()
	{
		EntityLevel.OnLevelUpdated -= UpdateGameStat;
	}
	public Health EntityHealth => PlayerStatsManager.Instance.HealthComponent;
	public Mana EntityMana => PlayerStatsManager.Instance.ManaComponent;
	public Level EntityLevel => PlayerStatsManager.Instance.LevelComponent;
	public EntityStats EntityStats => PlayerStatsManager.Instance.EntityStatsComponent;

	private void UpdateGameStat() => GameFlagManager.Instance.SetGameStat(FlagKeys.GameStat.Player_CurrentLevel, EntityLevel.LvlCurrent);
}
