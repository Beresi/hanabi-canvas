// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HanabiCanvas.Runtime.Canvas;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Main drawing canvas. Handles pixel painting, undo/redo, and symmetry mode.
    /// </summary>
    public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        // ---- Constants ----
        private const int DEFAULT_MAX_UNDO_STEPS = 20;

        // ---- Serialized Fields ----

        [Header("Settings")]
        [SerializeField] private DrawingCanvasConfigSO _config = default;
        [SerializeField] private ColorVariableSO _currentColor = default;

        [Header("Components")]
        [SerializeField] private RawImage _gridBoard;
        [SerializeField] private RawImage _gridOverylay;

        [Header("Events")]
        [SerializeField] private PatternListSO _patternLibrary;
        [SerializeField] private GameEventSO _onSavePattern;
        [SerializeField] private GameEventSO _onCanvasCleared;
        [SerializeField] private GameEventSO _onPixelPainted;

        [Header("Symmetry")]
        [Tooltip("Whether symmetry drawing is enabled")]
        [SerializeField] private BoolVariableSO _isSymmetryEnabled;

        [Tooltip("Symmetry mode: 0=Horizontal, 1=Vertical, 2=Both")]
        [SerializeField] private IntVariableSO _symmetryMode;

        [Header("Undo/Redo")]
        [Tooltip("Maximum undo steps (default 20)")]
        [SerializeField] private int _maxUndoSteps = DEFAULT_MAX_UNDO_STEPS;

        // ---- Private Fields ----
        private Rect _gridRect;
        private bool _isDirty = false;
        private Texture2D _texture;
        private PointerEventData.InputButton _activeButton;
        private UndoRedoManager _undoRedoManager;
        private bool _hasSnapshotForStroke;

        // ---- Unity Methods ----

        private void Start()
        {
            _gridRect = _gridBoard.rectTransform.rect;
            _isDirty = false;
            DrawOverlay();
            InitializeCanvas();

            int gridTotal = _config.GridSize * _config.GridSize;
            _undoRedoManager = new UndoRedoManager(_maxUndoSteps, gridTotal);
        }

        private void Update()
        {
            HandleUndoRedoInput();
        }

        private void OnEnable()
        {
            if (_onSavePattern != null)
            {
                _onSavePattern.Register(HandleSavePattern);
            }
            if (_onCanvasCleared != null)
            {
                _onCanvasCleared.Register(HandleCanvasCleared);
            }
        }

        private void OnDisable()
        {
            if (_onSavePattern != null)
            {
                _onSavePattern.Unregister(HandleSavePattern);
            }
            if (_onCanvasCleared != null)
            {
                _onCanvasCleared.Unregister(HandleCanvasCleared);
            }
        }

        [ContextMenu("Initialize Canvas")]
        private void InitializeCanvas()
        {
            _texture = new Texture2D(_config.GridSize, _config.GridSize, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[_config.GridSize * _config.GridSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            _texture.SetPixels(pixels);
            _texture.Apply();

            _gridBoard.texture = _texture;
        }

        [ContextMenu("Draw overlay")]
        private void DrawOverlay()
        {
            Rect rect = _gridOverylay.rectTransform.rect;
            int rectWidth = (int)rect.width;
            int rectHeight = (int)rect.height;

            var tex = new Texture2D(rectWidth, rectHeight, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Point;

            Color[] pixels = new Color[rectWidth * rectHeight];

            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            float cellW = (float)rect.width / _config.GridSize;
            float cellH = (float)rect.height / _config.GridSize;

            for (int i = 1; i < _config.GridSize; i++)
            {
                int px = Mathf.RoundToInt(i * cellW);
                int py = Mathf.RoundToInt(i * cellH);

                for (int j = 0; j < _config.BordersThickness; j++)
                {
                    int rowY = Mathf.Min(py + j, rectHeight);
                    int colX = Mathf.Min(px + j, rectWidth);

                    for (int t = 0; t < rectWidth; t++)
                        pixels[rowY * rectWidth + t] = _config.GridColor;
                    for (int t = 0; (t + 1) < rectHeight; t++)
                        pixels[t * rectWidth + colX] = _config.GridColor;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _gridOverylay.texture = tex;
        }

        /// <summary>
        /// Paints or erases at the pointer position, applying symmetry if enabled.
        /// </summary>
        public void Paint(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _gridBoard.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return;
            }

            float u = (localPoint.x - _gridRect.x) / _gridRect.width;
            float v = (localPoint.y - _gridRect.y) / _gridRect.height;

            int gridSize = _config.GridSize;

            int px = Mathf.Clamp(Mathf.FloorToInt(u * gridSize), 0, gridSize - 1);
            int py = Mathf.Clamp(Mathf.FloorToInt(v * gridSize), 0, gridSize - 1);

            // Push undo snapshot once per stroke (on first paint of a pointer-down)
            if (!_hasSnapshotForStroke && _undoRedoManager != null)
            {
                _undoRedoManager.PushSnapshot(_texture.GetPixels32());
                _hasSnapshotForStroke = true;
            }

            bool isErase = _activeButton == PointerEventData.InputButton.Right;
            bool didChange = ApplyPixel(px, py, isErase);

            // Apply symmetry
            if (_isSymmetryEnabled != null && _isSymmetryEnabled.Value)
            {
                int mode = _symmetryMode != null ? _symmetryMode.Value : 0;
                int mirrorX = gridSize - 1 - px;
                int mirrorY = gridSize - 1 - py;

                if (mode == 0 || mode == 2)
                {
                    // Horizontal — mirror X
                    didChange |= ApplyPixel(mirrorX, py, isErase);
                }

                if (mode == 1 || mode == 2)
                {
                    // Vertical — mirror Y
                    didChange |= ApplyPixel(px, mirrorY, isErase);
                }

                if (mode == 2)
                {
                    // Both — diagonal mirror
                    didChange |= ApplyPixel(mirrorX, mirrorY, isErase);
                }
            }

            if (didChange)
            {
                _isDirty = true;
                if (_onPixelPainted != null)
                {
                    _onPixelPainted.Raise();
                }
            }
        }

        /// <inheritdoc/>
        public void OnPointerDown(PointerEventData eventData)
        {
            _activeButton = eventData.button;
            _hasSnapshotForStroke = false;
            Paint(eventData);
        }

        /// <inheritdoc/>
        public void OnPointerUp(PointerEventData eventData)
        {
            _hasSnapshotForStroke = false;
        }

        /// <inheritdoc/>
        public void OnDrag(PointerEventData eventData) => Paint(eventData);

        private void LateUpdate()
        {
            if (!_isDirty) return;

            _texture.Apply();
            _isDirty = false;
        }

        /// <summary>
        /// Reads the canvas texture and returns a FireworkPattern containing all non-transparent pixels.
        /// </summary>
        public FireworkPattern GetFireworkPattern()
        {
            int gridSize = _config.GridSize;
            Color32[] raw = _texture.GetPixels32();
            int count = 0;

            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].a > 0) count++;
            }

            PixelEntry[] entries = new PixelEntry[count];
            int idx = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].a > 0)
                {
                    byte x = (byte)(i % gridSize);
                    byte y = (byte)(i / gridSize);
                    entries[idx] = new PixelEntry(x, y, raw[i]);
                    idx++;
                }
            }

            return new FireworkPattern
            {
                Pixels = entries,
                Width = gridSize,
                Height = gridSize,
            };
        }

        /// <summary>
        /// Returns the number of unique non-transparent colors on the canvas.
        /// Not intended for per-frame use.
        /// </summary>
        public int GetUniqueColorCount()
        {
            if (_texture == null)
            {
                return 0;
            }

            Color32[] raw = _texture.GetPixels32();
            int count = 0;
            Color32[] found = new Color32[8];

            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].a == 0)
                {
                    continue;
                }

                bool isDuplicate = false;
                for (int j = 0; j < count; j++)
                {
                    if (found[j].r == raw[i].r && found[j].g == raw[i].g
                        && found[j].b == raw[i].b && found[j].a == raw[i].a)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate && count < found.Length)
                {
                    found[count] = raw[i];
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Returns an array of unique non-transparent colors on the canvas.
        /// Not intended for per-frame use.
        /// </summary>
        public Color32[] GetUniqueColors()
        {
            if (_texture == null)
            {
                return new Color32[0];
            }

            Color32[] raw = _texture.GetPixels32();
            int count = 0;
            Color32[] found = new Color32[8];

            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].a == 0)
                {
                    continue;
                }

                bool isDuplicate = false;
                for (int j = 0; j < count; j++)
                {
                    if (found[j].r == raw[i].r && found[j].g == raw[i].g
                        && found[j].b == raw[i].b && found[j].a == raw[i].a)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate && count < found.Length)
                {
                    found[count] = raw[i];
                    count++;
                }
            }

            Color32[] result = new Color32[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = found[i];
            }

            return result;
        }

        /// <summary>
        /// Returns the count of non-transparent (filled) pixels on the canvas.
        /// Not intended for per-frame use.
        /// </summary>
        public int GetFilledPixelCount()
        {
            if (_texture == null)
            {
                return 0;
            }

            Color32[] raw = _texture.GetPixels32();
            int count = 0;

            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].a > 0)
                {
                    count++;
                }
            }

            return count;
        }

        // ---- Public Methods (Undo/Redo) ----

        /// <summary>
        /// Undoes the last drawing action, restoring the previous grid state.
        /// </summary>
        public void Undo()
        {
            if (_undoRedoManager == null || !_undoRedoManager.CanUndo)
            {
                return;
            }

            Color32[] restored = _undoRedoManager.Undo(_texture.GetPixels32());
            if (restored != null)
            {
                _texture.SetPixels32(restored);
                _texture.Apply();
            }
        }

        /// <summary>
        /// Redoes the last undone action, restoring the redo state.
        /// </summary>
        public void Redo()
        {
            if (_undoRedoManager == null || !_undoRedoManager.CanRedo)
            {
                return;
            }

            Color32[] restored = _undoRedoManager.Redo(_texture.GetPixels32());
            if (restored != null)
            {
                _texture.SetPixels32(restored);
                _texture.Apply();
            }
        }

        // ---- Private Methods ----

        private bool ApplyPixel(int px, int py, bool isErase)
        {
            if (isErase)
            {
                if (_texture.GetPixel(px, py) == Color.clear)
                {
                    return false;
                }

                _texture.SetPixel(px, py, Color.clear);
                return true;
            }
            else
            {
                if (_texture.GetPixel(px, py) == _currentColor.Value)
                {
                    return false;
                }

                _texture.SetPixel(px, py, _currentColor.Value);
                return true;
            }
        }

        private void HandleUndoRedoInput()
        {
            bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (isCtrl && Input.GetKeyDown(KeyCode.Z))
            {
                bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                if (isShift)
                {
                    Redo();
                }
                else
                {
                    Undo();
                }
            }

            if (isCtrl && Input.GetKeyDown(KeyCode.Y))
            {
                Redo();
            }
        }

        private void HandleSavePattern()
        {
            _patternLibrary.Add(GetFireworkPattern());
        }

        private void HandleCanvasCleared()
        {
            if (_undoRedoManager != null)
            {
                _undoRedoManager.Clear();
            }

            InitializeCanvas();
        }
    }
}
