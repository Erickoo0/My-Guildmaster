using System.Collections.Generic;
using UnityEngine;
public abstract class SkillControllerBase : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private SkillDataDatabase _skillDatabase;
	[SerializeField] private SkillTreeDatabase _skillTreeDatabase;

	// Tracks the last cast time of each skill by its ID
	private readonly Dictionary<string, float> _skillCooldowns = new Dictionary<string, float>();
	public FirePoint FirePoint { get; private set; }
	public CastBar CastBar { get; private set; }

	public SkillDataDatabase SkillDatabase => _skillDatabase;
	public SkillTreeDatabase SkillTreeDatabase => _skillTreeDatabase;

	protected virtual void Awake()
	{
		FirePoint = GetComponentInChildren<FirePoint>();
		CastBar = GetComponent<CastBar>();
	}

	protected bool IsSkillOnCooldown(string skillID, float cooldownDuration)
	{
		// Find the skill with the matching skillID in the dictionary
		if (_skillCooldowns.TryGetValue(skillID, out float lastCastTime))
			// If the enough time has passed (cooldownDuration) since the last cast, return true
			return Time.time < lastCastTime + cooldownDuration;

		return false;
	}

	/// <summary>
	/// Sets the last cast time for a skill to the current time.
	/// </summary>
	public void TriggerSkillCooldown(string skillID) => _skillCooldowns[skillID] = Time.time;

	/// <summary>
	/// Returns a normalized value (0.0 to 1.0) representing the remaining cooldown progress.
	/// </summary>
	public float GetRemainingCooldownRatio(string skillID, float cooldownDuration)
	{
		// If skill is not on cd, simply return 0
		if (!_skillCooldowns.TryGetValue(skillID, out float lastCastTime))
			return 0f;

		float timePassed = Time.time - lastCastTime;
		float remaining = cooldownDuration - timePassed;

		return Mathf.Clamp01(remaining/cooldownDuration);
	}
}
