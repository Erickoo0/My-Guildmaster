using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class SkillControllerPlayer : SkillControllerBase
{
	[Header("SkillControllerPlayer Skill Loadout")]
	[Tooltip("Slot 0: M1, Slot 1: Q, Slot 2: E, Slot 3: R, Slot 4: F")]
	[SerializeReference, SubclassSelector] private List<PlayerSkillStateBase> _skillSlots = new List<PlayerSkillStateBase>(5);
	private readonly bool[] _skillKeyHeld = new bool[5];

	[Header("References")]
	private ControllerPlayer _controllerPlayer;

	public IReadOnlyList<PlayerSkillStateBase> SkillSlots => _skillSlots;

	protected override void Awake()
	{
		base.Awake();

		// Cache references
		_controllerPlayer = GetComponent<ControllerPlayer>();
	}

	private void Start()
	{
		// Loop through Skill Slots and call Setup
		foreach (PlayerSkillStateBase skillState in SkillSlots)
			skillState?.Setup(_controllerPlayer, _controllerPlayer.StateMachine);
	}

	private void OnDestroy()
	{
		// Loop through Skill Slots and call OnDestroy
		foreach (PlayerSkillStateBase spellState in SkillSlots)
			spellState?.OnDestroy();
	}

	/// <summary>
	/// Returns true only if all conditions are met.
	/// Key is being held down, spell is not on cooldown, the player is not in dash state, and the player has enough mana.
	/// </summary>
	public void TryTriggerSkill(int skillKeyIndex, InputAction.CallbackContext context)
	{
		// Safety Check
		if (SkillSlots == null || skillKeyIndex < 0 || skillKeyIndex >= SkillSlots.Count || SkillSlots[skillKeyIndex] == null) return;

		// 1. Track key input state 
		if (context.started || context.performed) _skillKeyHeld[skillKeyIndex] = true;
		else if (context.canceled) _skillKeyHeld[skillKeyIndex] = false;
		if (!context.performed) return;

		// 2. Assign the intended skill
		PlayerSkillStateBase intendedPlayerSkill = SkillSlots[skillKeyIndex];
		if (intendedPlayerSkill.SkillDataInstance == null) return;

		// 4. Check current player state 
		if (_controllerPlayer.StateMachine.CurrentState == _controllerPlayer.DashState ||
			_controllerPlayer.StateMachine.CurrentState is PlayerSkillStateBase)
			return;

		// 3. Check localized cooldown of intended skill
		if (IsSkillOnCooldown(intendedPlayerSkill.SkillDataInstance.ID, intendedPlayerSkill.SkillDataInstance.Cooldown))
			return;

		// 5. Check mana
		if (_controllerPlayer.MpComponent == null
			|| !_controllerPlayer.MpComponent.HasEnoughMp(intendedPlayerSkill.MpCost))
			return;

		// 5. Execute Spell
		intendedPlayerSkill.CurrentSlotIndex = skillKeyIndex;
		_controllerPlayer.StateMachine.ChangeState(intendedPlayerSkill);
	}

	public bool IsSkillKeyHeld(int skillKeyIndex)
	{
		if (skillKeyIndex < 0 || skillKeyIndex >= _skillKeyHeld.Length) return false;
		return _skillKeyHeld[skillKeyIndex];
	}
}
