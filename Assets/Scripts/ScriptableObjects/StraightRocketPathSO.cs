// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Concrete rocket path: straight line with easing curve.
    /// The rocket travels directly from spawn to destination, with speed
    /// modulated by a configurable animation curve.
    /// </summary>
    [CreateAssetMenu(fileName = "New Straight Rocket Path", menuName = "Hanabi Canvas/Rocket Paths/Straight")]
    public class StraightRocketPathSO : RocketPathSO
    {
        // ---- Constants ----
        private const float MIN_SPEED = 0.1f;

        // ---- Serialized Fields ----
        [Header("Speed")]
        [Tooltip("Travel speed in world units per second")]
        [Min(0.1f)]
        [SerializeField] private float _speed = 15f;

        [Tooltip("Speed curve over normalized flight time (0=start, 1=end). Controls easing.")]
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // ---- Properties ----

        /// <summary>Travel speed in world units per second.</summary>
        public float Speed => _speed;

        /// <summary>Speed curve over normalized flight time.</summary>
        public AnimationCurve SpeedCurve => _speedCurve;

        // ---- Public Methods ----

        /// <inheritdoc/>
        public override Vector3 Evaluate(Vector3 spawn, Vector3 destination, float t)
        {
            float eased = _speedCurve.Evaluate(t);
            return Vector3.Lerp(spawn, destination, eased);
        }

        /// <inheritdoc/>
        public override float GetFlightDuration(Vector3 spawn, Vector3 destination)
        {
            float distance = Vector3.Distance(spawn, destination);
            return distance / _speed;
        }

        // ---- Unity Methods ----
        private void OnValidate()
        {
            _speed = Mathf.Max(MIN_SPEED, _speed);
        }
    }
}
