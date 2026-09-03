using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Manager for syncing other components and other scripts to the Action Map
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
	public static PlayerInputManager Instance { get; private set; }

	public PlayerInput PlayerInput { get; private set; }

	private void Awake()
	{
		// Destroy duplicates if they accidentally get created
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		// Set the static instance to this specific object
		Instance = this;

		// Grab the component
		PlayerInput = GetComponent<PlayerInput>();
	}
}
