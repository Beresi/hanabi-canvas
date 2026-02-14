// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime
{
    public class SavePatternButton : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [SerializeField] private GameEventSO _onSavePattern;

        // ---- Unity Methods ----
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() => _onSavePattern.Raise());
        }
    }
}
