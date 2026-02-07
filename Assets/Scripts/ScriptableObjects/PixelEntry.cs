// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [System.Serializable]
    public struct PixelEntry
    {
        // ---- Serialized Fields ----
        [SerializeField] private byte _x;
        [SerializeField] private byte _y;
        [SerializeField] private Color32 _color;

        // ---- Properties ----
        public byte X => _x;
        public byte Y => _y;
        public Color32 Color => _color;

        // ---- Constructor ----
        public PixelEntry(byte x, byte y, Color32 color)
        {
            _x = x;
            _y = y;
            _color = color;
        }
    }
}
