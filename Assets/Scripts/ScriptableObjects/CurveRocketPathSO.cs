// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Concrete rocket path: cubic Bezier curve.
    /// The rocket follows a smooth cubic Bezier curve from spawn to destination,
    /// with configurable control point offsets for shaping the flight arc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Curve Rocket Path", menuName = "Hanabi Canvas/Rocket Paths/Curve")]
    public class CurveRocketPathSO : RocketPathSO
    {
        // ---- Constants ----
        private const float MIN_SPEED = 0.1f;
        private const int ARC_LENGTH_SEGMENTS = 10;

        // ---- Serialized Fields ----
        [Header("Speed")]
        [Tooltip("Travel speed in world units per second")]
        [Min(0.1f)]
        [SerializeField] private float _speed = 10f;

        [Tooltip("Progress curve over normalized flight time (0=start, 1=end). Controls easing along the Bezier.")]
        [SerializeField] private AnimationCurve _progressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Control Points")]
        [Tooltip("Offset from spawn position for the first Bezier control point")]
        [SerializeField] private Vector3 _controlPoint1Offset = new Vector3(0f, 5f, 0f);

        [Tooltip("Offset from destination position for the second Bezier control point")]
        [SerializeField] private Vector3 _controlPoint2Offset = new Vector3(0f, 3f, 0f);

        // ---- Properties ----

        /// <summary>Travel speed in world units per second.</summary>
        public float Speed => _speed;

        /// <summary>Progress curve over normalized flight time.</summary>
        public AnimationCurve ProgressCurve => _progressCurve;

        /// <summary>Offset from spawn position for the first Bezier control point.</summary>
        public Vector3 ControlPoint1Offset => _controlPoint1Offset;

        /// <summary>Offset from destination position for the second Bezier control point.</summary>
        public Vector3 ControlPoint2Offset => _controlPoint2Offset;

        // ---- Public Methods ----

        /// <inheritdoc/>
        public override Vector3 Evaluate(Vector3 spawn, Vector3 destination, float t)
        {
            float eased = _progressCurve.Evaluate(t);
            Vector3 p0 = spawn;
            Vector3 p1 = spawn + _controlPoint1Offset;
            Vector3 p2 = destination + _controlPoint2Offset;
            Vector3 p3 = destination;
            return CubicBezier(p0, p1, p2, p3, eased);
        }

        /// <inheritdoc/>
        public override float GetFlightDuration(Vector3 spawn, Vector3 destination)
        {
            float arcLength = EstimateArcLength(spawn, destination, ARC_LENGTH_SEGMENTS);
            return arcLength / _speed;
        }

        // ---- Unity Methods ----
        private void OnValidate()
        {
            _speed = Mathf.Max(MIN_SPEED, _speed);
        }

        // ---- Private Methods ----

        /// <summary>
        /// Evaluates a point on a cubic Bezier curve.
        /// </summary>
        private static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float uu = u * u;
            float uuu = uu * u;
            float tt = t * t;
            float ttt = tt * t;

            return uuu * p0
                + 3f * uu * t * p1
                + 3f * u * tt * p2
                + ttt * p3;
        }

        /// <summary>
        /// Estimates the arc length of the Bezier curve by sampling at discrete segments.
        /// </summary>
        private float EstimateArcLength(Vector3 spawn, Vector3 destination, int segments)
        {
            Vector3 p0 = spawn;
            Vector3 p1 = spawn + _controlPoint1Offset;
            Vector3 p2 = destination + _controlPoint2Offset;
            Vector3 p3 = destination;

            float totalLength = 0f;
            Vector3 previous = p0;

            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 current = CubicBezier(p0, p1, p2, p3, t);
                totalLength += Vector3.Distance(previous, current);
                previous = current;
            }

            return totalLength;
        }
    }
}
