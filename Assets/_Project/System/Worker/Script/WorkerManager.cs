using UnityEngine;
/// <summary>
/// Manages worker logic and sustenance
/// </summary>
public class WorkerManager : MonoBehaviour
{
	public static WorkerManager Instance { get; private set; }

	public int CurrentSustenance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void Start()
	{
		ItemStoragePlayer.Instance.OnSlotUpdated += HandleInventoryChanged;

		RecalculateSustenance();
	}

	private void OnDestroy() => ItemStoragePlayer.Instance.OnSlotUpdated -= HandleInventoryChanged;

	private void HandleInventoryChanged(int slotIndex) => RecalculateSustenance();

	private void RecalculateSustenance()
	{
		int newSustenance = 0;

		for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
		{
			ItemInstance item = ItemStoragePlayer.Instance.GetItem(i);

			if (item == null || item.DataSo == null)
				continue;

			// Check for any items with a food property
			if (item.DataSo.TryGetProperty(out ItemPropertyFood foodProperty))
				newSustenance += foodProperty.SustenanceValue*item.stackSize;
		}

		// If value has changed, broadcast it
		if (newSustenance != CurrentSustenance)
		{
			CurrentSustenance = newSustenance;
			EventBus.RequestTotalSustenanceChanged(CurrentSustenance);
		}
	}
}
