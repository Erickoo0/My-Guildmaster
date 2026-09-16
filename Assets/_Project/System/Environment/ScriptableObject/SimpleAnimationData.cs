using UnityEngine;
/// <summary>
/// SO that holds animaiton frames and frame speed
/// </summary>
[CreateAssetMenu(fileName = "Simple Animation Data", menuName = "Environment/Animation Data")]
public class SimpleAnimationData : ScriptableObject
{
	public Sprite[] animationFrames;
	public float fps = 4f;
}
