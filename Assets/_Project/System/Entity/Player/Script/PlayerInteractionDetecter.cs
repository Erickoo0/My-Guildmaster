using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteractionDetecter : MonoBehaviour
{
	[SerializeField] private GameObject interactIcon;

	private readonly Dictionary<IInteractable, int> _interactablesInRange = new Dictionary<IInteractable, int>();
	private IInteractable _interactableTarget;

	private void Update()
	{
		UpdateTarget();
	}

	//

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!TryGetInteractable(other, out IInteractable interactable)) return;

		if (_interactablesInRange.TryGetValue(interactable, out int overlapCount))
			_interactablesInRange[interactable] = overlapCount + 1;
		else
			_interactablesInRange.Add(interactable, 1);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (!TryGetInteractable(other, out IInteractable interactable)) return;

		if (!_interactablesInRange.TryGetValue(interactable, out int overlapCount)) return;

		overlapCount--;
		if (overlapCount <= 0)
			_interactablesInRange.Remove(interactable);
		else
			_interactablesInRange[interactable] = overlapCount;
	}

	private void UpdateTarget()
	{
		// 1. Clean references that are no longer valid
		var invalidInteractables = _interactablesInRange.Keys
			.Where(i => i == null || ((MonoBehaviour)i) == null)
			.ToList();

		foreach (IInteractable invalidInteractable in invalidInteractables)
			_interactablesInRange.Remove(invalidInteractable);

		// 2. Filter a temporary list of things we can currently interact with
		var validInteractables = _interactablesInRange.Keys.Where(i => i.CanInteract()).ToList();

		if (validInteractables.Count == 0)
		{
			_interactableTarget = null;
			interactIcon.SetActive(false);
			return;
		}

		// 2. Set the target to the closest target
		_interactableTarget = validInteractables
			.OrderBy(i => Vector2.Distance(transform.position, GetInteractablePosition(i)))
			.FirstOrDefault();

		interactIcon.SetActive(true);
	}

	private static bool TryGetInteractable(Collider2D other, out IInteractable interactable)
	{
		if (other.TryGetComponent(out interactable)) return true;

		interactable = other.GetComponentInParent<IInteractable>();
		if (interactable != null) return true;

		interactable = other.GetComponentInChildren<IInteractable>();
		return interactable != null;
	}

	private static Vector2 GetInteractablePosition(IInteractable interactable)
	{
		if (interactable is IInteractionPointProvider interactionPointProvider)
			return interactionPointProvider.GetInteractionPoint();

		if (interactable is MonoBehaviour interactableBehaviour)
			return interactableBehaviour.transform.position;

		return Vector2.zero;
	}

	public void OnInteract(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		// 1. Check and Trigger Interaction
		if (_interactableTarget != null && _interactableTarget.CanInteract())
		{
			ControllerPlayer controllerPlayer = GetComponent<ControllerPlayer>();
			_interactableTarget.Interact(controllerPlayer);
		}

		// 2. Item Use
		else if (PlayerEquipment.Instance != null)
			PlayerEquipment.Instance.TryUseActiveItem();

	}
}
