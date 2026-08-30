using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ShopManager : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private GameObject shopMainPanel;
	[SerializeField] private GameObject shopItemPanel;
	[SerializeField] private GameObject shopItemPrefab;
	public static ShopManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null & Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	public void SetupShop(object data)
	{
		// Cast the object back to an array Type
		ItemDataSo[] shopList = data as ItemDataSo[];

		// Safety Check
		if (shopList == null || shopList.Length == 0) return;

		CreateShopItems(shopList);

		// Send request to UIManager to handle the showing of UI
		EventBus.RequestOpenMenu(shopMainPanel);

	}

	private void CreateShopItems(ItemDataSo[] shopList)
	{
		// Destroy old items
		foreach (Transform child in shopItemPanel.transform) Destroy(child.gameObject);

		foreach (ItemDataSo shopItemData in shopList)
		{
			// Create the button and get the components
			GameObject shopItem = Instantiate(shopItemPrefab, shopItemPanel.transform);
			var iconComponent = shopItem.transform.Find("Item Icon").GetComponent<Image>();
			var nameComponent = shopItem.transform.Find("Item Name").GetComponent<TextMeshProUGUI>();
			var buttonComponent = shopItem.GetComponent<Button>();

			// Update the data
			iconComponent.sprite = shopItemData.ItemIcon[0];
			nameComponent.text = shopItemData.ItemName;

			// Add click event listener to button
			buttonComponent.onClick.AddListener(() =>
			{
				ItemInstance newPurchase = new ItemInstance(shopItemData, 1);
				ItemStoragePlayer.Instance.AddItems(newPurchase);
			});
		}
	}
}
