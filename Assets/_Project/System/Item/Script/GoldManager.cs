using TMPro;
using UnityEngine;
/// <summary>
/// Handles currency
/// </summary>
public class GoldManager : MonoBehaviour, ISaveable
{
	[SerializeField] private TextMeshProUGUI goldText;

	private int playerGold = 0;
	public static GoldManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		goldText.text = playerGold.ToString();
	}

	public void PopulateSaveData(SaveData saveData)
	{
		saveData.GoldCurrent = playerGold;
	}

	public void LoadFromSaveData(SaveData saveData)
	{
		playerGold = saveData.GoldCurrent;
	}

	public void AddGold(int amount)
	{
		playerGold += amount;
		goldText.text = playerGold.ToString();
	}

	public void RemoveGold(int amount)
	{
		playerGold -= amount;
		goldText.text = playerGold.ToString();
	}
}
