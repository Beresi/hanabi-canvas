// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    public abstract class ListSO<T> : ScriptableObject
    {
        // ---- Protected Fields ----
        protected readonly List<T> items = new List<T>();

        // ---- Events ----
        public event System.Action<T> OnItemAdded;
        public event System.Action<T> OnItemRemoved;
        public event System.Action OnCleared;

        // ---- Properties ----
        public int Count => items.Count;

        // ---- Unity Methods ----
        private void OnEnable()
        {
            items.Clear();
        }

        // ---- Public Methods ----
        public void Add(T item)
        {
            items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public bool Remove(T item)
        {
            bool isRemoved = items.Remove(item);
            if (isRemoved)
            {
                OnItemRemoved?.Invoke(item);
            }
            return isRemoved;
        }

        public T GetAt(int index)
        {
            return items[index];
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void Clear()
        {
            items.Clear();
            OnCleared?.Invoke();
        }

        public void ProcessAll(System.Action<T> action)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                action(items[i]);
            }
        }
    }
}
