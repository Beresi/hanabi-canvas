// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Color Palette", menuName = "Hanabi Canvas/Config/Color Palette")]
    public class ColorPaletteSO : ScriptableObject
    {
        // ---- Constants ----
        private const int PALETTE_SIZE = 8;

        // ---- Serialized Fields ----
        [Header("Palette")]
        [Tooltip("Display name for this palette")]
        [SerializeField] private string _paletteName = "Default Palette";

        [Tooltip("8 colors in this palette. No duplicates. Alpha must be 255.")]
        [SerializeField] private Color32[] _colors = new Color32[]
        {
            new Color32(255, 50, 50, 255),
            new Color32(255, 128, 0, 255),
            new Color32(255, 255, 0, 255),
            new Color32(0, 255, 0, 255),
            new Color32(0, 255, 255, 255),
            new Color32(0, 128, 255, 255),
            new Color32(128, 0, 255, 255),
            new Color32(255, 255, 255, 255)
        };

        [Header("Background")]
        [Tooltip("Canvas background color")]
        [SerializeField] private Color32 _backgroundColor = new Color32(32, 32, 32, 255);

        // ---- Properties ----
        public string PaletteName => _paletteName;
        public Color32[] Colors => _colors;
        public Color32 BackgroundColor => _backgroundColor;

        // ---- Validation ----
        private void OnValidate()
        {
            if (_colors == null || _colors.Length != PALETTE_SIZE)
            {
                Color32[] resized = new Color32[PALETTE_SIZE];
                if (_colors != null)
                {
                    for (int i = 0; i < Mathf.Min(_colors.Length, PALETTE_SIZE); i++)
                    {
                        resized[i] = _colors[i];
                    }
                }
                _colors = resized;
            }

            for (int i = 0; i < PALETTE_SIZE; i++)
            {
                if (_colors[i].a != 255)
                {
                    _colors[i] = new Color32(_colors[i].r, _colors[i].g, _colors[i].b, 255);
                }
            }

            if (_backgroundColor.a != 255)
            {
                _backgroundColor = new Color32(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 255);
            }

            for (int i = 0; i < PALETTE_SIZE; i++)
            {
                for (int j = i + 1; j < PALETTE_SIZE; j++)
                {
                    if (_colors[i].r == _colors[j].r &&
                        _colors[i].g == _colors[j].g &&
                        _colors[i].b == _colors[j].b)
                    {
                        Debug.LogWarning(
                            $"[ColorPaletteSO] Duplicate color found at index {i} and {j} " +
                            $"in palette '{_paletteName}'.", this);
                    }
                }
            }
        }
    }
}
