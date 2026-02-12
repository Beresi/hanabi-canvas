// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using HanabiCanvas.Runtime.Events;
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Typed event channel for firework burst requests. Raise with a <see cref="FireworkRequest"/>
    /// payload to trigger the firework system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Firework Request Event", menuName = "Hanabi Canvas/Events/Firework Request Event")]
    public class FireworkRequestEventSO : GameEventSO<FireworkRequest> { }
}
