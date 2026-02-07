// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Pixel Data", menuName = "Hanabi Canvas/Data/Pixel Data")]
    public class PixelDataSO : ScriptableObject
    {
        // ---- Constants ----
        private const int MIN_DIMENSION = 1;
        private const int MAX_DIMENSION = 255;

        // ---- Serialized Fields ----
        [Header("Grid Dimensions")]
        [Tooltip("Width of the pixel grid")]
        [Min(1)]
        [SerializeField] private int _width = 32;

        [Tooltip("Height of the pixel grid")]
        [Min(1)]
        [SerializeField] private int _height = 32;

        [Header("Pixel Data")]
        [Tooltip("Sparse list of filled pixels")]
        [SerializeField] private List<PixelEntry> _pixels = new List<PixelEntry>();

        // ---- Properties ----
        public int Width => _width;
        public int Height => _height;
        public int PixelCount => _pixels.Count;

        // ---- Public Methods ----
        public void Clear()
        {
            _pixels.Clear();
        }

        public void SetPixel(byte x, byte y, Color32 color)
        {
            if (x >= _width || y >= _height)
            {
                Debug.LogWarning(
                    $"[PixelDataSO] SetPixel out of bounds: ({x}, {y}) on a {_width}x{_height} grid.",
                    this);
                return;
            }

            for (int i = 0; i < _pixels.Count; i++)
            {
                if (_pixels[i].X == x && _pixels[i].Y == y)
                {
                    _pixels[i] = new PixelEntry(x, y, color);
                    return;
                }
            }

            _pixels.Add(new PixelEntry(x, y, color));
        }

        public Color32? GetPixel(byte x, byte y)
        {
            for (int i = 0; i < _pixels.Count; i++)
            {
                if (_pixels[i].X == x && _pixels[i].Y == y)
                {
                    return _pixels[i].Color;
                }
            }

            return null;
        }

        public void RemovePixel(byte x, byte y)
        {
            for (int i = 0; i < _pixels.Count; i++)
            {
                if (_pixels[i].X == x && _pixels[i].Y == y)
                {
                    _pixels.RemoveAt(i);
                    return;
                }
            }
        }

        public string ToJson()
        {
            PixelDataJson wrapper = new PixelDataJson
            {
                width = _width,
                height = _height,
                pixels = _pixels.ToArray()
            };
            return JsonUtility.ToJson(wrapper, true);
        }

        public void FromJson(string json)
        {
            PixelDataJson wrapper = JsonUtility.FromJson<PixelDataJson>(json);
            _width = Mathf.Clamp(wrapper.width, MIN_DIMENSION, MAX_DIMENSION);
            _height = Mathf.Clamp(wrapper.height, MIN_DIMENSION, MAX_DIMENSION);
            _pixels.Clear();
            if (wrapper.pixels != null)
            {
                for (int i = 0; i < wrapper.pixels.Length; i++)
                {
                    _pixels.Add(wrapper.pixels[i]);
                }
            }
        }

        // ---- Validation ----
        private void OnValidate()
        {
            _width = Mathf.Clamp(_width, MIN_DIMENSION, MAX_DIMENSION);
            _height = Mathf.Clamp(_height, MIN_DIMENSION, MAX_DIMENSION);
        }

        // ---- Private Types ----
        [System.Serializable]
        private struct PixelDataJson
        {
            public int width;
            public int height;
            public PixelEntry[] pixels;
        }
    }
}
