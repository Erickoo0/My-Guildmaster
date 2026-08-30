/// <summary>
/// Defines the contract for any object in the game that the player can interact with
/// </summary>
public interface IInteractable
{
	public bool CanInteract();
	public void Interact(ControllerPlayer controllerPlayer);
}
