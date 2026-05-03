using UnityEngine;

[System.Serializable]
public class PlayerMouseAttackState : State<PlayerController>
{
    private MouseAttackData _attackData;
    private PlayerCombatController _combatController;
    private bool _isFinished;
    
    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        // Search the Combat Controller for the specific data asset
        _combatController = controller.GetComponent<PlayerCombatController>();
        
        if (_combatController != null)
        {
            _attackData = _combatController.GetAttackData<MouseAttackData>();
        }

        if (_attackData == null) 
            Debug.LogWarning($"{controller.gameObject.name} has MouseAttackState but no MouseAttackData in library.");

    }
    
    public override void Enter()
    {
        _isFinished = false;

        // Safety Check
        if (_attackData == null || _attackData.attackPrefab == null)
        {
            _isFinished = true;
            return;
        }
    
        // 1. Prepare Damage using the injected context
        DamageData executionDamage = _attackData.damageData;
        executionDamage.source = _combatController.CombatContext.source;
    
        // 2. Spawn the Attack Prefab
        GameObject attackInstance = Object.Instantiate(_attackData.attackPrefab, _combatController.CombatContext.mousePosition, Quaternion.identity);
    
        // 3. Give the HitBox the actual damage data!
        if (attackInstance.TryGetComponent<HitBox>(out HitBox spawnedHitbox))
        {
            spawnedHitbox.Setup(executionDamage);
            spawnedHitbox.enableHitbox = true;
        }
        else
        {
            Debug.LogWarning("Mouse attack prefab does not have a HitBox component attached!");
        }
        
        _isFinished = true;
    }
    
    public override void Update()
    {
        if (_isFinished) stateMachine.ChangeState(controller.IdleState);
    }
    
    public override void Exit() { }
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }
}
