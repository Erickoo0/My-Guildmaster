using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
/// <summary>
/// Listens to the EventBus for hit impact requests and applies:
/// 1. A brief time freeze (hit pause) proportional to the hitImpact value.
/// 2. A camera shake via the referenced CinemachineImpulseSource.
/// Place on the player object alongside a CinemachineImpulseSource configured for hit feedback.
/// </summary>
public class HitPauseController : MonoBehaviour
{
	[Header("Hit Pause Settings")]
	[Tooltip("Seconds of freeze per 1.0 hitImpact. At 0.05 and hitImpact=1.0, this is ~3 frames at 60fps.")]
	[SerializeField] private float _pauseScale = 0.05f;

	[Header("Screen Shake")]
	[Tooltip("CinemachineImpulseSource configured for hit impact shake. Separate from cast shake.")]
	[SerializeField] private CinemachineImpulseSource _impulseSource;

	private Coroutine _activePause;

	private void Awake()
	{
		EventBus.OnHitImpactRequested += HandleHitImpact;
	}

	private void OnDestroy()
	{
		EventBus.OnHitImpactRequested -= HandleHitImpact;
	}

	private void HandleHitImpact(float hitImpact, Vector3 position)
	{
		if (hitImpact <= 0f) return;

		// 1. Hit Pause (brief time freeze)
		float pauseDuration = hitImpact*_pauseScale;
		if (_activePause != null) StopCoroutine(_activePause);
		_activePause = StartCoroutine(PauseRoutine(pauseDuration));

		// 2. Screen Shake (camera impulse at hit position)
		if (_impulseSource != null)
			_impulseSource.GenerateImpulseAt(position, Vector3.up*hitImpact);
	}

	private IEnumerator PauseRoutine(float duration)
	{
		Time.timeScale = 0f;
		// WaitForSecondsRealtime uses unscaled time, so it works while timeScale is 0
		yield return new WaitForSecondsRealtime(duration);
		Time.timeScale = 1f;
		_activePause = null;
	}
}
