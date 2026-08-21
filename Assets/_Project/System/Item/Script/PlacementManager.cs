using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
/// <summary>
/// Handles placement of prop items in the world
/// </summary>
public class PlacementManager : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private Grid _worldGrid;
	[SerializeField] private SpriteRenderer _ghostRenderer;
	[SerializeField] private SpriteRenderer _cellHighlightRenderer;

	[SerializeField] private LayerMask _obstacleLayers;

	private ItemInstance _activeItem;
	private int _activeSlotIndex;
	private Camera _mainCamera;
	public static PlacementManager Instance { get; private set; }


	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		_mainCamera = Camera.main;
		_ghostRenderer.enabled = false;
		_cellHighlightRenderer.enabled = false;
	}

	private void Start() => InventoryManager.Instance.OnActiveSlotIndexChanged += UpdateActiveItem;

	private void Update()
	{
		// Only update if the ghost is enabled
		if (!_ghostRenderer.enabled)
			return;

		// 1. EXTRACT PROPERTY FIRST (This fixes the compiler error!)
		if (!_activeItem.DataSo.TryGetProperty<ItemPropertyPlaceable>(out ItemPropertyPlaceable placeableProperty))
			return;

		// 2. Get the snapped world position
		Vector3 placementPosition = GetMouseWorldPosition();

		if (placeableProperty.SnapToGrid)
			placementPosition = SnapToGrid(placementPosition);

		// 3. Move ghost to position
		_ghostRenderer.transform.position = placementPosition;
		_cellHighlightRenderer.transform.position = placementPosition;

		// 4. Determine if placement is valid 
		bool isCorrectLocation = LocationManager.Instance.CurrentLocation == GameLocation.Player_Guild;
		bool isNotHoveringUI = !EventSystem.current.IsPointerOverGameObject();

		// 5. Check if anything physical is blocking the tile
		Vector2Int calculatedSize = placeableProperty.GetGridSize(_activeItem.DataSo.ItemIcon[0]);
		Vector2 physicsBoxSize = new Vector2(calculatedSize.x - 0.1f, calculatedSize.y - 0.1f);
		Collider2D obstacle = Physics2D.OverlapBox(placementPosition, physicsBoxSize, 0f, _obstacleLayers);
		bool isSpaceClear = obstacle == null;

		bool isValid = isCorrectLocation && isNotHoveringUI && isSpaceClear;

		// 6. Tint the ghost based on validity
		_ghostRenderer.color = isValid ? new Color(1f, 1f, 1f, 0.7f) : new Color(1f, 1f, 1f, 0.3f);
		_cellHighlightRenderer.color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);

		// 7. Detect click to place item
		if (isValid && Mouse.current.leftButton.wasPressedThisFrame)
			PlaceItem(placeableProperty, placementPosition);
	}

	private void OnDestroy()
	{
		if (InventoryManager.Instance != null)
			InventoryManager.Instance.OnActiveSlotIndexChanged -= UpdateActiveItem;
	}

	private void UpdateActiveItem(int slotIndex)
	{
		_activeItem = InventoryManager.Instance.itemsList[slotIndex];
		_activeSlotIndex = slotIndex;
		UpdateGhostVisuals();
	}

	private void UpdateGhostVisuals()
	{
		// Hides ghost by default
		_ghostRenderer.enabled = false;
		_cellHighlightRenderer.enabled = false;

		// Safety Check
		if (_activeItem == null || _activeItem.DataSo == null)
			return;

		// If the item is placeable, then enable the ghost
		if (_activeItem.DataSo.TryGetProperty<ItemPropertyPlaceable>(out ItemPropertyPlaceable placeableProperty))
		{
			_ghostRenderer.sprite = _activeItem.DataSo.ItemIcon[0];
			_ghostRenderer.enabled = true;

			if (placeableProperty.SnapToGrid)
			{
				_cellHighlightRenderer.enabled = true;
				Vector2Int calculatedSize = placeableProperty.GetGridSize(_activeItem.DataSo.ItemIcon[0]);
				_cellHighlightRenderer.transform.localScale = new Vector3(calculatedSize.x, calculatedSize.y, 1f);
			}

		}
	}

	private Vector3 GetMouseWorldPosition()
	{
		Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
		Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, _mainCamera.nearClipPlane));
		mouseWorldPosition.z = 0;
		return mouseWorldPosition;
	}

	private Vector3 SnapToGrid(Vector3 rawWorldPosition)
	{
		// Ask the grid what cell this world position belongs to
		Vector3Int cellPosition = _worldGrid.WorldToCell(rawWorldPosition);
		// Get the exact center of that cell
		return _worldGrid.GetCellCenterWorld(cellPosition);
	}

	private void PlaceItem(ItemPropertyPlaceable placeableProperty, Vector3 position)
	{
		// Spawn the prefab
		Instantiate(placeableProperty.PlaceablePrefab, position, Quaternion.identity);

		// Consume the item from the inventory
		InventoryManager.Instance.RemoveItems(_activeSlotIndex);

		// Reevaluate the active item
		_activeItem = InventoryManager.Instance.itemsList[_activeSlotIndex];
		UpdateGhostVisuals();
	}
}
