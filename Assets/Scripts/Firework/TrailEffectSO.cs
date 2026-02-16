// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Velocity-stretch trail effect. This is a render-time effect — InitializeEffect
    /// and UpdateEffect are intentionally no-ops. The actual trail rendering happens
    /// in <see cref="FireworkManager"/>.RebuildMesh() which reads this SO's public properties.
    /// </summary>
    [CreateAssetMenu(fileName = "New Trail Effect", menuName = "Hanabi Canvas/Firework Effects/Trail")]
    public class TrailEffectSO : FireworkEffectSO
    {
        // ---- Constants ----
        private const float MIN_STRETCH_MULTIPLIER = 0.01f;
        private const float MIN_MAX_STRETCH_LENGTH = 0.1f;

        // ---- Serialized Fields ----

        [Tooltip("Multiplier for velocity-based quad elongation")]
        [Min(MIN_STRETCH_MULTIPLIER)]
        [SerializeField] private float _stretchMultiplier = 0.15f;

        [Tooltip("Maximum stretch in world units")]
        [Min(MIN_MAX_STRETCH_LENGTH)]
        [SerializeField] private float _maxStretchLength = 1.0f;

        [Tooltip("Below this velocity magnitude, use standard billboard")]
        [Min(0f)]
        [SerializeField] private float _minVelocityThreshold = 0.5f;

        // ---- Public Properties ----

        /// <summary>Multiplier for velocity-based quad elongation.</summary>
        public float StretchMultiplier => _stretchMultiplier;

        /// <summary>Maximum stretch in world units.</summary>
        public float MaxStretchLength => _maxStretchLength;

        /// <summary>Below this velocity magnitude, use standard billboard.</summary>
        public float MinVelocityThreshold => _minVelocityThreshold;

        // ---- Unity Callbacks ----

        private void OnValidate()
        {
            _stretchMultiplier = Mathf.Max(_stretchMultiplier, MIN_STRETCH_MULTIPLIER);
            _maxStretchLength = Mathf.Max(_maxStretchLength, MIN_MAX_STRETCH_LENGTH);
            _minVelocityThreshold = Mathf.Max(_minVelocityThreshold, 0f);
        }

        // ---- FireworkEffectSO Implementation ----

        /// <inheritdoc/>
        public override void InitializeEffect(FireworkParticle[] particles, int count)
        {
            // No-op: trail is a render-time effect
        }

        /// <inheritdoc/>
        public override void UpdateEffect(FireworkParticle[] particles, int count, float deltaTime, float elapsedTime)
        {
            // No-op: trail stretching is handled in FireworkManager.RebuildMesh()
        }
    }
}
