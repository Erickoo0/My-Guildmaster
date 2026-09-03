using TMPro;
using UnityEngine;
/// <summary>
/// Handles the display of SkillData live cast time
/// </summary>
public class CastBar : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject castBarPrefab;
	[SerializeField] private Vector3 offset = new Vector3(0, 3f, 0);
	private GameObject _castBarInstance;
	private float _currentCastTime;

	// Tracking variables 
	private bool _isCasting;
	private float _maxCastTime;
	private SpriteRenderer _spriteRenderer;
	private TextMeshProUGUI castText;

	[Header("UI")]
	private ProgressBar progressBarRef;


	private void Awake()
	{
		_spriteRenderer = GetComponentInChildren<SpriteRenderer>();

		// Spawn the bar immediately, but keep it hidden
		if (progressBarRef == null && castBarPrefab != null)
			SpawnCastBar();

		HideBar();
	}

	private void Update()
	{
		if (!_isCasting || progressBarRef == null) return;

		_currentCastTime += Time.deltaTime;

		float remainingTime = Mathf.Max(0, _maxCastTime - _currentCastTime);
		float progress = Mathf.Min(_currentCastTime, _maxCastTime);
		progressBarRef.SetValues(progress, _maxCastTime, "");
		if (castText != null) castText.text = $"{remainingTime:F1}s";
	}

	public void BeginCast(float castDuration, string spellName = "")
	{
		if (_castBarInstance == null || castDuration <= 0) return;

		_maxCastTime = castDuration;
		_currentCastTime = 0f;
		_isCasting = true;

		_castBarInstance.SetActive(true);
		progressBarRef.SetValues(0, _maxCastTime, "");
		if (castText != null) castText.text = $"{_maxCastTime:f1}";
	}

	public void StopCast()
	{
		_isCasting = false;
		if (_castBarInstance != null) _castBarInstance.SetActive(false);
	}

	private void SpawnCastBar()
	{
		_castBarInstance = Instantiate(castBarPrefab, _spriteRenderer.transform);
		_castBarInstance.transform.localPosition = offset;

		progressBarRef = _castBarInstance.GetComponent<ProgressBar>();
		castText = _castBarInstance.GetComponentInChildren<TextMeshProUGUI>();
	}

	public void ShowBar(float maxCastTime, string spellName = "")
	{
		if (_castBarInstance == null) return;
		_castBarInstance.SetActive(true);
		progressBarRef.SetValues(0, maxCastTime, spellName);
	}

	public void UpdateProgress(float currentTime, float maxCastTime, string timeText = "")
	{
		if (progressBarRef == null) return;
		progressBarRef.SetValues(currentTime, maxCastTime, "");
		if (castText != null) castText.text = timeText;
	}

	public void HideBar()
	{
		if (_castBarInstance == null) return;
		_castBarInstance.SetActive(false);
	}
}
