// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using HanabiCanvas.Runtime;
using UnityEngine;

namespace HanabiCanvas.Runtime.Events
{
    /// <summary>
    /// Typed event channel for artwork-related events. Raise with an <see cref="ArtworkData"/>
    /// payload to notify listeners of artwork creation, update, or deletion.
    /// </summary>
    [CreateAssetMenu(fileName = "New Artwork Event", menuName = "Hanabi Canvas/Events/Artwork Event")]
    public class ArtworkEventSO : GameEventSO<ArtworkData>
    {
    }
}
