using Unity.Cinemachine;
using UnityEngine;

[System.Serializable]
public class PlayerSpellCastState : BasePlayerSpellState
{
    public override void Enter()
    {
        // Face the aim direction upon starting the cast
        Vector2 aimDirection = (controller.WorldMousePosition - controller.transform.position).normalized;
        controller.EntityAnimator.FaceDirection(aimDirection);
        controller.EntityAnimator.animator.Update(0f);
        
        base.Enter();  
    }
    
    public override void Update()
    {
        base.Update();
        
        // Face the target while winding up
        if (!hasTriggered)
        {
            Vector2 aimDirection = (controller.WorldMousePosition - controller.transform.position).normalized;
            controller.EntityAnimator.FaceDirection(aimDirection);
        }
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

        Vector3 casterPosition = controller.transform.position;
        Vector2 castDirection = (controller.WorldMousePosition - casterPosition).normalized;
        
        // 1. Create a primary payload describing the INITIAL CAST event
        EffectPayload initialCastPayload = new EffectPayload(
            user: controller.gameObject,
            target: controller.gameObject,                 // Default target is caster for instant self-effects
            targetPosition: controller.WorldMousePosition, // Target position is where the mouse is pointing
            hitDirection: castDirection,
            hitImpactPoint: casterPosition
            );
        
        // 2. Execute all spell effects
        if (spellData.spellEffects != null && spellData.spellEffects.Count > 0)
            foreach (Effect effect in spellData.spellEffects)
                effect.Execute(initialCastPayload);
        
        // 3. Apply Recoil & Screen shake if necessary
        controller.GetComponent<CinemachineImpulseSource>().GenerateImpulse();  
        if (spellData.spellAnimation == AnimationBool.IsAttackingStrong) 
            controller?.EntityMover.ApplyRecoil(castDirection);
        
        // 4. Apply VFX
        if (spellData.spellPrefab != null)
        {
            Vector3 spawnPosition = controller._firePoint != null
                ? controller._firePoint.transform.position
                : casterPosition;
            
            float relativeX = controller.WorldMousePosition.x - spawnPosition.x;
            
            Quaternion spawnRotation = relativeX >= 0
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 180, 0);
            
            GameObject spellVFX = Object.Instantiate(spellData.spellPrefab, spawnPosition, spawnRotation, controller.transform);
            Object.Destroy(spellVFX, 1f);
        }
        
        
        // 4.  Consume Mana
        controller?.mpComponent.ConsumeMp(spellData.baseMpCost);
      
        hasTriggered = true;
    }
}
