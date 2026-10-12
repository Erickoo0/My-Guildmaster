using System;
using UnityEngine;
/// <summary>
/// Base class for rewards that are applied when a ResearchNode is unlocked.
/// </summary>
[Serializable]
public abstract class ResearchReward
{
	[Tooltip("Whether the reward is a one-time reward or persistent")]
	[SerializeField] private bool _isPersistent = true;
	[SerializeField, TextArea(3, 10)] private string _rewardDescription;
	public string RewardDescription => _rewardDescription;
	public bool IsPersistent => _isPersistent;

	public abstract void ApplyReward();
}
