using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteractionDetecter : MonoBehaviour
{
	[SerializeField] private GameObject interactIcon;

	private List<IInteractable> _interactablesInRange = new List<IInteractable>();
	private IInteractable _interactableTarget;

	private void Update()
	{
		UpdateTarget();
	}

	//

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out IInteractable interactable))
		{
			if (!_interactablesInRange.Contains(interactable))
			{
				_interactablesInRange.Add(interactable);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.TryGetComponent(out IInteractable interactable))
		{
			_interactablesInRange.Remove(interactable);
		}
	}

	private void UpdateTarget()
	{
		// 1. Clean the list of objects that are null
		_interactablesInRange.RemoveAll(i => i == null || ((MonoBehaviour)i) == null);

		// 2. Filter a temporary list of things we can currently interact with
		var validInteractables = _interactablesInRange.Where(i => i.CanInteract()).ToList();

		if (validInteractables.Count == 0)
		{
			_interactableTarget = null;
			interactIcon.SetActive(false);
			return;
		}

		// 2. Set the target to the closest target
		_interactableTarget = validInteractables
			.OrderBy(i => Vector2.Distance(transform.position, ((MonoBehaviour)i).transform.position))
			.FirstOrDefault();

		interactIcon.SetActive(true);
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
