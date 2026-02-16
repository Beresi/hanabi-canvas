// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Abstract base class for composable firework effects.
    /// Effects are attached to <see cref="FireworkBehaviourSO"/> instances and run
    /// after the behaviour's own particle update each frame.
    /// </summary>
    public abstract class FireworkEffectSO : ScriptableObject
    {
        /// <summary>
        /// Called once after behaviour.InitializeParticles().
        /// RandomSeed and BaseColor are already set on each particle.
        /// </summary>
        /// <param name="particles">The particle array to initialize effects on.</param>
        /// <param name="count">Number of active particles in the array.</param>
        public abstract void InitializeEffect(FireworkParticle[] particles, int count);

        /// <summary>
        /// Called each frame after behaviour.UpdateParticles(). Modifies particles in-place.
        /// </summary>
        /// <param name="particles">The particle array to apply effects to.</param>
        /// <param name="count">Number of active particles in the array.</param>
        /// <param name="deltaTime">Frame delta time in seconds.</param>
        /// <param name="elapsedTime">Total seconds since this firework started.</param>
        public abstract void UpdateEffect(FireworkParticle[] particles, int count, float deltaTime, float elapsedTime);
    }
}
