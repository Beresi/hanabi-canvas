// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Minimal mutable value type holding the runtime state of a single firework particle.
    /// Public fields for direct mutation without copy overhead (mutable value type).
    /// </summary>
    public struct FireworkParticle
    {
        /// <summary>World position of the particle.</summary>
        public Vector3 Position;

        /// <summary>Current velocity in units per second.</summary>
        public Vector3 Velocity;

        /// <summary>Base color. RGB stays constant; alpha is faded by the updater.</summary>
        public Color Color;

        /// <summary>Current rendered size in world units.</summary>
        public float Size;

        /// <summary>Remaining life in seconds. Particle is dead when this reaches zero.</summary>
        public float Life;

        /// <summary>Initial life value, used to compute normalized progress (1 - Life / MaxLife).</summary>
        public float MaxLife;

        /// <summary>Starting position for lerp-based behaviours (e.g., pattern formation).</summary>
        public Vector3 StartPosition;

        /// <summary>Target position for lerp-based behaviours (e.g., pattern formation).</summary>
        public Vector3 TargetPosition;
    }
}
