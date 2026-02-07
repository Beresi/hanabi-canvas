// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Float Variable", menuName = "Hanabi Canvas/Variables/Float Variable")]
    public class FloatVariableSO : ScriptableObject
    {
        // ---- Serialized Fields ----
        [Header("Initial Value")]
        [Tooltip("Value reset to this on play mode entry")]
        [SerializeField] private float _initialValue;

        // ---- Private Fields ----
        private float _runtimeValue;

        // ---- Events ----
        public event System.Action<float> OnValueChanged;

        // ---- Properties ----
        public float Value
        {
            get => _runtimeValue;
            set
            {
                if (Mathf.Approximately(_runtimeValue, value))
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
