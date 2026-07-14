using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Static Blueprint for a base level 1 skill.
/// </summary>
[CreateAssetMenu(fileName = "Skill_Data_", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
	[Header("References")]
	public string ID;
	public string Name;
	public Sprite Icon;
	public GameObject Prefab;
	public AnimationBool Animation;

	[Header("Base Stats")]
	public float DamageBase = 0f;
	public DamageScalingStat DamageScalingStat = DamageScalingStat.AttackPower;
	public float DamageScalingRatio = 1f;
	public float MpCostBase = 0; // Not needed for enemies
	public float CastTimeBase;

	[Header("Game Feel Settings")]
	public bool CastBarDisplay = true;

	[Header("Behavior Settings")]
	[SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();
	[SerializeReference, SubclassSelector] public List<Requirement> RequirementsList = new List<Requirement>();

	[Header("Selection Settings")]
	public float SelectionWeight = 10f;
	public int AnimationTag => Animator.StringToHash(Animation.ToString());




	protected void OnValidate()
	{
		if (ID != name)
		{
			ID = name;
        #if UNITY_EDITOR
			EditorUtility.SetDirty(this);
        #endif
		}
	}

	public bool AreRequirementsMet(GameObject context)
	{
		// 1. No requirements means always true
		if (RequirementsList == null || RequirementsList.Count == 0) return true;

		// 2. Iterate through each requirement and check if met
		foreach (Requirement requirement in RequirementsList)
			if (!requirement.IsMet(context))
				return false;

		// If no requirement returns false, then return true
		return true;
	}

	/// <summary>
	/// Create a fresh SkillDataInstance from this blueprint.
	/// With copied stats and effects ready for modification.
	/// </summary>
	public SkillDataInstance CreateSkillDataInstance()
	{
		// 1. Build the base stats dictionary of the skill
		Dictionary<SkillStat, float> statsBase = new Dictionary<SkillStat, float>
		{
			{
				SkillStat.DamageBase, DamageBase
			},
			{
				SkillStat.DamageScalingRatio, DamageScalingRatio
			},
			{
				SkillStat.MpCost, MpCostBase
			},
			{
				SkillStat.CastTime, CastTimeBase
			},
			{
				SkillStat.DisplayCastBar, CastBarDisplay ? 1f : 0f
			}
		};

		// 2. Clone the effects list
		List<Effect> effectsClonedList = new List<Effect>();
		if (EffectsList != null)
			foreach (Effect effect in EffectsList)
				if (effect != null)
					effectsClonedList.Add(effect.Clone());

		// 3. Construct and return the SkillDataInstance
		return new SkillDataInstance(this, statsBase, effectsClonedList);

	}
}

public enum AnimationBool
{
	IsAttacking,
	IsAttackingStrong
}
