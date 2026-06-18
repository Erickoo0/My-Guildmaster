using UnityEngine;

[System.Serializable]
public class EntityAttackState : BaseAttackState
{
    public override void Enter()
    {
        // Safety locks
        if (controller.aiLerp != null)
        {
            controller.aiLerp.canSearch = false;
            controller.aiLerp.canMove = false;
            controller.aiLerp.destination = controller.transform.position;
        }
        
        // Force Rigidbody velocity zero
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Face the aim direction upon starting the cast
        Vector2 aimDirection = (controller.currentTarget.position - controller.transform.position).normalized;
        controller.EntityAnimator.FaceDirection(aimDirection);
        controller.EntityAnimator.animator.Update(0f);
        
        base.Enter();  
    }
    
    public override void Update()
    {
        base.Update();
        
        // 1. If knocked back
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack)
        {
            stateMachine.ChangeState(controller.ChaseState);
            return;
        }
        
        // 2. Face the target while winding up
        if (controller.currentTarget != null && !hasTriggered)
        {
            Vector2 aimDirection = (controller.currentTarget.position - controller.transform.position).normalized;
            controller.EntityAnimator.FaceDirection(aimDirection);
        }
    }
    
    public override void Exit()
    {
        base.Exit();
      
        Vector2 aimDirection = (controller.currentTarget.position - controller.transform.position).normalized;
        controller.EntityAnimator.FaceDirection(aimDirection);
        controller.EntityAnimator.animator.Update(0f);
    }

    protected override void HandleAnimationEvent()
    {
        if (hasTriggered) return;
        if (controller == null) return;
        
        Vector3 casterPosition = controller.transform.position;
        Vector2 castDirection = (controller.currentTarget.position - casterPosition).normalized;

        // 1. Create a primary payload describing the INITIAL CAST event
        EffectPayload initialCastPayload = new EffectPayload(
            user: controller.gameObject,
            target: controller.gameObject,                 // Default target is caster for instant self-effects
            targetPosition: controller.currentTarget.position, // Target position is where the mouse is pointing
            hitDirection: castDirection,
            hitImpactPoint: casterPosition
            );
        
        // 2. Execute all spell effects
        if (attackData.spellEffects != null && attackData.spellEffects.Count > 0)
            foreach (Effect effect in attackData.spellEffects)
                effect.Execute(initialCastPayload);
        
        // 3. Apply Recoil if necessary
        if (attackData.spellAnimation == AnimationBool.IsAttackingStrong) 
            controller?.EntityMover.ApplyRecoil(castDirection);
        
        // 4. Apply VFX
        if (attackData.spellPrefab != null)
        {
            Vector3 spawnPosition = controller._firePoint != null
                ? controller._firePoint.transform.position
                : casterPosition;
            
            float relativeX = controller.currentTarget.transform.position.x - spawnPosition.x;
            
            Quaternion spawnRotation = relativeX >= 0
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 180, 0);
            
            GameObject spellVFX = Object.Instantiate(attackData.spellPrefab, spawnPosition, spawnRotation, controller.transform);
            Object.Destroy(spellVFX, 1f);
        }
        
        hasTriggered = true;
    }
}
