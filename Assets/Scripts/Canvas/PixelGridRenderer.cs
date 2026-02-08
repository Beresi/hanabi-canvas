// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Canvas
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PixelGridRenderer : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("References")]
        [Tooltip("The PixelCanvas whose grid state this renderer displays")]
        [SerializeField] private PixelCanvas _pixelCanvas;

        [Header("Grid Lines")]
        [Tooltip("Color of the grid lines overlaid on the canvas")]
        [SerializeField] private Color _gridLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Tooltip("Width of grid lines relative to cell size (0-1)")]
        [Range(0.001f, 0.1f)]
        [SerializeField] private float _gridLineWidth = 0.02f;

        [Header("Hover")]
        [Tooltip("Color of the hover highlight on the active cell")]
        [SerializeField] private Color _hoverColor = new Color(1f, 1f, 1f, 0.3f);

        // ---- Private Fields ----
        private Texture2D _texture;
        private Color32[] _pixels;
        private MeshRenderer _meshRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private bool _isDirty;

        // ---- Shader Property IDs ----
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BackgroundColorId = Shader.PropertyToID("_BackgroundColor");
        private static readonly int GridColorId = Shader.PropertyToID("_GridColor");
        private static readonly int GridLineWidthId = Shader.PropertyToID("_GridLineWidth");
        private static readonly int GridSizeId = Shader.PropertyToID("_GridSize");
        private static readonly int HoverCellId = Shader.PropertyToID("_HoverCell");
        private static readonly int HoverColorId = Shader.PropertyToID("_HoverColor");

        // ---- Unity Methods ----
        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _propertyBlock = new MaterialPropertyBlock();

            if (_pixelCanvas == null)
            {
                Debug.LogWarning($"[{nameof(PixelGridRenderer)}] PixelCanvas reference is not assigned.", this);
                enabled = false;
                return;
            }

            InitializeTexture();
            UpdateScale();
        }

        private void OnEnable()
        {
            if (_pixelCanvas != null)
            {
                _pixelCanvas.OnGridChanged += HandleGridChanged;
                _isDirty = true;
            }
        }

        private void OnDisable()
        {
            if (_pixelCanvas != null)
            {
                _pixelCanvas.OnGridChanged -= HandleGridChanged;
            }
        }

        private void OnDestroy()
        {
            if (_texture != null)
            {
                Destroy(_texture);
            }
        }

        private void LateUpdate()
        {
            if (_isDirty)
            {
                RefreshTexture();
                _isDirty = false;
            }

            UpdateShaderProperties();
        }

        // ---- Public Methods ----
        public void Initialize(PixelCanvas pixelCanvas)
        {
            _pixelCanvas = pixelCanvas;
            _meshRenderer = GetComponent<MeshRenderer>();
            _propertyBlock = new MaterialPropertyBlock();

            InitializeTexture();
            UpdateScale();

            _pixelCanvas.OnGridChanged += HandleGridChanged;
            _isDirty = true;
        }

        // ---- Private Methods ----
        private void InitializeTexture()
        {
            int width = _pixelCanvas.GridWidth;
            int height = _pixelCanvas.GridHeight;

            _texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            _pixels = new Color32[width * height];

            RefreshTexture();

            _meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(MainTexId, _texture);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void UpdateScale()
        {
            float worldWidth = _pixelCanvas.GridWidth * _pixelCanvas.CellSize;
            float worldHeight = _pixelCanvas.GridHeight * _pixelCanvas.CellSize;
            transform.localScale = new Vector3(worldWidth, worldHeight, 1f);
        }

        private void RefreshTexture()
        {
            int width = _pixelCanvas.GridWidth;
            int height = _pixelCanvas.GridHeight;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color32? cellColor = _pixelCanvas.GetCellColor(x, y);
                    _pixels[y * width + x] = cellColor.HasValue
                        ? cellColor.Value
                        : new Color32(0, 0, 0, 0);
                }
            }

            _texture.SetPixels32(_pixels);
            _texture.Apply();
        }

        private void UpdateShaderProperties()
        {
            _meshRenderer.GetPropertyBlock(_propertyBlock);

            Color32 bgColor = _pixelCanvas.BackgroundColor;
            _propertyBlock.SetColor(BackgroundColorId, bgColor);
            _propertyBlock.SetColor(GridColorId, _gridLineColor);
            _propertyBlock.SetFloat(GridLineWidthId, _gridLineWidth);
            _propertyBlock.SetVector(GridSizeId, new Vector4(
                _pixelCanvas.GridWidth, _pixelCanvas.GridHeight, 0f, 0f));

            Vector2Int hover = _pixelCanvas.HoveredCell;
            _propertyBlock.SetVector(HoverCellId, new Vector4(hover.x, hover.y, 0f, 0f));
            _propertyBlock.SetColor(HoverColorId, _hoverColor);

            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void HandleGridChanged()
        {
            _isDirty = true;
        }
    }
}
