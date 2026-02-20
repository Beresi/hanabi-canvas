// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

namespace HanabiCanvas.Runtime.GameFlow
{
    /// <summary>
    /// High-level application state controlling which screen/mode is active.
    /// </summary>
    public enum AppState
    {
        /// <summary>Main menu screen.</summary>
        Menu,

        /// <summary>Active game session (drawing/launching/watching cycle).</summary>
        Playing,

        /// <summary>Replaying saved artworks as fireworks.</summary>
        Slideshow,

        /// <summary>Settings/export/import screen.</summary>
        Settings
    }
}
