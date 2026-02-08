// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime.Canvas
{
    public class PixelCanvas : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("Configuration")]
        [Tooltip("Canvas configuration defining grid size, cell size, and palette")]
        [SerializeField] private CanvasConfigSO _config;

        [Tooltip("Output pixel data asset — written to on launch")]
        [SerializeField] private PixelDataSO _outputData;

        [Header("References")]
        [Tooltip("Camera used for screen-to-world input conversion")]
        [SerializeField] private Camera _camera;

        [Header("Shared Variables")]
        [Tooltip("Shared active color index — written by PaletteUI, read by PixelCanvas")]
        [SerializeField] private IntVariableSO _activeColorIndex;

        [Tooltip("Shared active tool index — written by CanvasToolbar, read by PixelCanvas")]
        [SerializeField] private IntVariableSO _activeToolIndex;

        [Header("Events")]
        [Tooltip("Listened to — clears the canvas when raised")]
        [SerializeField] private GameEventSO _onCanvasCleared;

        [Tooltip("Listened to — serializes pixel data when raised")]
        [SerializeField] private GameEventSO _onLaunchFirework;

        // ---- Private Fields ----
        private Color32?[,] _grid;
        private int _activeColorIndexValue;
        private CanvasTool _activeTool;
        private Vector2Int _hoveredCell;
        private bool _isInputEnabled;
        private bool _isDirty;

        // ---- Events ----
        public event System.Action OnGridChanged;

        // ---- Properties ----
        public int GridWidth => _config != null ? _config.GridWidth : 0;
        public int GridHeight => _config != null ? _config.GridHeight : 0;
        public float CellSize => _config != null ? _config.CellSize : 0f;
        public Vector2Int HoveredCell => _hoveredCell;
        public CanvasTool ActiveTool => _activeTool;
        public int ActiveColorIndex => _activeColorIndexValue;

        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set => _isInputEnabled = value;
        }

        public Color32 BackgroundColor
        {
            get
            {
                if (_config != null && _config.DefaultPalette != null)
                {
                    return _config.DefaultPalette.BackgroundColor;
                }
                return new Color32(32, 32, 32, 255);
            }
        }

        // ---- Unity Methods ----
        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogWarning($"[{nameof(PixelCanvas)}] CanvasConfigSO is not assigned.", this);
                enabled = false;
                return;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    Debug.LogWarning($"[{nameof(PixelCanvas)}] No camera assigned and Camera.main is null.", this);
                }
            }

            InitializeGrid();
            _isInputEnabled = true;
        }

        private void OnEnable()
        {
            if (_activeColorIndex != null)
            {
                _activeColorIndex.OnValueChanged += HandleActiveColorChanged;
            }

            if (_activeToolIndex != null)
            {
                _activeToolIndex.OnValueChanged += HandleActiveToolChanged;
            }

            if (_onCanvasCleared != null)
            {
                _onCanvasCleared.Register(HandleCanvasCleared);
            }

            if (_onLaunchFirework != null)
            {
                _onLaunchFirework.Register(HandleLaunchFirework);
            }
        }

        private void OnDisable()
        {
            if (_activeColorIndex != null)
            {
                _activeColorIndex.OnValueChanged -= HandleActiveColorChanged;
            }

            if (_activeToolIndex != null)
            {
                _activeToolIndex.OnValueChanged -= HandleActiveToolChanged;
            }

            if (_onCanvasCleared != null)
            {
                _onCanvasCleared.Unregister(HandleCanvasCleared);
            }

            if (_onLaunchFirework != null)
            {
                _onLaunchFirework.Unregister(HandleLaunchFirework);
            }
        }

        private void Update()
        {
            if (!_isInputEnabled || _camera == null)
            {
                return;
            }

            UpdateHoveredCell();
            HandleToolInput();
        }

        // ---- Public Methods ----
        public void Initialize(CanvasConfigSO config, PixelDataSO outputData)
        {
            _config = config;
            _outputData = outputData;
            InitializeGrid();
            _isInputEnabled = true;
            enabled = true;
        }

        public Color32? GetCellColor(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return null;
            }
            return _grid[x, y];
        }

        public void SetCell(int x, int y, Color32 color)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }

            _grid[x, y] = color;
            MarkDirty();
        }

        public void EraseCell(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }

            _grid[x, y] = null;
            MarkDirty();
        }

        public void SetActiveColor(int index)
        {
            if (_config == null || _config.DefaultPalette == null)
            {
                return;
            }

            _activeColorIndexValue = Mathf.Clamp(index, 0, _config.DefaultPalette.Colors.Length - 1);
        }

        public void SetActiveTool(CanvasTool tool)
        {
            _activeTool = tool;
        }

        public void Clear()
        {
            if (_grid == null)
            {
                return;
            }

            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    _grid[x, y] = null;
                }
            }

            MarkDirty();
        }

        public void SerializeToPixelData()
        {
            if (_outputData == null)
            {
                Debug.LogWarning($"[{nameof(PixelCanvas)}] PixelDataSO is not assigned. Cannot serialize.", this);
                return;
            }

            _outputData.Clear();

            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (_grid[x, y].HasValue)
                    {
                        _outputData.SetPixel((byte)x, (byte)y, _grid[x, y].Value);
                    }
                }
            }
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            Vector3 diff = worldPosition - transform.position;
            float localX = Vector3.Dot(diff, transform.right);
            float localY = Vector3.Dot(diff, transform.up);

            float halfWidth = GridWidth * CellSize * 0.5f;
            float halfHeight = GridHeight * CellSize * 0.5f;

            int gridX = Mathf.FloorToInt((localX + halfWidth) / CellSize);
            int gridY = Mathf.FloorToInt((localY + halfHeight) / CellSize);

            return new Vector2Int(gridX, gridY);
        }

        public Vector3 GridToWorld(int gridX, int gridY)
        {
            float halfWidth = GridWidth * CellSize * 0.5f;
            float halfHeight = GridHeight * CellSize * 0.5f;

            float localX = (gridX + 0.5f) * CellSize - halfWidth;
            float localY = (gridY + 0.5f) * CellSize - halfHeight;

            return transform.position + transform.right * localX + transform.up * localY;
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;
        }

        public void Fill(int x, int y, Color32 color)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }

            FloodFill(x, y, color);
        }

        public void ApplyToolAt(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }

            Color32[] paletteColors = GetPaletteColors();
            if (paletteColors == null)
            {
                return;
            }

            switch (_activeTool)
            {
                case CanvasTool.Draw:
                    _grid[x, y] = paletteColors[_activeColorIndexValue];
                    MarkDirty();
                    break;
                case CanvasTool.Erase:
                    _grid[x, y] = null;
                    MarkDirty();
                    break;
                case CanvasTool.Fill:
                    FloodFill(x, y, paletteColors[_activeColorIndexValue]);
                    break;
            }
        }

        public int FilledPixelCount
        {
            get
            {
                if (_grid == null)
                {
                    return 0;
                }

                int count = 0;
                for (int x = 0; x < GridWidth; x++)
                {
                    for (int y = 0; y < GridHeight; y++)
                    {
                        if (_grid[x, y].HasValue)
                        {
                            count++;
                        }
                    }
                }
                return count;
            }
        }

        // ---- Private Methods ----
        private void InitializeGrid()
        {
            _grid = new Color32?[GridWidth, GridHeight];
            _activeColorIndexValue = 0;
            _activeTool = CanvasTool.Draw;
            _hoveredCell = new Vector2Int(-1, -1);
            _isDirty = false;
        }

        private void HandleActiveColorChanged(int newIndex)
        {
            SetActiveColor(newIndex);
        }

        private void HandleActiveToolChanged(int newToolIndex)
        {
            if (System.Enum.IsDefined(typeof(CanvasTool), newToolIndex))
            {
                SetActiveTool((CanvasTool)newToolIndex);
            }
        }

        private void HandleCanvasCleared()
        {
            Clear();
        }

        private void HandleLaunchFirework()
        {
            SerializeToPixelData();
        }

        private void UpdateHoveredCell()
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = _camera.nearClipPlane;
            Vector3 worldPos = _camera.ScreenToWorldPoint(mouseScreenPos);

            Vector2Int gridPos = WorldToGrid(worldPos);

            if (IsInBounds(gridPos.x, gridPos.y))
            {
                _hoveredCell = gridPos;
            }
            else
            {
                _hoveredCell = new Vector2Int(-1, -1);
            }
        }

        private void HandleToolInput()
        {
            if (_hoveredCell.x < 0)
            {
                return;
            }

            bool shouldApply = false;

            switch (_activeTool)
            {
                case CanvasTool.Draw:
                case CanvasTool.Erase:
                    shouldApply = Input.GetMouseButton(0);
                    break;
                case CanvasTool.Fill:
                    shouldApply = Input.GetMouseButtonDown(0);
                    break;
            }

            if (shouldApply)
            {
                ApplyToolAt(_hoveredCell.x, _hoveredCell.y);
            }
        }

        private void FloodFill(int startX, int startY, Color32 fillColor)
        {
            Color32? targetColor = _grid[startX, startY];

            if (targetColor.HasValue && ColorsEqual(targetColor.Value, fillColor))
            {
                return;
            }

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            bool[,] visited = new bool[GridWidth, GridHeight];

            queue.Enqueue(new Vector2Int(startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                _grid[cell.x, cell.y] = fillColor;

                TryEnqueueNeighbor(cell.x + 1, cell.y, targetColor, visited, queue);
                TryEnqueueNeighbor(cell.x - 1, cell.y, targetColor, visited, queue);
                TryEnqueueNeighbor(cell.x, cell.y + 1, targetColor, visited, queue);
                TryEnqueueNeighbor(cell.x, cell.y - 1, targetColor, visited, queue);
            }

            MarkDirty();
        }

        private void TryEnqueueNeighbor(int x, int y, Color32? targetColor,
            bool[,] visited, Queue<Vector2Int> queue)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }

            if (visited[x, y])
            {
                return;
            }

            if (!ColorsMatch(_grid[x, y], targetColor))
            {
                return;
            }

            visited[x, y] = true;
            queue.Enqueue(new Vector2Int(x, y));
        }

        private void MarkDirty()
        {
            _isDirty = true;
            OnGridChanged?.Invoke();
        }

        private Color32[] GetPaletteColors()
        {
            if (_config == null || _config.DefaultPalette == null)
            {
                return null;
            }
            return _config.DefaultPalette.Colors;
        }

        private static bool ColorsEqual(Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

        private static bool ColorsMatch(Color32? a, Color32? b)
        {
            if (!a.HasValue && !b.HasValue)
            {
                return true;
            }

            if (!a.HasValue || !b.HasValue)
            {
                return false;
            }

            return ColorsEqual(a.Value, b.Value);
        }
    }
}
