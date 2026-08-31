using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Generic interactable component for all prop objects
/// Hooks into UnityEvents to easily assign responses in the inspector.
/// </summary>
public class InteractableProp : MonoBehaviour, IInteractable, IInteractionPointProvider
{
	[SerializeField] private bool isInteractable = true;
	[Tooltip("Optional. If null, will use the object's transform position.")]
	[SerializeField] private Transform interactionPoint;
	[Tooltip("The method to call when the object is interacted with.")]
	[SerializeField] private UnityEvent onInteract;

	public bool CanInteract() => isInteractable;

	public void Interact(ControllerPlayer controllerPlayer)
	{
		if (!CanInteract()) return;
		onInteract?.Invoke(); // Triggers whatever functionality is hooked up in the Unity Editor
	}

	public Vector2 GetInteractionPoint()
	{
		if (interactionPoint != null)
			return interactionPoint.position;

		return transform.position;
	}

	/// <summary>
	/// Used to set the interactable state of the object.
	/// </summary>
	public void SetInteractable(bool value)
	{
		isInteractable = value;
	}
}
