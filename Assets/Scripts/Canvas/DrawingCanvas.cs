// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime
{
    public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
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

        private Rect _gridRect;
        private bool _isDirty = false;
        private Texture2D _texture;
        private PointerEventData.InputButton _activeButton;

        void Start()
        {
            _gridRect = _gridBoard.rectTransform.rect;
            _isDirty = false;
            DrawOverlay();
            InitializeCanvas();
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

        public void Paint(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _gridBoard.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            float u = (localPoint.x - _gridRect.x) / _gridRect.width;
            float v = (localPoint.y - _gridRect.y) / _gridRect.height;

            var gridSize = _config.GridSize;

            int px = Mathf.Clamp(Mathf.FloorToInt(u * gridSize), 0, gridSize - 1);
            int py = Mathf.Clamp(Mathf.FloorToInt(v * gridSize), 0, gridSize - 1);

            if (_activeButton == PointerEventData.InputButton.Right)
            {
                if (_texture.GetPixel(px, py) == Color.clear)
                    return;

                _texture.SetPixel(px, py, Color.clear);
            }
            else
            {
                if (_texture.GetPixel(px, py) == _currentColor.Value)
                    return;

                _texture.SetPixel(px, py, _currentColor.Value);
            }

            _isDirty = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _activeButton = eventData.button;
            Paint(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

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

        private void HandleSavePattern()
        {
            _patternLibrary.Add(GetFireworkPattern());
        }

        private void HandleCanvasCleared()
        {
            InitializeCanvas();
        }
    }
}
