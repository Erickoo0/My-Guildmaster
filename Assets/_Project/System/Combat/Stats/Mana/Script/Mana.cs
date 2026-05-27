using UnityEngine;
using System;

public class Mana : MonoBehaviour
{
	[Header("MP Settings")]
	[SerializeField] private float mpBase = 100f;
	[SerializeField] private float mpPerLvl = 10f;
    
	public float mpMax { get; private set; }
	private float _mpCurrent;
    
	[Header("References")] 
	[Tooltip("The actual object the mana component belongs to")] 
	[SerializeField] private GameObject entityRoot;
    
	public event Action OnMpUpdated;
    
	public float MpCurrent
	{
		get => _mpCurrent;
		set
		{
			float mpPrevious = _mpCurrent;
			_mpCurrent = Mathf.Clamp(value, 0, mpMax);

			if (!Mathf.Approximately(_mpCurrent, mpPrevious))
			{
				OnMpUpdated?.Invoke();
			}
		}
	}
    
	private void Awake()
	{
		if (entityRoot == null) entityRoot = gameObject;
        
		mpMax = mpBase;
		MpCurrent = mpMax;
	}
    
	public void RecalculateMaxMp(int currentLevel)
	{
		mpMax = mpBase + (currentLevel - 1) * mpPerLvl;
		_mpCurrent = mpMax;
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
