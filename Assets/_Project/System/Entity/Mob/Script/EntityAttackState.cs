using UnityEngine;

[System.Serializable]
public class EntityAttackState : BaseAttackState
{
    protected override void HandleAnimationEvent()
    {
        if (hasTriggered) return;
        if (controller == null) return;
        
        Vector3 casterPosition = controller.transform.position;
        TryUpdateAttackDirection();
        Vector2 castDirection = attackDirection;
        
        Vector3 targetPosition = controller.currentTarget != null ? controller.currentTarget.position : casterPosition + (Vector3)castDirection;

        // 1. Create a primary payload describing the INITIAL CAST event
        EffectPayload initialCastPayload = new EffectPayload(
            user: controller.gameObject,
            target: controller.gameObject,                 // Default target is caster for instant self-effects
            targetPosition: targetPosition, // Target position is where the mouse is pointing
            hitDirection: castDirection,
            hitImpactPoint: casterPosition
            );
        
        // 2. Execute all spell effects
        if (attackDataInstance.Effects != null && attackDataInstance.Effects.Count > 0)
            foreach (Effect effect in attackDataInstance.Effects)
                effect.Execute(initialCastPayload);
        
        // 3. Apply Recoil if necessary
        if (attackDataInstance.SpellAnimation == AnimationBool.IsAttackingStrong) 
            controller.EntityMover.ApplyRecoil(castDirection);
        
        // 4. Apply VFX
        if (attackDataInstance.SpellPrefab != null)
        {
            Vector3 spawnPosition = controller._spellController.firePoint != null
                ? controller._spellController.firePoint.transform.position
                : casterPosition;
            
            float relativeX = targetPosition.x - spawnPosition.x;
            
            Quaternion spawnRotation = relativeX >= 0
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 180, 0);
            
            GameObject spellVFX = Object.Instantiate(attackDataInstance.SpellPrefab, spawnPosition, spawnRotation, controller.transform);
            Object.Destroy(spellVFX, 1f);
        }
        
        hasTriggered = true;
    }
}
