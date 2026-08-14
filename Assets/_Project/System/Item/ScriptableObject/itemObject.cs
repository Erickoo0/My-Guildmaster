using DG.Tweening;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class ItemObject : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private ItemDataSo startingItemDataSo; // Used ONLY when placing items manually via the editor

	[Header("Spawn Animation")]
	[SerializeField] private float bounceDuration = 0.5f;
	[SerializeField] private float bounceHeight = 0.65f;
	[SerializeField] private int bounceCount = 3;
	[SerializeField] private float flyDuration = 0.4f;
	[SerializeField] private float jumpPower = 1.2f;

	[Header("Pickup Animation")]
	[SerializeField] private float pullSpeed = 0.2f;
	[SerializeField] private Ease pullEase = Ease.InBack; // Snappy vacuum feel
	private Sequence _activeSequence;                     // Spawn Animation sequence
	private bool _canBePickedUp = false;

	private ItemInstance _itemInstance;
	private SpriteRenderer _spriteRenderer;

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();

		// If we assigned starting item in the inspector, then initialize it
		if (startingItemDataSo != null) SetItemObject(new ItemInstance(startingItemDataSo));

	}

	private void Update()
	{
		if (_itemInstance?.DataSo == null || _itemInstance.DataSo.ItemIcon == null) return;

		_spriteRenderer.sprite = GlobalHelper.GetAnimatedSprite(_itemInstance.DataSo);
	}

	private void OnDestroy() => _activeSequence?.Kill(); // Kill the animation sequence on destroy

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!_canBePickedUp) return;
		TryPickup(collision);
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (!_canBePickedUp) return;
		TryPickup(collision);
	}

	public void SetItemObject(ItemInstance newItemInstance, Vector3? dropTarget = null, bool animate = true)
	{
		_itemInstance = newItemInstance;
		gameObject.name = _itemInstance.DataSo.ItemName;

		if (!animate) return; // Skip the animation
		PlaySpawnAnimation(dropTarget);
	}

	private void PlaySpawnAnimation(Vector3? dropTarget)
	{
		_canBePickedUp = false;

		// 1. Generate Noise / Jitter for this specific item
		float delay = Random.Range(0f, 0.15f);                            // The "Popcorn" effect
		float actualJumpPower = jumpPower*Random.Range(0.8f, 1.2f);       // +/- 20%
		float actualFlyDuration = flyDuration*Random.Range(0.85f, 1.15f); // +/- 15%
		float actualBounceHeight = bounceHeight*Random.Range(0.8f, 1.2f); // +/- 20%

		// 2. Hide the item initially so it doesn't just sit there during the delay
		Vector3 originalScale = transform.localScale;
		transform.localScale = Vector3.zero;

		// 3. Create a sequence jump -> bounce
		_activeSequence = DOTween.Sequence();

		// 4. Add the random start delay
		_activeSequence.AppendInterval(delay);

		// 5. Pop the scale back to normal quickly right as it starts moving
		_activeSequence.Append(transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack));

		// 6.Final position is either the provided target, or its current position if spawned directly
		Vector3 finalPosition = dropTarget ?? transform.position;

		// 7. Fly to dropTarget if it has been provided by a Source
		if (dropTarget.HasValue)
		{
			// Join the jump animation with the scale pop sequence
			_activeSequence.Join(transform.DOJump(finalPosition, actualJumpPower, 1, actualFlyDuration).SetEase(Ease.Linear));

			// 8. Enable Pickup AFTER fly animation
			_activeSequence.AppendCallback(() => _canBePickedUp = true);
		}

		// 9. Bounce Logic
		for (int i = 0; i < bounceCount; i++)
		{
			// Decrease the height and Duration each bounce
			float currentBounceHeight = actualBounceHeight*(1f - (i*0.4f));
			float currentDuration = bounceDuration*(1f - (i*0.2f));

			// Adds DOMoveY (Up) to the spawnSequence
			_activeSequence.Append(transform.DOMoveY(finalPosition.y + currentBounceHeight, currentDuration/2)
				.SetEase(Ease.OutQuad));
			// Adds DOMoveY (Down) to the spawnSequence
			_activeSequence.Append(transform.DOMoveY(finalPosition.y, currentDuration/2)
				.SetEase(Ease.InQuad));
		}

		// 4. Clean up the sequence reference 
		_activeSequence.OnComplete(() =>
		{
			_activeSequence = null;
			_canBePickedUp = true; // Enable pickup after the animation is finished
		});
	}

	private async void TryPickup(Collider2D collision)
	{
		try
		{
			if (!collision.CompareTag("Player") || _itemInstance == null) return;

			// 1. Lock out further triggers
			_canBePickedUp = false; // Set to false since item is now picked up

			// 2. Kill the current animation sequence 
			_activeSequence.Kill();

			// 3. Shrink the item as it gets sucked up
			transform.DOScale(Vector3.zero, pullSpeed).SetEase(Ease.InBack);

			// 4. Play the vacuum animation towards the player, pause the code here untill animation finishes.
			await transform.DOMove(collision.transform.position, pullSpeed).SetEase(pullEase).AsyncWaitForCompletion();

			// 5. Execute pickup
			if (_itemInstance.DataSo.ItemID == "Item_Resource_Gold")
			{
				GoldManager.Instance.AddGold(_itemInstance.stackSize);
				Destroy(gameObject);
			} else
			{
				bool wasPickedUp = InventoryManager.Instance.AddItems(_itemInstance);
				if (wasPickedUp)
				{
					Destroy(gameObject);
				}
			}
		} catch (Exception e)
		{
			Debug.LogError($"Failed to pick up item: {e.Message}");
			throw;
		}
	}
}
