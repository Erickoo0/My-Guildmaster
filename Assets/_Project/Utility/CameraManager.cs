using System;
using Unity.Cinemachine;
using UnityEngine;
public class CameraManager : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private CinemachineConfiner2D _confiner;
	[SerializeField] private CinemachineCamera _camera;

	[Header("Location Bounds")]
	[SerializeField] private LocationData[] _locationBounds;

	private void Start()
	{
		LocationManager.Instance.OnLocationChanged += HandleLocationChanged;
		HandleLocationChanged(LocationManager.Instance.CurrentLocation);
	}

	private void OnDisable() => LocationManager.Instance.OnLocationChanged -= HandleLocationChanged;

	private void HandleLocationChanged(GameLocation location)
	{
		LocationData locationData = GetLocationData(location);
		if (locationData == null)
		{
			Debug.LogWarning($"No camera data found for location: {location}");
			return;
		}

		// 1. Update camera bounds
		_confiner.BoundingShape2D = locationData.Bounds;
		_confiner.InvalidateBoundingShapeCache();

		// 2. Update camera zoom
		_camera.Lens.OrthographicSize = locationData.ZoomLevel;
	}

	private LocationData GetLocationData(GameLocation location)
	{
		for (int i = 0; i < _locationBounds.Length; i++)
		{
			if (_locationBounds[i].Location == location)
				return _locationBounds[i];
		}

		return null;
	}
	[Serializable]
	private class LocationData
	{
		public GameLocation Location;
		public Collider2D Bounds;
		public float ZoomLevel = 10f;
	}
}
