// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Payload struct describing a request to spawn firework bursts at a given position
    /// with pattern data and colors from the player's pixel-art drawing.
    /// </summary>
    public struct FireworkRequest
    {
        /// <summary>World-space origin of the burst.</summary>
        public Vector3 Position;

        /// <summary>Pixel entries defining the pattern (positions and colors).</summary>
        public PixelEntry[] Pattern;

        /// <summary>Width of the source grid (for world-space mapping).</summary>
        public int PatternWidth;

        /// <summary>Height of the source grid (for world-space mapping).</summary>
        public int PatternHeight;
    }
}
