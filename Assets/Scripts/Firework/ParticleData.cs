// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    public struct ParticleData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Color Color;
        public float Size;
        public float Life;
        public Vector3 FormationTarget;
        public bool IsPattern;
    }
}
