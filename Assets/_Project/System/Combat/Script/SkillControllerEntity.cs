using System.Collections.Generic;
using UnityEngine;
public class SkillControllerEntity : SkillControllerBase
{
	[Header("SkillControllerEntity Skill Loadout")]
	[SerializeReference, SubclassSelector] private List<EntitySkillStateBase> _skillStatesList = new List<EntitySkillStateBase>();

	[Header("References")]
	private ControllerEntity _controllerEntity;

	public IReadOnlyList<EntitySkillStateBase> SkillStatesList => _skillStatesList;

	protected override void Awake()
	{
		base.Awake();

		// Cache references
		_controllerEntity = GetComponent<ControllerEntity>();

		// Setup attack states
		foreach (EntitySkillStateBase skillState in SkillStatesList)
			skillState.Setup(_controllerEntity, _controllerEntity.StateMachine);
	}

	private void OnDestroy()
	{
		// Execute the OnDestroy method of each spell state
		foreach (EntitySkillStateBase spellState in _skillStatesList)
			spellState?.OnDestroy();
	}

	public EntitySkillStateBase GetRandomSkillState()
	{
		// Safety Check
		if (SkillStatesList == null || SkillStatesList.Count == 0) return null;

		float totalWeight = 0;
		List<EntitySkillStateBase> validStates = new List<EntitySkillStateBase>();

		// 1. Gather all valid states and calculate total weight
		foreach (EntitySkillStateBase skillState in SkillStatesList)
		{
			if (skillState != null && skillState.SelectionWeight > 0)
			{
				// Check if the skill is on cooldown
				if (IsSkillOnCooldown(skillState.SkillDataInstance.ID, skillState.SkillDataInstance.Cooldown))
					continue;

				// Check if the SkillStatesList requirements are met
				if (skillState.CheckRequirementsMet(this.gameObject))
				{
					validStates.Add(skillState);
					totalWeight += skillState.SelectionWeight;
				}
			}
		}

		if (validStates.Count == 0) return null;

		// 2. Roll a random number
		float randomVal = Random.Range(0f, totalWeight);
		float currentWeight = 0f;

		// 3. Find the winning state
		foreach (var state in validStates)
		{
			currentWeight += state.SelectionWeight;
			if (randomVal <= currentWeight)
				return state;
		}

		return null; // Fallback safety
	}
}
