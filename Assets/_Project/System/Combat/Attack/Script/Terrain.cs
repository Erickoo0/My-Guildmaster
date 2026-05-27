using UnityEngine;

public class Terrain : MonoBehaviour
{
	private SpriteRenderer _spriteRenderer;

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Setup(Vector2 lookDirection, float hpMax, float terrainLifetime)
	{
		if (TryGetComponent(out Health hpComponent))
			hpComponent.hpMax = hpMax;
        
		Destroy(gameObject, terrainLifetime);
        
		HandleRotationAndFlipping(lookDirection);
	}

	private void HandleRotationAndFlipping(Vector2 direction)
	{
		// 1. Calculate the raw 360 angle around the Z axis
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
		// 2. Apply the rotation to the transform
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		// 3. Handle the smart flip to prevent the sprite from turning upside down
		if (_spriteRenderer != null)
		{
			// If the mouse is to the left of the spawn point 
			if (direction.x < 0)
			{
				// Flip the sprite on the Y axis to counteract the upside-down rotation
				_spriteRenderer.flipY = true;
			}
			else
			{
				_spriteRenderer.flipY = false;
			}
		}
	}
}
