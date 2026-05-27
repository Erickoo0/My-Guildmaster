using UnityEngine;

public class PlayerStatProvider : MonoBehaviour, IStatProvider
{
	public Health EntityHealth => PlayerStatsManager.Instance.HealthComponent;
    
	public Mana EntityMana => PlayerStatsManager.Instance.ManaComponent;
	
	public Level EntityLevel => PlayerStatsManager.Instance.LevelComponent;
}
