using System;
using Unity.Cinemachine;
using UnityEngine;
public class CameraManager : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private CinemachineConfiner2D _confiner;

	[Header("Location Bounds")]
	[SerializeField] private LocationBound[] _locationBounds;

	private void Start()
	{
		LocationManager.Instance.OnLocationChanged += HandleLocationChanged;
		HandleLocationChanged(LocationManager.Instance.CurrentLocation);
	}

	private void OnDisable() => LocationManager.Instance.OnLocationChanged -= HandleLocationChanged;

	private void HandleLocationChanged(GameLocation location)
	{
		Collider2D cameraBound = GetBoundsForLocation(location);
		if (cameraBound == null)
		{
			Debug.LogWarning($"No bounds found for location: {location}");
			return;
		}

		_confiner.BoundingShape2D = cameraBound;
		_confiner.InvalidateBoundingShapeCache();
	}

	private Collider2D GetBoundsForLocation(GameLocation location)
	{
		for (int i = 0; i < _locationBounds.Length; i++)
		{
			if (_locationBounds[i].Location == location)
				return _locationBounds[i].Bounds;
		}

		return null;
	}
	[Serializable]
	private class LocationBound
	{
		public GameLocation Location;
		public Collider2D Bounds;
	}
}
