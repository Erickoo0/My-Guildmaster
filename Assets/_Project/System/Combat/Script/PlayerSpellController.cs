using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerSpellController : BaseSpellController
{
    [Header("Player Spell Loadout")]
    [Tooltip("Slot 0: M1, Slot 1: Q, Slot 2: E, Slot 3: R, Slot 4: F")]
    [SerializeReference, SubclassSelector] public List<BasePlayerSpellState> SpellSlots = new List<BasePlayerSpellState>(5);
    private bool[] _spellKeyHeld = new bool[5];
    
    [Header("References")]
    private PlayerController _playerController;

    protected override void Start()
    {
        base.Start();
        _playerController = GetComponent<PlayerController>();
        
        // Setup spell states
        foreach (BasePlayerSpellState spellState in SpellSlots)
            spellState?.Setup(_playerController, _playerController.StateMachine);
    }

    public void TryTriggerSpell(int spellKeyIndex, InputAction.CallbackContext context)
    {
        // Safety Check
        if (SpellSlots == null || spellKeyIndex < 0 || spellKeyIndex >= SpellSlots.Count || SpellSlots[spellKeyIndex] == null) return;
        
        // 1. Track key input state 
        if (context.started || context.performed) _spellKeyHeld[spellKeyIndex] = true;
        else if (context.canceled) _spellKeyHeld[spellKeyIndex] = false;
        if (!context.performed) return;

        BasePlayerSpellState intendedSpell = SpellSlots[spellKeyIndex];
        
        // 2. Check cooldown and current state
        if (!CheckActionCooldown() 
            || _playerController.StateMachine.CurrentState == _playerController.DashState) 
            return;
        
        // 3. Check mana
        if (_playerController.mpComponent == null 
            || !_playerController.mpComponent.HasEnoughMp(intendedSpell.MpCost)) 
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
