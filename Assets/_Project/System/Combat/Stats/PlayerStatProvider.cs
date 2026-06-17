using UnityEngine;

public class PlayerStatProvider : MonoBehaviour, IStatProvider
{
	public Health EntityHealth => PlayerStatsManager.Instance.HealthComponent;
	public Mana EntityMana => PlayerStatsManager.Instance.ManaComponent;
	public Level EntityLevel => PlayerStatsManager.Instance.LevelComponent;

	private void Start()
	{
		EntityLevel.OnLevelUpdated += UpdateGameStat;
	}

	private void OnDisable()
	{
		EntityLevel.OnLevelUpdated -= UpdateGameStat;
	}

	private void UpdateGameStat() => GameFlagManager.Instance.SetGameStat(FlagKeys.GameStat.Player_CurrentLevel, EntityLevel.LvlCurrent);
}
