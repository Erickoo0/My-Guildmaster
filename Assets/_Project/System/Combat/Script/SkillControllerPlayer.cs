using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SkillControllerPlayer : SkillControllerBase
{
    [Header("SkillControllerPlayer Spell Loadout")]
    [Tooltip("Slot 0: M1, Slot 1: Q, Slot 2: E, Slot 3: R, Slot 4: F")]
    [SerializeReference, SubclassSelector] private List<PlayerSkillStateBase> _spellSlots = new List<PlayerSkillStateBase>(5);
    private readonly bool[] _spellKeyHeld = new bool[5];
    
    [Header("References")]
    private PlayerController _playerController;
    
    public IReadOnlyList<PlayerSkillStateBase> SpellSlots => _spellSlots;

    protected override void Start()
    {
        base.Start();
        _playerController = GetComponent<PlayerController>();
        
        // Setup skill states
        foreach (PlayerSkillStateBase spellState in SpellSlots)
            spellState?.Setup(_playerController, _playerController.StateMachine);
    }

    private void OnDestroy()
    {
        foreach (PlayerSkillStateBase spellState in SpellSlots)
            spellState?.OnDestroy();
    }

    public void TryTriggerSpell(int spellKeyIndex, InputAction.CallbackContext context)
    {
        // Safety Check
        if (SpellSlots == null || spellKeyIndex < 0 || spellKeyIndex >= SpellSlots.Count || SpellSlots[spellKeyIndex] == null) return;
        
        // 1. Track key input state 
        if (context.started || context.performed) _spellKeyHeld[spellKeyIndex] = true;
        else if (context.canceled) _spellKeyHeld[spellKeyIndex] = false;
        if (!context.performed) return;

        PlayerSkillStateBase intendedSpell = SpellSlots[spellKeyIndex];
        
        // 2. Check cooldown and current state
        if (!CheckActionCooldown() 
            || _playerController.StateMachine.CurrentState == _playerController.DashState) 
            return;
        
        // 3. Check mana
        if (_playerController.MpComponent == null 
            || !_playerController.MpComponent.HasEnoughMp(intendedSpell.MpCost)) 
            return;
        
        // 4. Execute Spell
        intendedSpell.CurrentSlotIndex = spellKeyIndex;
        _playerController.StateMachine.ChangeState(intendedSpell);
    }

    public bool IsSpellKeyHeld(int spellKeyIndex)
    {
        if (spellKeyIndex < 0 || spellKeyIndex >= _spellKeyHeld.Length) return false;
        return _spellKeyHeld[spellKeyIndex];
    }
}
