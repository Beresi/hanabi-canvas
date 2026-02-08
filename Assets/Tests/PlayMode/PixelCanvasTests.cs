// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Canvas;
using HanabiCanvas.Runtime.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HanabiCanvas.Tests.PlayMode
{
    public class PixelCanvasTests
    {
        // ---- Private Fields ----
        private GameObject _canvasObject;
        private PixelCanvas _canvas;
        private CanvasConfigSO _config;
        private ColorPaletteSO _palette;
        private PixelDataSO _outputData;
        private GameEventSO _onCanvasCleared;
        private GameEventSO _onLaunchFirework;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _palette = ScriptableObject.CreateInstance<ColorPaletteSO>();
            _config = ScriptableObject.CreateInstance<CanvasConfigSO>();
            _outputData = ScriptableObject.CreateInstance<PixelDataSO>();
            _onCanvasCleared = ScriptableObject.CreateInstance<GameEventSO>();
            _onLaunchFirework = ScriptableObject.CreateInstance<GameEventSO>();

#if UNITY_EDITOR
            SerializedObject configSO = new SerializedObject(_config);
            configSO.FindProperty("_gridWidth").intValue = 16;
            configSO.FindProperty("_gridHeight").intValue = 16;
            configSO.FindProperty("_cellSize").floatValue = 0.25f;
            configSO.FindProperty("_defaultPalette").objectReferenceValue = _palette;
            configSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _canvasObject = new GameObject("TestPixelCanvas");
            _canvasObject.SetActive(false);
            _canvas = _canvasObject.AddComponent<PixelCanvas>();

#if UNITY_EDITOR
            SerializedObject canvasSO = new SerializedObject(_canvas);
            canvasSO.FindProperty("_config").objectReferenceValue = _config;
            canvasSO.FindProperty("_outputData").objectReferenceValue = _outputData;
            canvasSO.FindProperty("_onCanvasCleared").objectReferenceValue = _onCanvasCleared;
            canvasSO.FindProperty("_onLaunchFirework").objectReferenceValue = _onLaunchFirework;
            canvasSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _canvasObject.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            if (_canvasObject != null)
            {
                Object.DestroyImmediate(_canvasObject);
            }
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_palette);
            Object.DestroyImmediate(_outputData);
            Object.DestroyImmediate(_onCanvasCleared);
            Object.DestroyImmediate(_onLaunchFirework);
        }

        // ---- Grid Initialization ----
        [UnityTest]
        public IEnumerator Awake_WithConfig_InitializesGrid()
        {
            yield return null;

            Assert.AreEqual(16, _canvas.GridWidth);
            Assert.AreEqual(16, _canvas.GridHeight);
        }

        [UnityTest]
        public IEnumerator Awake_WithConfig_DefaultsToDraw()
        {
            yield return null;

            Assert.AreEqual(CanvasTool.Draw, _canvas.ActiveTool);
            Assert.AreEqual(0, _canvas.ActiveColorIndex);
        }

        // ---- Cell Operations ----
        [UnityTest]
        public IEnumerator SetCell_ValidPosition_StoresColor()
        {
            yield return null;

            Color32 color = new Color32(255, 0, 0, 255);
            _canvas.SetCell(5, 5, color);

            Color32? result = _canvas.GetCellColor(5, 5);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(color.r, result.Value.r);
            Assert.AreEqual(color.g, result.Value.g);
            Assert.AreEqual(color.b, result.Value.b);
        }

        [UnityTest]
        public IEnumerator SetCell_OutOfBounds_DoesNothing()
        {
            yield return null;

            _canvas.SetCell(-1, 0, new Color32(255, 0, 0, 255));
            _canvas.SetCell(0, -1, new Color32(255, 0, 0, 255));
            _canvas.SetCell(100, 0, new Color32(255, 0, 0, 255));

            Assert.AreEqual(0, _canvas.FilledPixelCount);
        }

        [UnityTest]
        public IEnumerator GetCellColor_EmptyCell_ReturnsNull()
        {
            yield return null;

            Color32? result = _canvas.GetCellColor(0, 0);
            Assert.IsFalse(result.HasValue);
        }

        [UnityTest]
        public IEnumerator EraseCell_FilledCell_ClearsIt()
        {
            yield return null;

            _canvas.SetCell(3, 3, new Color32(255, 0, 0, 255));
            Assert.AreEqual(1, _canvas.FilledPixelCount);

            _canvas.EraseCell(3, 3);
            Assert.IsFalse(_canvas.GetCellColor(3, 3).HasValue);
            Assert.AreEqual(0, _canvas.FilledPixelCount);
        }

        // ---- Tool Selection ----
        [UnityTest]
        public IEnumerator SetActiveTool_ChangesTool()
        {
            yield return null;

            _canvas.SetActiveTool(CanvasTool.Erase);
            Assert.AreEqual(CanvasTool.Erase, _canvas.ActiveTool);

            _canvas.SetActiveTool(CanvasTool.Fill);
            Assert.AreEqual(CanvasTool.Fill, _canvas.ActiveTool);
        }

        [UnityTest]
        public IEnumerator SetActiveColor_ValidIndex_UpdatesIndex()
        {
            yield return null;

            _canvas.SetActiveColor(3);
            Assert.AreEqual(3, _canvas.ActiveColorIndex);
        }

        [UnityTest]
        public IEnumerator SetActiveColor_OutOfRange_ClampsToValid()
        {
            yield return null;

            _canvas.SetActiveColor(100);
            Assert.AreEqual(7, _canvas.ActiveColorIndex);

            _canvas.SetActiveColor(-5);
            Assert.AreEqual(0, _canvas.ActiveColorIndex);
        }

        // ---- Clear ----
        [UnityTest]
        public IEnumerator Clear_WithFilledCells_ResetsAll()
        {
            yield return null;

            _canvas.SetCell(0, 0, new Color32(255, 0, 0, 255));
            _canvas.SetCell(1, 1, new Color32(0, 255, 0, 255));
            _canvas.SetCell(2, 2, new Color32(0, 0, 255, 255));
            Assert.AreEqual(3, _canvas.FilledPixelCount);

            _canvas.Clear();
            Assert.AreEqual(0, _canvas.FilledPixelCount);
        }

        [UnityTest]
        public IEnumerator OnCanvasClearedEvent_WhenRaised_ClearsGrid()
        {
            yield return null;

            _canvas.SetCell(0, 0, new Color32(255, 0, 0, 255));
            _canvas.SetCell(1, 1, new Color32(0, 255, 0, 255));
            Assert.AreEqual(2, _canvas.FilledPixelCount);

            _onCanvasCleared.Raise();

            Assert.AreEqual(0, _canvas.FilledPixelCount);
        }

        [UnityTest]
        public IEnumerator OnLaunchFireworkEvent_WhenRaised_SerializesToOutput()
        {
            yield return null;

            _canvas.SetCell(0, 0, new Color32(255, 0, 0, 255));
            _canvas.SetCell(5, 10, new Color32(0, 255, 0, 255));

            _onLaunchFirework.Raise();

            Assert.AreEqual(2, _outputData.PixelCount);
        }

        // ---- Serialization ----
        [UnityTest]
        public IEnumerator SerializeToPixelData_WithFilledCells_WritesToOutput()
        {
            yield return null;

            _canvas.SetCell(0, 0, new Color32(255, 0, 0, 255));
            _canvas.SetCell(5, 10, new Color32(0, 255, 0, 255));

            _canvas.SerializeToPixelData();

            Assert.AreEqual(2, _outputData.PixelCount);
        }

        [UnityTest]
        public IEnumerator SerializeToPixelData_EmptyGrid_ClearsOutput()
        {
            yield return null;

            _outputData.SetPixel(0, 0, new Color32(255, 0, 0, 255));
            Assert.AreEqual(1, _outputData.PixelCount);

            _canvas.SerializeToPixelData();
            Assert.AreEqual(0, _outputData.PixelCount);
        }

        // ---- OnGridChanged Event ----
        [UnityTest]
        public IEnumerator OnGridChanged_WhenCellSet_FiresEvent()
        {
            yield return null;

            int callCount = 0;
            _canvas.OnGridChanged += () => callCount++;

            _canvas.SetCell(0, 0, new Color32(255, 0, 0, 255));

            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator OnGridChanged_WhenErased_FiresEvent()
        {
            yield return null;

            _canvas.SetCell(0, 0, new Color32(255, 0, 0, 255));

            int callCount = 0;
            _canvas.OnGridChanged += () => callCount++;

            _canvas.EraseCell(0, 0);

            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator OnGridChanged_WhenCleared_FiresEvent()
        {
            yield return null;

            _canvas.SetCell(0, 0, new Color32(255, 0, 0, 255));

            int callCount = 0;
            _canvas.OnGridChanged += () => callCount++;

            _canvas.Clear();

            Assert.AreEqual(1, callCount);
        }

        // ---- Bounds Checking ----
        [UnityTest]
        public IEnumerator IsInBounds_ValidPosition_ReturnsTrue()
        {
            yield return null;

            Assert.IsTrue(_canvas.IsInBounds(0, 0));
            Assert.IsTrue(_canvas.IsInBounds(15, 15));
            Assert.IsTrue(_canvas.IsInBounds(8, 8));
        }

        [UnityTest]
        public IEnumerator IsInBounds_InvalidPosition_ReturnsFalse()
        {
            yield return null;

            Assert.IsFalse(_canvas.IsInBounds(-1, 0));
            Assert.IsFalse(_canvas.IsInBounds(0, -1));
            Assert.IsFalse(_canvas.IsInBounds(16, 0));
            Assert.IsFalse(_canvas.IsInBounds(0, 16));
        }

        // ---- Coordinate Conversion ----
        [UnityTest]
        public IEnumerator WorldToGrid_CanvasCenter_ReturnsCenterCell()
        {
            yield return null;

            Vector3 center = _canvasObject.transform.position;
            Vector2Int gridPos = _canvas.WorldToGrid(center);

            Assert.AreEqual(8, gridPos.x);
            Assert.AreEqual(8, gridPos.y);
        }

        [UnityTest]
        public IEnumerator GridToWorld_CenterCell_ReturnsExpectedPosition()
        {
            yield return null;

            Vector3 worldPos = _canvas.GridToWorld(8, 8);

            // Cell (8,8) center = (8+0.5)*0.25 - 16*0.25/2 = 0.125
            float expectedOffset = (8 + 0.5f) * _canvas.CellSize - _canvas.GridWidth * _canvas.CellSize * 0.5f;
            Assert.AreEqual(expectedOffset, worldPos.x, 0.01f);
            Assert.AreEqual(expectedOffset, worldPos.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator WorldToGrid_GridToWorld_RoundTrip()
        {
            yield return null;

            int testX = 5;
            int testY = 10;

            Vector3 worldPos = _canvas.GridToWorld(testX, testY);
            Vector2Int gridPos = _canvas.WorldToGrid(worldPos);

            Assert.AreEqual(testX, gridPos.x);
            Assert.AreEqual(testY, gridPos.y);
        }

        // ---- FilledPixelCount ----
        [UnityTest]
        public IEnumerator FilledPixelCount_EmptyGrid_ReturnsZero()
        {
            yield return null;

            Assert.AreEqual(0, _canvas.FilledPixelCount);
        }

        [UnityTest]
        public IEnumerator FilledPixelCount_AfterSetAndErase_ReturnsCorrect()
        {
            yield return null;

            _canvas.SetCell(0, 0, new Color32(255, 0, 0, 255));
            _canvas.SetCell(1, 1, new Color32(0, 255, 0, 255));
            Assert.AreEqual(2, _canvas.FilledPixelCount);

            _canvas.EraseCell(0, 0);
            Assert.AreEqual(1, _canvas.FilledPixelCount);
        }
    }
}
