using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("Attack Library")]
    [SerializeField] private List<AttackData> attackLibrary;
    
    private PlayerController _playerController;
    private Camera _mainCam;
    private Vector2 _rawMousePosition;
    public CombatContext CombatContext { get; private set; } = new CombatContext();

    private void Awake()
    {
        _mainCam = Camera.main;
        _playerController = GetComponent<PlayerController>();
    }
    
    
    //Tracks Mouse Movement
    public void OnPoint(InputAction.CallbackContext context)
    {
        _rawMousePosition = context.ReadValue<Vector2>();
    }
    
    public T GetAttackData<T>(string id) where T : AttackData
    {
        // Search the library for a piece of data that:
        // 1. Matches the ID string
        // 2. Is of the type (T) we are looking for
        return attackLibrary.OfType<T>().FirstOrDefault(data => data.attackID == id);
    }

    public void OnMouseClick(InputAction.CallbackContext context)
    { 
        if (!context.performed) return;
        
        // Calculate the distance between the camera and the world plane (Z=0)
        // If camera is at -11, this becomes 11.
        float distanceToPlane = Mathf.Abs(_mainCam.transform.position.z);
        
        // Create the projection vector using that distance
        Vector3 mouseInput = new Vector3(_rawMousePosition.x, _rawMousePosition.y, distanceToPlane);
        
        // Convert to world space
        Vector3 worldMousePos = _mainCam.ScreenToWorldPoint(mouseInput);
        
        worldMousePos.z = 0f;
        
        // Gather the context 
        CombatContext combatContext = CombatContext;
        combatContext.source = gameObject;
        combatContext.mousePosition = worldMousePos;
        combatContext.userPosition = transform.position;
        combatContext.facingDirection = transform.right;
        CombatContext = combatContext;
        
        // Execute attack module
        _playerController.StateMachine.ChangeState(_playerController.MouseAttackState);
    }
}
