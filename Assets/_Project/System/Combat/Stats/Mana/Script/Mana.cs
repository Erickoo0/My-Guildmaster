using System;
using UnityEngine;
public class Mana : MonoBehaviour
{
	[Header("MP Settings")]
	[SerializeField] private float _mpBase = 100f;
	[SerializeField] private float _mpPerLvl = 10f;

	[Header("References")]
	[Tooltip("The actual object the mana component belongs to")]
	[SerializeField] private GameObject entityRoot;
	private float _mpCurrent;

	public float MpMax { get; private set; }

	public float MpCurrent
	{
		get => _mpCurrent;
		set
		{
			float mpPrevious = _mpCurrent;
			_mpCurrent = Mathf.Clamp(value, 0, MpMax);

			if (!Mathf.Approximately(_mpCurrent, mpPrevious))
			{
				OnMpUpdated?.Invoke();
			}
		}
	}

	private void Awake()
	{
		if (entityRoot == null) entityRoot = gameObject;

		MpMax = _mpBase;
		MpCurrent = MpMax;
	}

	public event Action OnMpUpdated;

	public void RecalculateMaxMp(int currentLevel)
	{
		MpMax = _mpBase + (currentLevel - 1)*_mpPerLvl;
		_mpCurrent = MpMax;
		OnMpUpdated?.Invoke();
	}

	public void MpHealInstant(float mpHealAmount)
	{
		MpCurrent += mpHealAmount;
	}

	public bool HasEnoughMp(float mpCost)
	{
		return MpCurrent >= mpCost;
	}

	public void ConsumeMp(float mpCost)
	{
		if (!HasEnoughMp(mpCost)) return;
		MpCurrent -= mpCost;
	}
}
