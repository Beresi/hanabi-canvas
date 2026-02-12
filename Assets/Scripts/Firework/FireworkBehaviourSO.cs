// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Abstract base for all firework behaviour ScriptableObjects.
    /// Each concrete subclass defines how particles are counted, initialized,
    /// updated, and when they are considered complete.
    /// </summary>
    public abstract class FireworkBehaviourSO : ScriptableObject
    {
        /// <summary>
        /// Returns how many particles this behaviour needs for the given request.
        /// </summary>
        /// <param name="request">The firework request describing position and color.</param>
        /// <returns>Number of particles to allocate.</returns>
        public abstract int GetParticleCount(FireworkRequest request);

        /// <summary>
        /// Initialize the particle array for this behaviour.
        /// Called once when a firework begins playing.
        /// </summary>
        /// <param name="particles">The particle array to initialize in-place.</param>
        /// <param name="count">Number of particles to initialize.</param>
        /// <param name="request">The firework request describing position and color.</param>
        public abstract void InitializeParticles(FireworkParticle[] particles, int count, FireworkRequest request);

        /// <summary>
        /// Advance all particles by one time step.
        /// Called each frame while the firework is active.
        /// </summary>
        /// <param name="particles">The particle array to update in-place.</param>
        /// <param name="count">Number of active particles in the array.</param>
        /// <param name="deltaTime">Frame delta time in seconds.</param>
        public abstract void UpdateParticles(FireworkParticle[] particles, int count, float deltaTime);

        /// <summary>
        /// Returns true when all particles are dead/complete and the firework can finish.
        /// </summary>
        /// <param name="particles">The particle array to check.</param>
        /// <param name="count">Number of particles in the array.</param>
        /// <returns>True if the behaviour considers the firework complete.</returns>
        public abstract bool IsComplete(FireworkParticle[] particles, int count);
    }
}
