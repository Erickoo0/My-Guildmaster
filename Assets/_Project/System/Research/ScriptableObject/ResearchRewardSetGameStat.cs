using UnityEngine;
/// <summary>
/// Rsearch reward that modifiers a game stat.
/// </summary>
public class ResearchRewardModifyGameStat : ResearchReward
{
	[SerializeField] private FlagKeys.GameStat _stat;
	[SerializeField] private int _value;

	public override void ApplyReward()
	{
		EventBus.RequestGameStatChanged(_stat, _value);
	}
}
