using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InventoryTest : MonoBehaviour
{
	[Header("Test Items")]
	[SerializeField] private List<ItemDataSo> _testItemsList = new List<ItemDataSo>();

	private GameObject _player;

	private void Start() => _player = GameObject.FindGameObjectWithTag("Player");

	public void SpawnTestItem(InputAction.CallbackContext ContextMenu)
	{
		// Safety Checks
		if (!ContextMenu.performed)
			return;

		if (_testItemsList == null || _testItemsList.Count == 0 || _player == null)
			return;

		// 1. Pick a random item from the list
		ItemDataSo selectedItem = _testItemsList[Random.Range(0, _testItemsList.Count)];

		// 2. Create the data
		ItemInstance newItemInstance = new ItemInstance(selectedItem, 1);

		// 3. Get the prefab
		GameObject newItemPrefab = selectedItem.ItemObject != null ? selectedItem.ItemObject : ItemStoragePlayer.Instance.DefaultItemObjectPrefab;

		// 4. Spawn the item
		GameObject newItemObject = Instantiate(newItemPrefab, _player.transform.position, Quaternion.identity);
		if (newItemObject.TryGetComponent(out ItemObject itemObject))
			itemObject.SetItemObject(newItemInstance);


	}
}
