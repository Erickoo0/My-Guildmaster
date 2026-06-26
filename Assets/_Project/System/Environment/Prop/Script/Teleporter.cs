using System.Collections;
using UnityEngine;

public enum TeleportFacing { Up, Down, Left, Right }

public class Teleporter : MonoBehaviour
{
    [Header("Teleporter Settings")] 
    [SerializeField] private Teleporter targetDestinationTeleporter;
    [SerializeField] private GameLocation targetLocation;
    [SerializeField] private FacingDirection faceDirection;
    [SerializeField] private float teleportCooldown = 1f;
    private float _teleportCooldownTimer;
    
    public FacingDirection GetFacingDirection() => faceDirection;
    
    private void Update()
    {
        if (_teleportCooldownTimer > 0f)
            _teleportCooldownTimer -= Time.deltaTime;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Validation checks
        if (!other.CompareTag("SpellControllerPlayer") || _teleportCooldownTimer > 0 || other.isTrigger) return;
        
        if (targetDestinationTeleporter == null)
        {
            Debug.LogWarning($"Teleporter on {gameObject.name} is missing a target destination!");
            return;
        }
        
        // 2. Teleport
        StartCoroutine(TeleportSequence(other));
    }

    private IEnumerator TeleportSequence(Collider2D other)
    {
        // 1. Start fade out animation
        var controller = other.GetComponent<PlayerController>();
        if (FadeController.Instance != null)
        {
            // Stop player from moving during the animation
            controller.SetCanMove(false);
            yield return FadeController.Instance.FadeOut();
        }
        
        // 2. Teleport the player
        other.transform.position = targetDestinationTeleporter.transform.position;
        
        // 3. Update location
        if (LocationManager.Instance != null)
            LocationManager.Instance.UpdateLocation(targetDestinationTeleporter.targetLocation);

        
        // 4. Set the face direction 
        controller.EntityAnimator.FaceDirection(targetDestinationTeleporter.GetFacingDirection());
        
        // 5. Set the cooldown for BOTH teleporters
        SetTeleporterCooldown();
        targetDestinationTeleporter.SetTeleporterCooldown();
        
        
        // 6. Give the camera some time to catch up to the new position
        yield return new WaitForSeconds(0.1f);
        
        // 7. Start fade in animation
        if (FadeController.Instance != null)
        {
            yield return FadeController.Instance.FadeIn();
            controller.SetCanMove(true);
        }
    }
    
    private void SetTeleporterCooldown() => _teleportCooldownTimer = teleportCooldown;
}
