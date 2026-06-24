using UnityEngine;
using System.Collections.Generic;

public class EntitySpellController : BaseSpellController
{
    [Header("Entity Spell Loadout")]
    [SerializeReference, SubclassSelector] public List<BaseAttackState> attackStates = new List<BaseAttackState>();

    [Header("References")]
    private MobController _mobController;

    protected override void Start()
    {
        base.Start();
        // Cache references
        _mobController = GetComponent<MobController>();
        
        // Setup attack states
        foreach (BaseAttackState attackState in attackStates)
            attackState.Setup(_mobController, _mobController.StateMachine);
    }

    public BaseAttackState GetRandomAttackState()
    {
        if (attackStates == null || attackStates.Count == 0) return null;

        float totalWeight = 0;
        List<BaseAttackState> validStates = new List<BaseAttackState>();
        
        // 1. Gather all valid states and calculate total weight
        foreach (BaseAttackState attackState in attackStates)
        {
            if (attackState != null && attackState.SelectionWeight > 0)
            {
                // Check if the attackStates requirements are met
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
