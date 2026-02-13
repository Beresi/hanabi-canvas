using System;
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    public abstract class VariableSO<T> : ScriptableObject
    {
        [SerializeField] protected T initialValue;
        [SerializeField] protected T runtimeValue;

        public event Action<T> OnValueChanged;

        public T Value
        {
            get => runtimeValue;
            set
            {
                if (value.Equals(runtimeValue)) return;

                runtimeValue = value;
                OnValueChanged?.Invoke(runtimeValue);
            }
        }

        private void OnEnable()
        {
            ResetToInitial();
        }

        public void ResetToInitial()
        {
            runtimeValue = initialValue;
        }
    }
}
