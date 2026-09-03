using System;
using UnityEngine;
/// <summary>
/// Defines a objective as needing a Requirement class to be met.
/// </summary>
[Serializable]
public class QuestObjectiveState : QuestObjectiveBase
{
	[SerializeReference, SubclassSelector] public Requirement requirement;

	public override bool IsCountBased => false;
	public override int RequiredAmount => -1;

	public override bool IsConditionMet()
	{
		if (requirement == null) return false;
		return requirement.IsMet();
	}
}
