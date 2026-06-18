using UnityEngine;

public class PS_VFX_Trigger_Script : MonoBehaviour
{
// WARNING: Replace 'YourControllerClassName' with the actual name of your controller script
    [Header("References")]
    private PlayerController controller; 

    private void Awake() => controller = GetComponent<PlayerController>();
    
    void Update()
    {
        // Safety check to avoid null reference errors if the controller isn't assigned
        if (controller == null) return;

        // 1. Get the mouse position from your controller
        Vector3 mousePosition = controller.WorldMousePosition;

        // 2. Calculate the relative position on the X-axis (Right vs. Left)
        float relativeX = mousePosition.x - transform.position.x;

        // 3. Keep the current X and Z rotations so we don't mess up any 2D tilt you have going on
        Vector3 currentRotation = transform.localEulerAngles;

        // 4. Flip the Y rotation based on whether the relative X is positive or negative
        if (relativeX >= 0)
        {
            // Mouse is to the right (positive) -> Set Y rotation to 0
            transform.localEulerAngles = new Vector3(currentRotation.x, 0f, currentRotation.z);
        }
        else
        {
            // Mouse is to the left (negative) -> Set Y rotation to 180
            transform.localEulerAngles = new Vector3(currentRotation.x, 180f, currentRotation.z);
        }
    }
}
