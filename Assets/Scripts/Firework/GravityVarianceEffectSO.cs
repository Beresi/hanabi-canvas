// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Per-particle gravity variance effect. Applies a unique downward displacement
    /// to each particle based on RandomSeed, creating visible spread where some
    /// particles fall faster than others. Modifies Position directly for immediate
    /// visual impact (velocity-based gravity is handled by the behaviour).
    /// Best suited for Burst and Ring behaviours.
    /// </summary>
    [CreateAssetMenu(fileName = "New Gravity Variance Effect", menuName = "Hanabi Canvas/Firework Effects/Gravity Variance")]
    public class GravityVarianceEffectSO : FireworkEffectSO
    {
        // ---- Serialized Fields ----

        [Tooltip("Minimum gravity multiplier (applied to particles with RandomSeed=0)")]
        [Min(0f)]
        [SerializeField] private float _minGravityMultiplier = 0.5f;

        [Tooltip("Maximum gravity multiplier (applied to particles with RandomSeed=1)")]
        [Min(0f)]
        [SerializeField] private float _maxGravityMultiplier = 2.0f;

        [Tooltip("Base gravity strength in units/sec squared")]
        [SerializeField] private float _baseGravity = 3.0f;

        [Tooltip("Seconds after launch before gravity begins taking effect")]
        [Min(0f)]
        [SerializeField] private float _startDelay = 0.5f;

        // ---- Public Properties ----

        /// <summary>Minimum gravity multiplier (applied to particles with RandomSeed=0).</summary>
        public float MinGravityMultiplier => _minGravityMultiplier;

        /// <summary>Maximum gravity multiplier (applied to particles with RandomSeed=1).</summary>
        public float MaxGravityMultiplier => _maxGravityMultiplier;

        /// <summary>Base gravity strength in units/sec squared.</summary>
        public float BaseGravity => _baseGravity;

        /// <summary>Seconds after launch before gravity begins taking effect.</summary>
        public float StartDelay => _startDelay;

        // ---- Unity Callbacks ----

        private void OnValidate()
        {
            _minGravityMultiplier = Mathf.Max(_minGravityMultiplier, 0f);
            _maxGravityMultiplier = Mathf.Max(_maxGravityMultiplier, 0f);
            _startDelay = Mathf.Max(_startDelay, 0f);
        }

        // ---- FireworkEffectSO Implementation ----

        /// <inheritdoc/>
        public override void InitializeEffect(FireworkParticle[] particles, int count)
        {
            // No-op: RandomSeed already set by behaviour
        }

        /// <inheritdoc/>
        public override void UpdateEffect(FireworkParticle[] particles, int count,
            float deltaTime, float elapsedTime)
        {
            for (int i = 0; i < count; i++)
            {
                if (particles[i].Life <= 0f)
                {
                    continue;
                }

                float gravityMultiplier = Mathf.Lerp(
                    _minGravityMultiplier,
                    _maxGravityMultiplier,
                    particles[i].RandomSeed);

                // Gravity kicks in only after the start delay.
                float gravityTime = elapsedTime - _startDelay;
                if (gravityTime <= 0f)
                {
                    particles[i].GravityDisplacementY = 0f;
                    continue;
                }

                // Cumulative displacement: s = 1/2 * a * t^2 (kinematic equation).
                // The manager undoes the previous frame's displacement before the
                // behaviour runs, so this absolute offset is applied on a clean position
                // each frame — works for both Burst (velocity-accumulated) and Pattern
                // (lerp-set) behaviours.
                float totalDisplacement = 0.5f * _baseGravity * gravityMultiplier
                    * gravityTime * gravityTime;

                particles[i].Position.y -= totalDisplacement;
                particles[i].GravityDisplacementY = totalDisplacement;
            }
        }
    }
}
