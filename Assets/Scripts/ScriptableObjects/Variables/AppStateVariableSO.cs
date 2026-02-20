// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using HanabiCanvas.Runtime.GameFlow;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Shared runtime variable holding the current <see cref="AppState"/>.
    /// Fires <see cref="VariableSO{T}.OnValueChanged"/> when the app state transitions.
    /// </summary>
    [CreateAssetMenu(fileName = "New App State Variable", menuName = "Hanabi Canvas/Variables/App State Variable")]
    public class AppStateVariableSO : VariableSO<AppState>
    {
    }
}
