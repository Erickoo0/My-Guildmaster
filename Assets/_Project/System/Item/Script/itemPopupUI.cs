using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles ItemPopUp data
/// </summary>
public class ItemPopup : MonoBehaviour
{
	public ItemInstance itemInstance;
	public Image itemIconDisplay;
	public TextMeshProUGUI itemName;
	public TextMeshProUGUI itemStackSize;

	[SerializeField] private float lifetime = 3f;

	private void Update()
	{
		if (itemInstance?.DataSo == null || !itemInstance.DataSo.IsAnimated)
			return;

		itemIconDisplay.sprite = GlobalHelper.GetAnimatedSprite(itemInstance.DataSo);
	}

	public void SetPopUp(ItemInstance newItem)
	{
		itemInstance = newItem;

		// Set the text
		itemName.text = itemInstance.DataSo.ItemName;
		itemStackSize.text = itemInstance.stackSize.ToString();

		Destroy(gameObject, lifetime);
	}
}
