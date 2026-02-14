// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(
        fileName = "New Color Palette",
        menuName = "Hanabi Canvas/Config/Color Palette")]
    public class ColorPaletteSO : ScriptableObject
    {
        // ---- Serialized Fields ----
        [SerializeField] private Color[] _colors = new Color[]
        {
            new Color(1f, 0.2f, 0.2f, 1f),   // Red
            new Color(1f, 0.6f, 0.1f, 1f),   // Orange
            new Color(1f, 1f, 0.2f, 1f),     // Yellow
            new Color(0.2f, 1f, 0.3f, 1f),   // Green
            new Color(0.2f, 1f, 1f, 1f),     // Cyan
            new Color(0.3f, 0.4f, 1f, 1f),   // Blue
            new Color(0.7f, 0.3f, 1f, 1f),   // Purple
            new Color(1f, 1f, 1f, 1f),       // White
        };

        // ---- Properties ----
        public int Count => _colors.Length;

        // ---- Indexer ----
        public Color this[int index]
        {
            get
            {
                if (index < 0 || index >= _colors.Length)
                {
                    Debug.LogWarning($"ColorPaletteSO: Index {index} is out of bounds (0..{_colors.Length - 1}). Returning white.");
                    return Color.white;
                }
                return _colors[index];
            }
        }

        // ---- Validation ----
        private void OnValidate()
        {
            if (_colors == null)
            {
                return;
            }

            for (int i = 0; i < _colors.Length; i++)
            {
                if (_colors[i].a < 1f)
                {
                    _colors[i] = new Color(_colors[i].r, _colors[i].g, _colors[i].b, 1f);
                }
            }
        }
    }
}
