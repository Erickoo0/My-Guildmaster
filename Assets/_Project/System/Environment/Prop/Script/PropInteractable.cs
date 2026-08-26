using UnityEngine;
using UnityEngine.Events;
public class PropInteractable : MonoBehaviour, IInteractable, IInteractionPointProvider
{
	[SerializeField] private bool isInteractable = true;
	[SerializeField] private Transform interactionPoint;
	[SerializeField] private UnityEvent onInteract;

	public bool CanInteract() => isInteractable;

	public void Interact(ControllerPlayer controllerPlayer)
	{
		if (!CanInteract()) return;
		onInteract?.Invoke();
	}

	public Vector2 GetInteractionPoint()
	{
		if (interactionPoint != null)
			return interactionPoint.position;

		return transform.position;
	}

	public void SetInteractable(bool value)
	{
		isInteractable = value;
	}
}
