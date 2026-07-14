using System;
using UnityEngine;
using Object = UnityEngine.Object;
[Serializable]
public class EffectSpawnVFX : Effect
{
	[Header("VFX")]
	[field: SerializeField] public GameObject Prefab { get; private set; }
	[field: SerializeField] public float Scale { get; private set; } = 1.0f;
	[field: SerializeField] public bool AlignToHitDirection { get; private set; } = true;
	[field: SerializeField] public bool AttachToUser { get; private set; } = true;

	public override bool Execute(EffectPayload effectPayload)
	{
		if (Prefab == null) return false;

		Vector2 spawnPosition = AttachToUser ? effectPayload.User.transform.position : effectPayload.TargetPosition;
		Transform vfxParent = AttachToUser ? effectPayload.User.transform : null;

		// Spawn the VFX and apply scale
		GameObject vfxInstance = Object.Instantiate(Prefab, spawnPosition, Quaternion.identity, vfxParent);
		vfxInstance.transform.localScale *= Scale;
		Object.Destroy(vfxInstance, 1.0f);

		if (AlignToHitDirection && effectPayload.HitDirection != Vector2.zero)
		{
			float angle = Mathf.Atan2(effectPayload.HitDirection.y, effectPayload.HitDirection.x)*Mathf.Rad2Deg;
			vfxInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
		} else if (!AlignToHitDirection && effectPayload.HitDirection != Vector2.zero)
		{
			if (effectPayload.HitDirection.x < 0)
				vfxInstance.transform.rotation = Quaternion.Euler(0, 180f, 0);
			else if (effectPayload.HitDirection.x > 0)
				vfxInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
		}

		return true; // Effect successfully fired
	}

	public override Effect Clone()
	{
		return new EffectSpawnVFX
		{
			Prefab = Prefab,
			AlignToHitDirection = AlignToHitDirection,
			AttachToUser = AttachToUser
		};
	}
}
