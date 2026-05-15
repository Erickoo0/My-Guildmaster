using UnityEngine;

[System.Serializable]
public class PlayerMouseAttackState : State<PlayerController>
{
    [SerializeField] private string attackID;
    private MouseAttackData _attackData;
    private bool _isFinished;
    
    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _attackData = controller?.GetAttackData<MouseAttackData>(attackID);
    }
    
    public override void Enter()
    {
        // Safety Check
        if (_attackData == null || _attackData.attackPrefab == null)
        {
            Debug.LogWarning($"{controller.gameObject.name} has MouseAttackState but no MouseAttackData in library.");

            _isFinished = true;
            return;
        }
        
        _isFinished = false;
    
        // 1. Prepare Damage using the injected context
        DamageData finalDamage = _attackData.CreateDamageData(controller.gameObject);
    
        // 2. Spawn the Attack Prefab
        GameObject attackInstance = Object.Instantiate(_attackData.attackPrefab, controller.WorldMousePosition, Quaternion.identity);
    
        // 3. Give the HitBox the actual damage data!
        if (attackInstance.TryGetComponent<HitBox>(out HitBox spawnedHitbox))
        {
            spawnedHitbox.Setup(finalDamage);
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
