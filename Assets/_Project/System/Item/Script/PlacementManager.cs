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
	[SerializeField] private Transform _propContainer;

	[Header("Rules")]
	[SerializeField] private LayerMask _obstacleLayers;

	private ItemInstance _activeItem;
	private int _activeSlotIndex;
	private Vector2Int _currentCellSize;
	private Camera _mainCamera;
	public static PlacementManager Instance { get; private set; }

	// Checks if the currently active item is placeable 
	// Used for external input validation.
	public bool IsPlacementMode => _activeItem != null && _activeItem.DataSo.TryGetProperty<ItemPropertyPlaceable>(out ItemPropertyPlaceable placeableProperty);

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

		// 1. Check for property
		if (!_activeItem.DataSo.TryGetProperty<ItemPropertyPlaceable>(out ItemPropertyPlaceable placeableProperty))
			return;

		// 2. Calculate positions
		Vector3 placementPosition = GetMouseWorldPosition();
		if (placeableProperty.SnapToGrid)
			placementPosition = SnapToGrid(placementPosition);

		Vector3 objectPosition = placementPosition + new Vector3(0f, -0.5f, 0f);
		Vector3 centerBoxPosition = objectPosition + new Vector3(0f, _currentCellSize.y*0.5f, 0f);

		// 3. Move visuals
		_ghostRenderer.transform.position = objectPosition;
		_cellHighlightRenderer.transform.position = centerBoxPosition;

		// 4. Check if placement is valid 
		bool isValid = IsPlacementValid(centerBoxPosition);


		// 5. Tint the ghost based on validity
		_ghostRenderer.color = isValid ? new Color(1f, 1f, 1f, 0.7f) : new Color(1f, 1f, 1f, 0.3f);
		_cellHighlightRenderer.color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);

		// 6. Detect click to place item
		if (isValid && Mouse.current.leftButton.wasPressedThisFrame)
			PlaceItem(placeableProperty, objectPosition);
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
			_currentCellSize = placeableProperty.GetGridSize(_activeItem.DataSo.ItemIcon[0]);

			if (placeableProperty.SnapToGrid)
			{
				_cellHighlightRenderer.enabled = true;
				_cellHighlightRenderer.transform.localScale = new Vector3(_currentCellSize.x, _currentCellSize.y, 1f);
			}
		}
	}

	private bool IsPlacementValid(Vector3 placementPosition)
	{
		if (LocationManager.Instance.CurrentLocation != GameLocation.Player_Guild)
			return false;

		if (EventSystem.current.IsPointerOverGameObject())
			return false;

		// Calculate collision box
		Vector2 collisionBoxSize = new Vector2(_currentCellSize.x - 0.1f, _currentCellSize.y - 0.1f);
		// Safety net to ensure the physics box never becomes 0 or negative
		collisionBoxSize.x = Mathf.Max(0.1f, collisionBoxSize.x);
		collisionBoxSize.y = Mathf.Max(0.1f, collisionBoxSize.y);

		Collider2D obstacle = Physics2D.OverlapBox(placementPosition, collisionBoxSize, 0f, _obstacleLayers);

		return obstacle == null;
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
