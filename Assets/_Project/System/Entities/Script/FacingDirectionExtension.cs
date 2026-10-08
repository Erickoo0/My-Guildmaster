using UnityEngine;
public interface IFaceable
{
	public void FaceDirection(FacingDirection faceDirection);
}

public enum FacingDirection
{
	Up,
	Down,
	Left,
	Right,
	None // Useful for "keep current orientation"
}

public static class FacingDirectionExtension
{
	public static Vector2 ToVector2(this FacingDirection facingDirection)
	{
		return facingDirection switch
		{
			FacingDirection.Up => Vector2.up,
			FacingDirection.Down => Vector2.down,
			FacingDirection.Left => Vector2.left,
			FacingDirection.Right => Vector2.right,
			_ => Vector2.zero,
		};
	}
}
