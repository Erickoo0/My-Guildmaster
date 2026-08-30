using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class LightProp : MonoBehaviour
{
	[SerializeField] private int turnOffHour = 6;
	[SerializeField] private int turnOnHour = 20;
	private Light2D _light;

	private void Awake()
	{
		_light = GetComponent<Light2D>();
	}


	private void OnEnable()
	{
		// Subscribe to the time update event
		EventBus.OnWorldTimeChanged += HandleTimeUpdate;
	}

	private void OnDisable()
	{
		// Unsubscribe to prevent memory leaks
		EventBus.OnWorldTimeChanged -= HandleTimeUpdate;
	}

	private void HandleTimeUpdate(object sender, TimeSpan currentTime)
	{
		int currentHour = currentTime.Hours;

		// Logic for spanning across midnight (e.g., 20:00 to 06:00)
		if (turnOnHour > turnOffHour)
		{
			_light.enabled = (currentHour >= turnOnHour || currentHour < turnOffHour);
		}
		// Logic for normal daytime range (e.g., 06:00 to 20:00)
		else
		{
			_light.enabled = (currentHour >= turnOnHour && currentHour < turnOffHour);
		}
	}
}
