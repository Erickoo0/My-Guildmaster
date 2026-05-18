using UnityEngine;

[CreateAssetMenu(fileName = "Simple Animation Data", menuName = "Environment/Animation Data")]
public class SimpleAnimationData : ScriptableObject
{
   public Sprite[] animationFrames;
   public float fps = 4f;
}
