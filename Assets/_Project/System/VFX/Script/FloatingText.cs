using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
/// <summary>
/// Displays a float for a set duration, slowly fading and moving up
/// </summary>
public class FloatingText : MonoBehaviour
{
	[SerializeField] private TMP_Text textMesh; // Reference to the Text game object
	[SerializeField] private float duration = 3f;

	private IObjectPool<FloatingText> _pool;
	private string text; // The text to display

	// The Manager calls this when grabbing the text from the queue
	public void Initialize(int amount, Vector3 spawnPosition, Color color, IObjectPool<FloatingText> pool)
	{
		_pool = pool;

		// 1. Check if Amount is positive or negative
		if (amount >= 0)
			text = "+" + amount;
		else
			text = amount.ToString();

		// 2. Apply color
		textMesh.color = color;

		// 3. Reset the State
		transform.position = spawnPosition;
		textMesh.text = text;
		textMesh.alpha = 1f;

		Animate();
	}

	private void Animate()
	{
		// Float up by 1.5 units over 1 second using Outback ease for a little "pop"
		transform.DOMoveY(transform.position.y + 1f, duration).SetEase(Ease.OutBack);

		// Fade alpha to 0 over 1 second
		// OnComplete is DOTween's way of executing code when the animation is finished
		textMesh.DOFade(0f, duration).OnComplete(() =>
		{
			if (_pool != null)
				_pool.Release(this);
		});
	}
}
