using UnityEngine;
/// <summary>
/// An optional interaction point for IInteractable objects to implement
/// </summary>
public interface IInteractionPointProvider
{
	public Vector2 GetInteractionPoint();
}
