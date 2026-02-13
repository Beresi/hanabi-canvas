// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using HanabiCanvas.Runtime.GameFlow;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Shared runtime variable holding the current <see cref="GameState"/>.
    /// Fires <see cref="OnValueChanged"/> when the state transitions.
    /// </summary>
    [CreateAssetMenu(fileName = "New Game State Variable", menuName = "Hanabi Canvas/Variables/Game State Variable")]
    public class GameStateVariableSO : VariableSO<GameState>
    {
    }
}
