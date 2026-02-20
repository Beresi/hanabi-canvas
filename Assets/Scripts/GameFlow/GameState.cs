// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

namespace HanabiCanvas.Runtime.GameFlow
{
    /// <summary>
    /// Represents the current state of a firework session lifecycle.
    /// </summary>
    public enum GameState
    {
        /// <summary>Player is drawing on the pixel canvas.</summary>
        Drawing,

        /// <summary>System is constructing and dispatching the launch request.</summary>
        Launching,

        /// <summary>Rocket is ascending toward its destination before exploding.</summary>
        Ascending,

        /// <summary>Firework has exploded; player is watching the display.</summary>
        Watching,

        /// <summary>Display is finished; system is resetting for the next round.</summary>
        Resetting
    }
}
