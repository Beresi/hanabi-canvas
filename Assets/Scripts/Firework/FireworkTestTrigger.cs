// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime.Firework
{
    public class FireworkTestTrigger : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("Events")]
        [Tooltip("Event to raise when the launch key is pressed")]
        [SerializeField] private GameEventSO _onLaunchFirework;

        // ---- Unity Methods ----
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_onLaunchFirework != null)
                {
                    _onLaunchFirework.Raise();
                }
                else
                {
                    Debug.LogWarning(
                        $"[{nameof(FireworkTestTrigger)}] OnLaunchFirework event is not assigned.", this);
                }
            }
        }
    }
}
