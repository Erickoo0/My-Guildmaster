using UnityEngine;

[System.Serializable]
public abstract class BaseKiteState : BaseActionState
{
	[SerializeField] protected float kittingThreshold = 3f;
	[SerializeField] protected float preferredDistance = 5f;
	[SerializeField] protected float recheckPathInterval = 0.4f;
	[SerializeField] protected float kiteDistance = 5f;

	protected float _recheckTimer;
	
	public bool CheckShouldKite(float currentDistance) => currentDistance < kittingThreshold;
}
