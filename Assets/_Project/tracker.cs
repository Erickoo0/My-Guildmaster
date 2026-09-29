using UnityEngine;

// This single line fixes the race condition with Cinemachine
[DefaultExecutionOrder(-100)]
public class PixelPerfectProxy : MonoBehaviour
{
	public Transform playerTarget;

	[Header("Camera Speed (Lower is faster)")]
	public float smoothTime = 0.15f;

	private Vector2 velocity = Vector2.zero;

	void Start()
	{
		if (playerTarget != null)
		{
			transform.position = new Vector3(playerTarget.position.x, playerTarget.position.y, transform.position.z);
		}
	}

	void LateUpdate()
	{
		if (playerTarget == null) return;

		// 1. Smoothly track the player using continuous float math
		Vector2 newPos = Vector2.SmoothDamp(transform.position, playerTarget.position, ref velocity, smoothTime);

		// 2. Micro-snap to kill the infinite sub-pixel crawl.
		// 0.005 units is incredibly microscopic. It stops the math curve 
		// from running infinitely without the human eye ever seeing the jump.
		if (Vector2.Distance(newPos, playerTarget.position) < 0.005f)
		{
			newPos = playerTarget.position;
			velocity = Vector2.zero;
		}

		// 3. Apply position while strictly preserving your Z-axis (prevents 3D camera fighting)
		transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
	}
}
