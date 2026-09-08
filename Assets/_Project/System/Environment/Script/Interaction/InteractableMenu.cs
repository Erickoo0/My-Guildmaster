using UnityEngine;
/// <summary>
/// An interactable component for props to display a UI menu
/// </summary>
public class InteractableMenu : MonoBehaviour, IInteractable
{
	[SerializeField] private bool _isInteractable = true;

	[Header("References")]
	[SerializeField] private MenuType _menu;

	public bool CanInteract() => _isInteractable && _menu != MenuType.None;

	public void Interact(ControllerPlayer controllerPlayer)
	{
		if (!CanInteract())
			return;

		EventBus.RequestMenuToggle(_menu);
	}

	public void SetInteractable(bool value) => _isInteractable = value;
}
