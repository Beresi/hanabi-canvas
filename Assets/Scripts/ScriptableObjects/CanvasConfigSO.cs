// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Canvas Config", menuName = "Hanabi Canvas/Config/Canvas Config")]
    public class CanvasConfigSO : ScriptableObject
    {
        // ---- Constants ----
        private const int MIN_GRID_SIZE = 8;
        private const int MAX_GRID_SIZE = 64;
        private const float MIN_CELL_SIZE = 0.01f;

        // ---- Serialized Fields ----
        [Header("Grid")]
        [Tooltip("Width of the pixel grid in cells")]
        [Range(8, 64)]
        [SerializeField] private int _gridWidth = 32;

        [Tooltip("Height of the pixel grid in cells")]
        [Range(8, 64)]
        [SerializeField] private int _gridHeight = 32;

        [Tooltip("World-space size of each cell for rendering")]
        [Min(0.01f)]
        [SerializeField] private float _cellSize = 0.25f;

        [Header("Palette")]
        [Tooltip("Default color palette for this canvas")]
        [SerializeField] private ColorPaletteSO _defaultPalette;

        // ---- Properties ----
        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public float CellSize => _cellSize;
        public ColorPaletteSO DefaultPalette => _defaultPalette;

        // ---- Validation ----
        private void OnValidate()
        {
            _gridWidth = Mathf.Clamp(_gridWidth, MIN_GRID_SIZE, MAX_GRID_SIZE);
            _gridHeight = Mathf.Clamp(_gridHeight, MIN_GRID_SIZE, MAX_GRID_SIZE);
            _cellSize = Mathf.Max(MIN_CELL_SIZE, _cellSize);

            if (_defaultPalette == null)
            {
                Debug.LogWarning("[CanvasConfigSO] Default palette is not assigned.", this);
            }
        }
    }
}
