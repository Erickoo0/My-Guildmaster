using UnityEngine;

[System.Serializable]
public class BasePlayerSpellBuffState : BasePlayerSpellState
{
    private BuffSpellData _buffSpellData;
    private BuffSpellData.BuffType _buffType;

    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _buffSpellData = spellData as BuffSpellData;
    }

    public override void Enter()
    {
        // Safety Check
        if (_buffSpellData == null || _buffSpellData.spellPrefab == null)
        {
            Debug.LogWarning("Missing Buff Data");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        // Face the aim direction
        Vector2 aimDirection = (controller.WorldMousePosition - controller.transform.position).normalized;
        controller.EntityAnimator.FaceDirection(aimDirection);
        controller.EntityAnimator.animator.Update(0f);
        
        base.Enter();
    }
    
    public override void Exit()
    {
        base.Exit();
      
        Vector2 aimDirection = (controller.WorldMousePosition - controller.transform.position).normalized;
        controller.EntityAnimator.FaceDirection(aimDirection);
        controller.EntityAnimator.animator.Update(0f);
    }

    protected override void HandleAnimationEvent()
    {
        if (hasTriggered) return;
        if (controller == null) return;

        Vector3 spawnPosition = controller.transform.position;
        Vector3 direction = (controller.WorldMousePosition - spawnPosition).normalized;
        
        GameObject buff = Object.Instantiate(_buffSpellData.spellPrefab, spawnPosition, Quaternion.identity);
        buff.transform.SetParent(controller.transform);
        
        if (buff.TryGetComponent(out Buff buffComponent))
        {
            GameObject buffReceiver = controller.gameObject;
            _buffType = _buffSpellData.buffType;
            float buffAmount = _buffSpellData.buffAmount;
            float buffDuration = _buffSpellData.buffDuration;
            
            buffComponent.Setup(buffReceiver, _buffType, buffAmount, buffDuration);
        }
        
        // Apply Scale
        if (_buffSpellData.spellScale != 1f) buff.transform.localScale *= _buffSpellData.spellScale;
        
        // Apply Recoil
        if (spellData.spellAnimation == AnimationBool.IsAttackingStrong) 
        {
            controller?.EntityMover.ApplyRecoil(direction);
        }
        
        // Consume Mana
        controller?.mpComponent.ConsumeMp(spellData.baseMpCost);
      
        hasTriggered = true;
    }
}
