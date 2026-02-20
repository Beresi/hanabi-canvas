// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Abstract base for rocket flight path strategies.
    /// Each concrete subclass defines how a rocket travels from spawn to destination.
    /// </summary>
    public abstract class RocketPathSO : ScriptableObject
    {
        // ---- Constants ----
        private const float FINITE_DIFF_EPSILON = 0.001f;

        /// <summary>
        /// Evaluates the world-space position along the path at normalized time t.
        /// </summary>
        /// <param name="spawn">The starting world position of the rocket.</param>
        /// <param name="destination">The target world position of the rocket.</param>
        /// <param name="t">Normalized time along the path (0 = start, 1 = end).</param>
        /// <returns>The interpolated world-space position at time t.</returns>
        public abstract Vector3 Evaluate(Vector3 spawn, Vector3 destination, float t);

        /// <summary>
        /// Returns the total flight duration in seconds for this path.
        /// </summary>
        /// <param name="spawn">The starting world position of the rocket.</param>
        /// <param name="destination">The target world position of the rocket.</param>
        /// <returns>Flight duration in seconds.</returns>
        public abstract float GetFlightDuration(Vector3 spawn, Vector3 destination);

        /// <summary>
        /// Returns the velocity (direction and speed) at normalized time t.
        /// Default uses finite differencing. Override for analytical precision.
        /// </summary>
        /// <param name="spawn">The starting world position of the rocket.</param>
        /// <param name="destination">The target world position of the rocket.</param>
        /// <param name="t">Normalized time along the path (0 = start, 1 = end).</param>
        /// <returns>The velocity vector at time t.</returns>
        public virtual Vector3 EvaluateVelocity(Vector3 spawn, Vector3 destination, float t)
        {
            float t0 = Mathf.Max(0f, t - FINITE_DIFF_EPSILON);
            float t1 = Mathf.Min(1f, t + FINITE_DIFF_EPSILON);
            Vector3 p0 = Evaluate(spawn, destination, t0);
            Vector3 p1 = Evaluate(spawn, destination, t1);
            float duration = GetFlightDuration(spawn, destination);
            if (duration <= 0f)
            {
                return Vector3.zero;
            }
            return (p1 - p0) / ((t1 - t0) * duration);
        }
    }
}
