// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Int Variable", menuName = "Hanabi Canvas/Variables/Int Variable")]
    public class IntVariableSO : ScriptableObject
    {
        // ---- Serialized Fields ----
        [Header("Initial Value")]
        [Tooltip("Value reset to this on play mode entry")]
        [SerializeField] private int _initialValue;

        // ---- Private Fields ----
        private int _runtimeValue;

        // ---- Events ----
        public event System.Action<int> OnValueChanged;

        // ---- Properties ----
        public int Value
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
