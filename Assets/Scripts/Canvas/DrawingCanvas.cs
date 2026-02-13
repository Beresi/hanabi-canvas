using UnityEngine;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HanabiCanvas.Runtime
{
    public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [Header("Settings")]
        [SerializeField] private DrawingCanvasConfigSO _config = default;
        [SerializeField] private ColorVariableSO _currentColor = default;

        [Header("Components")]
        [SerializeField] private RawImage _gridBoard;
        [SerializeField] private RawImage _gridOverylay;

        private Rect _gridRect;
        private bool _isDirty = false;
        private Texture2D _texture;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _gridRect = _gridBoard.rectTransform.rect;
            _isDirty = false;
            DrawOverlay();
            InitializeCanvas();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        [ContextMenu("Initialize Canvas")]
        private void InitializeCanvas()
        {
            _texture = new Texture2D(_config.GridSize, _config.GridSize, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Point;

            // Fill with white (or whatever default)
            Color[] pixels = new Color[_config.GridSize * _config.GridSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = _config.BackgroundColor;
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

            if (_texture.GetPixel(px, py) == _currentColor.Value)
                return;

            _texture.SetPixel(px, py, _currentColor.Value);
            _isDirty = true;
        }

        public void EraseCell()
        {

        }

        public void OnPointerDown(PointerEventData eventData) => Paint(eventData);


        private void LateUpdate()
        {
            if (!_isDirty) return;

            _texture.Apply();
            _isDirty = false;
        }

        public void OnDrag(PointerEventData eventData) => Paint(eventData);
    }
}
