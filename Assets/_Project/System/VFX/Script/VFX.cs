using System;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(IObjectPool<VFX>))]
public class VFX : MonoBehaviour
{
    private IObjectPool<VFX> _pool;
    
    /// <summary>
    /// Links the particles to the global pool
    /// </summary>
    public void SetPool(IObjectPool<VFX> pool) => _pool = pool;

    // Automatically called when a Particle System finishes
    // Must set "Stop Action" to "Callback" in particle system
    private void OnParticleSystemStopped()
    {
        if (_pool != null)
            _pool.Release(this);
    }
}
