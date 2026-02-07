// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HanabiCanvas.Runtime.Events
{
    public abstract class GameEventSO<T> : ScriptableObject
    {
        // ---- Private Fields ----
        private readonly List<System.Action<T>> _listeners = new List<System.Action<T>>();

        // ---- Public Methods ----
        public void Raise(T value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke(value);
            }
        }

        public void Register(System.Action<T> listener)
        {
            _listeners.Add(listener);
        }

        public void Unregister(System.Action<T> listener)
        {
            _listeners.Remove(listener);
        }
    }
}
