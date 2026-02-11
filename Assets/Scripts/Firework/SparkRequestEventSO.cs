// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using HanabiCanvas.Runtime.Events;
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Typed event channel for spark burst requests. Raise with a <see cref="SparkRequest"/>
    /// payload to trigger the spark system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Spark Request Event", menuName = "Hanabi Canvas/Events/Spark Request Event")]
    public class SparkRequestEventSO : GameEventSO<SparkRequest> { }
}
