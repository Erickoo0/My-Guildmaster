using UnityEngine;
using System.Collections.Generic;

public class SkillControllerEntity : SkillControllerBase
{
    [Header("SkillControllerEntity Spell Loadout")]
    [SerializeReference, SubclassSelector] private List<SkillStateBase> _skillStatesList = new List<SkillStateBase>();

    [Header("References")]
    private MobController _mobController;

    public IReadOnlyList<SkillStateBase> SkillStatesList => _skillStatesList;

    protected override void Start()
    {
        base.Start();
        // Cache references
        _mobController = GetComponent<MobController>();
        
        // Setup attack states
        foreach (SkillStateBase attackState in SkillStatesList)
            attackState.Setup(_mobController, _mobController.StateMachine);
    }
    
    private void OnDestroy()
    {
        foreach (SkillStateBase spellState in _skillStatesList)
            spellState?.OnDestroy();
    }

    public SkillStateBase GetRandomSkillState()
    {
        if (SkillStatesList == null || SkillStatesList.Count == 0) return null;

        float totalWeight = 0;
        List<SkillStateBase> validStates = new List<SkillStateBase>();
        
        // 1. Gather all valid states and calculate total weight
        foreach (SkillStateBase attackState in SkillStatesList)
        {
            if (attackState != null && attackState.SelectionWeight > 0)
            {
                // Check if the SkillStatesList requirements are met
                if (attackState.CheckRequirementsMet(this.gameObject))
                {
                    validStates.Add(attackState);
                    totalWeight += attackState.SelectionWeight; 
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
            {
                return state;
            }
        }

        return null; // Fallback safety
    }
}
