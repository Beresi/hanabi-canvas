// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Bool Variable", menuName = "Hanabi Canvas/Variables/Bool Variable")]
    public class BoolVariableSO : ScriptableObject
    {
        // ---- Serialized Fields ----
        [Header("Initial Value")]
        [Tooltip("Value reset to this on play mode entry")]
        [SerializeField] private bool _initialValue;

        // ---- Private Fields ----
        private bool _runtimeValue;

        // ---- Events ----
        public event System.Action<bool> OnValueChanged;

        // ---- Properties ----
        public bool Value
        {
            get => _runtimeValue;
            set
            {
                if (_runtimeValue == value)
                {
                    return;
                }
                _runtimeValue = value;
                OnValueChanged?.Invoke(_runtimeValue);
            }
        }

        // ---- Unity Methods ----
        private void OnEnable()
        {
            ResetToInitial();
        }

        // ---- Public Methods ----
        public void ResetToInitial()
        {
            _runtimeValue = _initialValue;
        }
    }
}
