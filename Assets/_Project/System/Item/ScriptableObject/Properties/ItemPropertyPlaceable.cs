using System;
using UnityEngine;
[Serializable]
public class ItemPropertyPlaceable : ItemPropertyBase
{
	[field: SerializeField] public GameObject PlaceablePrefab { get; private set; }

	[field: Header("Grid Sizing")]
	[field: SerializeField] public bool SnapToGrid { get; private set; } = true;
	[field: Tooltip("If true, automatically calculates size based on the sprite's Pixels Per Unit.")]
	[field: SerializeField] public bool AutoCalculateSize { get; private set; } = true;

	[field: Tooltip("Used only if Auto Calculate is FALSE. (X = Width, Y = Height)")]
	[field: SerializeField] public Vector2Int ManualGridSize { get; private set; } = new Vector2Int(1, 1);

	/// <summary>
	/// Returns the automated grid size or the manual size if automation is turned off.
	/// </summary>
	public Vector2Int GetGridSize(Sprite itemSprite)
	{
		if (AutoCalculateSize && itemSprite != null)
		{
			// Use CeilToInt so a 1.2 unit sprite safely rounds up to 2 grid cells
			int autoWidth = Mathf.Max(1, Mathf.CeilToInt(itemSprite.bounds.size.x));
			int autoHeight = Mathf.Max(1, Mathf.CeilToInt(itemSprite.bounds.size.y));

			return new Vector2Int(autoWidth, autoHeight);
		}

		// Enforce a strict minimum of 1x1 if the user forgets and leaves Manual Size at 0x0
		return new Vector2Int(Mathf.Max(1, ManualGridSize.x), Mathf.Max(1, ManualGridSize.y));
	}
}
