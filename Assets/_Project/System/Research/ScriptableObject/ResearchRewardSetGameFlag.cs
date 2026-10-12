using System;
using UnityEngine;
/// <summary>
/// Research reward that sets a game flag.
/// </summary>
[Serializable]
public class ResearchRewardSetGameFlag : ResearchReward
{
	[SerializeField] private FlagKeys.GameFlag _flag;
	[SerializeField] private bool _state = true;

	public override void ApplyReward()
	{
		EventBus.RequestGameFlagChanged(_flag, _state);
	}
}
