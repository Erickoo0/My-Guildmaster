using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles displaying a single required AlchemyResourceSlotUI in the alchemy menu
/// </summary>
public class AlchemyResourceSlotUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image _icon;
	[SerializeField] private TextMeshProUGUI _name;
	[SerializeField] private TextMeshProUGUI _amount;

	public ItemDataSo ResourceData { get; private set; }

	public void Setup(ItemDataSo itemData, int requiredAmount, int currentAmount)
	{
		ResourceData = itemData;

		_icon.sprite = itemData.ItemIcon[0];
		_name.text = itemData.ItemName;

		string colorTag = currentAmount >= requiredAmount ? "<color=#ffffff>" : "<color=#ff4444>";
		_amount.text = $"{colorTag}{currentAmount} / {requiredAmount}</color>";
	}
}
