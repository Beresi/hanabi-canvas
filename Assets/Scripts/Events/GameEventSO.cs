// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HanabiCanvas.Runtime.Events
{
    [CreateAssetMenu(fileName = "New Game Event", menuName = "Hanabi Canvas/Events/Game Event")]
    public class GameEventSO : ScriptableObject
    {
        // ---- Private Fields ----
        private readonly List<System.Action> _listeners = new List<System.Action>();

        // ---- Public Methods ----
        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke();
            }
        }

        public void Register(System.Action listener)
        {
            _listeners.Add(listener);
        }

        public void Unregister(System.Action listener)
        {
            _listeners.Remove(listener);
        }
    }
}
