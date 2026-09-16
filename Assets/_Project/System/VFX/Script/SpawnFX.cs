using UnityEngine;
/// <summary>
/// Handles the logic of the entity spawn vfx
/// </summary>
public class SpawnFX : MonoBehaviour
{
	private float _despawnTime;
	private float _despawnTimeMax;

	private void Update()
	{
		if (_despawnTime <= 0)
		{
			Destroy(gameObject);
		} else
		{
			_despawnTime -= Time.deltaTime;
		}
	}

	public void SetupSpawnFX(float lifetime)
	{
		_despawnTimeMax = lifetime;
		_despawnTime = _despawnTimeMax;
	}
}
