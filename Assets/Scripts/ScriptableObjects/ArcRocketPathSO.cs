// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Concrete rocket path: linear base with vertical arc offset.
    /// The rocket follows a straight line from spawn to destination with an
    /// additional vertical offset controlled by an arc curve, creating a
    /// parabolic flight path.
    /// </summary>
    [CreateAssetMenu(fileName = "New Arc Rocket Path", menuName = "Hanabi Canvas/Rocket Paths/Arc")]
    public class ArcRocketPathSO : RocketPathSO
    {
        // ---- Constants ----
        private const float MIN_SPEED = 0.1f;
        private const float MIN_ARC_HEIGHT = 0f;
        private const float ARC_LENGTH_MULTIPLIER = 1.15f;

        // ---- Serialized Fields ----
        [Header("Speed")]
        [Tooltip("Travel speed in world units per second")]
        [Min(0.1f)]
        [SerializeField] private float _speed = 12f;

        [Tooltip("Progress curve over normalized flight time (0=start, 1=end). Controls easing along the base line.")]
        [SerializeField] private AnimationCurve _progressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Arc")]
        [Tooltip("Maximum vertical offset at the peak of the arc in world units")]
        [Min(0f)]
        [SerializeField] private float _arcHeight = 3f;

        [Tooltip("Arc shape curve over normalized flight time. Peak at 0.5 creates a symmetric parabola.")]
        [SerializeField] private AnimationCurve _arcCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 1f),
            new Keyframe(1f, 0f)
        );

        // ---- Properties ----

        /// <summary>Travel speed in world units per second.</summary>
        public float Speed => _speed;

        /// <summary>Maximum vertical offset at the peak of the arc.</summary>
        public float ArcHeight => _arcHeight;

        /// <summary>Progress curve over normalized flight time.</summary>
        public AnimationCurve ProgressCurve => _progressCurve;

        /// <summary>Arc shape curve over normalized flight time.</summary>
        public AnimationCurve ArcCurve => _arcCurve;

        // ---- Public Methods ----

        /// <inheritdoc/>
        public override Vector3 Evaluate(Vector3 spawn, Vector3 destination, float t)
        {
            float eased = _progressCurve.Evaluate(t);
            Vector3 linear = Vector3.Lerp(spawn, destination, eased);
            linear.y += _arcCurve.Evaluate(eased) * _arcHeight;
            return linear;
        }

        /// <inheritdoc/>
        public override float GetFlightDuration(Vector3 spawn, Vector3 destination)
        {
            float distance = Vector3.Distance(spawn, destination);
            return distance * ARC_LENGTH_MULTIPLIER / _speed;
        }

        // ---- Unity Methods ----
        private void OnValidate()
        {
            _speed = Mathf.Max(MIN_SPEED, _speed);
            _arcHeight = Mathf.Max(MIN_ARC_HEIGHT, _arcHeight);
        }
    }
}
