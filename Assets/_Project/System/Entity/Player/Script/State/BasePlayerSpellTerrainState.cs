using UnityEngine;

[System.Serializable]
public class BasePlayerSpellTerrainState : BasePlayerSpellState
{
    private TerrainSpellData _terrainSpellData;

    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _terrainSpellData = spellData as TerrainSpellData;
    }

    public override void Enter()
    {
        // Safety Check
        if (_terrainSpellData == null || _terrainSpellData.spellPrefab == null)
        {
            Debug.LogWarning("Missing Terrain Data");
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
        
        Vector3 spawnPosition = controller.WorldMousePosition;
        Vector3 casterPosition = controller.transform.position;
        Vector2 direction = (spawnPosition - casterPosition).normalized;
        
        GameObject terrain = Object.Instantiate(_terrainSpellData.spellPrefab, spawnPosition, Quaternion.identity);
        
        if (terrain.TryGetComponent(out Terrain terrainComponent))
        {
            float terrainHp = _terrainSpellData.terrainHpMax;
            float terrainDuration = _terrainSpellData.terrainDuration;
            terrainComponent.Setup(direction, terrainHp, terrainDuration);
        }
        
        // Apply Scale
        if (_terrainSpellData.spellScale != 1f) terrain.transform.localScale *= _terrainSpellData.spellScale;
        
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
