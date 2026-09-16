using UnityEngine;
/// <summary>
/// Handles ItemPopupUI notifications 
/// </summary>
public class ItemPopupManager : MonoBehaviour
{
	[Header("Visual References")]
	public GameObject itemPopupUI;
	public Transform popupParent; // Vertical Layout Group

	private void Start()
	{
		ItemStoragePlayer.Instance.OnItemAddedToInventory += SpawnPopup;
	}

	private void OnDestroy()
	{
		if (ItemStoragePlayer.Instance != null)
			ItemStoragePlayer.Instance.OnItemAddedToInventory -= SpawnPopup;
	}

	private void SpawnPopup(ItemInstance itemInstance)
	{
		GameObject newPopup = Instantiate(itemPopupUI, popupParent);
		newPopup.GetComponent<ItemPopup>().SetPopUp(itemInstance);
	}
}
