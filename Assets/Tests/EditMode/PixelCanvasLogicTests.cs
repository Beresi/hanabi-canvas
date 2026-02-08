// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Canvas;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Tests.EditMode
{
    public class PixelCanvasLogicTests
    {
        // ---- Private Fields ----
        private CanvasConfigSO _config;
        private ColorPaletteSO _palette;
        private PixelDataSO _outputData;
        private GameObject _canvasObject;
        private PixelCanvas _canvas;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _palette = ScriptableObject.CreateInstance<ColorPaletteSO>();
            _config = ScriptableObject.CreateInstance<CanvasConfigSO>();
            _outputData = ScriptableObject.CreateInstance<PixelDataSO>();

            SerializedObject configSO = new SerializedObject(_config);
            configSO.FindProperty("_gridWidth").intValue = 8;
            configSO.FindProperty("_gridHeight").intValue = 8;
            configSO.FindProperty("_cellSize").floatValue = 1.0f;
            configSO.FindProperty("_defaultPalette").objectReferenceValue = _palette;
            configSO.ApplyModifiedPropertiesWithoutUndo();

            _canvasObject = new GameObject("TestCanvas");
            _canvas = _canvasObject.AddComponent<PixelCanvas>();
            _canvas.Initialize(_config, _outputData);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_canvasObject);
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_palette);
            Object.DestroyImmediate(_outputData);
        }

        // ---- Flood Fill Tests ----
        [Test]
        public void Fill_EmptyGrid_FillsAllCells()
        {
            _canvas.SetActiveTool(CanvasTool.Fill);
            _canvas.SetActiveColor(0);

            _canvas.ApplyToolAt(0, 0);

            Color32 expectedColor = _palette.Colors[0];
            Assert.AreEqual(64, _canvas.FilledPixelCount);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Color32? cell = _canvas.GetCellColor(x, y);
                    Assert.IsTrue(cell.HasValue, $"Cell ({x},{y}) should be filled");
                    Assert.AreEqual(expectedColor.r, cell.Value.r);
                    Assert.AreEqual(expectedColor.g, cell.Value.g);
                    Assert.AreEqual(expectedColor.b, cell.Value.b);
                }
            }
        }

        [Test]
        public void Fill_BoundedByWall_FillsOnlyConnected()
        {
            Color32 wallColor = _palette.Colors[1];

            // Create a vertical wall at x=3
            for (int y = 0; y < 8; y++)
            {
                _canvas.SetCell(3, y, wallColor);
            }

            _canvas.SetActiveTool(CanvasTool.Fill);
            _canvas.SetActiveColor(0);
            _canvas.ApplyToolAt(0, 0);

            Color32 fillColor = _palette.Colors[0];

            // Left side (x=0..2) should be filled
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Color32? cell = _canvas.GetCellColor(x, y);
                    Assert.IsTrue(cell.HasValue, $"Cell ({x},{y}) should be filled");
                    Assert.AreEqual(fillColor.r, cell.Value.r);
                }
            }

            // Wall (x=3) should keep its original color
            for (int y = 0; y < 8; y++)
            {
                Color32? cell = _canvas.GetCellColor(3, y);
                Assert.IsTrue(cell.HasValue);
                Assert.AreEqual(wallColor.r, cell.Value.r);
            }

            // Right side (x=4..7) should remain empty
            for (int x = 4; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Assert.IsFalse(_canvas.GetCellColor(x, y).HasValue,
                        $"Cell ({x},{y}) should remain empty (right side of wall)");
                }
            }
        }

        [Test]
        public void Fill_SameColorAsTarget_DoesNothing()
        {
            Color32 color = _palette.Colors[0];
            _canvas.SetCell(0, 0, color);

            int changeCount = 0;
            _canvas.OnGridChanged += () => changeCount++;

            _canvas.SetActiveTool(CanvasTool.Fill);
            _canvas.SetActiveColor(0);
            _canvas.ApplyToolAt(0, 0);

            Assert.AreEqual(0, changeCount);
        }

        [Test]
        public void Fill_ExistingColorRegion_ReplacesConnected()
        {
            Color32 originalColor = _palette.Colors[0];
            Color32 newColor = _palette.Colors[2];

            // Paint a 3x3 block
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    _canvas.SetCell(x, y, originalColor);
                }
            }

            _canvas.SetActiveTool(CanvasTool.Fill);
            _canvas.SetActiveColor(2);
            _canvas.ApplyToolAt(1, 1);

            // The 3x3 block should have the new color
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Color32? cell = _canvas.GetCellColor(x, y);
                    Assert.IsTrue(cell.HasValue);
                    Assert.AreEqual(newColor.r, cell.Value.r);
                    Assert.AreEqual(newColor.g, cell.Value.g);
                    Assert.AreEqual(newColor.b, cell.Value.b);
                }
            }

            // Cells outside the block should remain empty
            Assert.IsFalse(_canvas.GetCellColor(4, 4).HasValue);
        }

        [Test]
        public void Fill_LShapedRegion_FillsEntireConnectedArea()
        {
            Color32 wallColor = _palette.Colors[1];

            // Create L-shaped wall
            //  X X . .
            //  . X . .
            //  . X . .
            //  . . . .
            _canvas.SetCell(0, 3, wallColor);
            _canvas.SetCell(1, 3, wallColor);
            _canvas.SetCell(1, 2, wallColor);
            _canvas.SetCell(1, 1, wallColor);

            _canvas.SetActiveTool(CanvasTool.Fill);
            _canvas.SetActiveColor(0);
            _canvas.ApplyToolAt(4, 4);

            // Most cells should be filled (everything except wall cells and the enclosed corner)
            Color32 fillColor = _palette.Colors[0];
            Color32? openCell = _canvas.GetCellColor(4, 4);
            Assert.IsTrue(openCell.HasValue);
            Assert.AreEqual(fillColor.r, openCell.Value.r);

            // Wall cells should keep their color
            Color32? wallCell = _canvas.GetCellColor(1, 3);
            Assert.IsTrue(wallCell.HasValue);
            Assert.AreEqual(wallColor.r, wallCell.Value.r);
        }

        // ---- Coordinate Conversion Tests ----
        [Test]
        public void WorldToGrid_AtOrigin_ReturnsCenterCell()
        {
            // 8x8 grid, cellSize=1 → canvas spans -4 to +4
            // Origin maps to cell (4, 4)
            Vector2Int gridPos = _canvas.WorldToGrid(Vector3.zero);
            Assert.AreEqual(4, gridPos.x);
            Assert.AreEqual(4, gridPos.y);
        }

        [Test]
        public void WorldToGrid_BottomLeftCorner_ReturnsZeroZero()
        {
            Vector3 bottomLeft = new Vector3(-3.9f, -3.9f, 0f);
            Vector2Int gridPos = _canvas.WorldToGrid(bottomLeft);
            Assert.AreEqual(0, gridPos.x);
            Assert.AreEqual(0, gridPos.y);
        }

        [Test]
        public void WorldToGrid_TopRightCorner_ReturnsMaxCell()
        {
            Vector3 topRight = new Vector3(3.5f, 3.5f, 0f);
            Vector2Int gridPos = _canvas.WorldToGrid(topRight);
            Assert.AreEqual(7, gridPos.x);
            Assert.AreEqual(7, gridPos.y);
        }

        [Test]
        public void GridToWorld_ZeroZero_ReturnsBottomLeftCellCenter()
        {
            // Cell (0,0) center = (-4 + 0.5, -4 + 0.5) = (-3.5, -3.5)
            Vector3 worldPos = _canvas.GridToWorld(0, 0);
            Assert.AreEqual(-3.5f, worldPos.x, 0.01f);
            Assert.AreEqual(-3.5f, worldPos.y, 0.01f);
        }

        [Test]
        public void GridToWorld_MaxCell_ReturnsTopRightCellCenter()
        {
            // Cell (7,7) center = (-4 + 7.5, -4 + 7.5) = (3.5, 3.5)
            Vector3 worldPos = _canvas.GridToWorld(7, 7);
            Assert.AreEqual(3.5f, worldPos.x, 0.01f);
            Assert.AreEqual(3.5f, worldPos.y, 0.01f);
        }

        [Test]
        public void WorldToGrid_GridToWorld_RoundTrip_AllCells()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Vector3 worldPos = _canvas.GridToWorld(x, y);
                    Vector2Int gridPos = _canvas.WorldToGrid(worldPos);
                    Assert.AreEqual(x, gridPos.x, $"Round-trip X failed for cell ({x},{y})");
                    Assert.AreEqual(y, gridPos.y, $"Round-trip Y failed for cell ({x},{y})");
                }
            }
        }

        // ---- ApplyToolAt Tests ----
        [Test]
        public void ApplyToolAt_DrawTool_SetsColor()
        {
            _canvas.SetActiveTool(CanvasTool.Draw);
            _canvas.SetActiveColor(0);

            _canvas.ApplyToolAt(3, 3);

            Color32? cell = _canvas.GetCellColor(3, 3);
            Assert.IsTrue(cell.HasValue);
            Assert.AreEqual(_palette.Colors[0].r, cell.Value.r);
        }

        [Test]
        public void ApplyToolAt_EraseTool_ClearsCell()
        {
            _canvas.SetCell(3, 3, new Color32(255, 0, 0, 255));

            _canvas.SetActiveTool(CanvasTool.Erase);
            _canvas.ApplyToolAt(3, 3);

            Assert.IsFalse(_canvas.GetCellColor(3, 3).HasValue);
        }

        [Test]
        public void ApplyToolAt_OutOfBounds_DoesNothing()
        {
            _canvas.SetActiveTool(CanvasTool.Draw);
            _canvas.SetActiveColor(0);

            _canvas.ApplyToolAt(-1, 0);
            _canvas.ApplyToolAt(0, -1);
            _canvas.ApplyToolAt(8, 0);
            _canvas.ApplyToolAt(0, 8);

            Assert.AreEqual(0, _canvas.FilledPixelCount);
        }

        // ---- Serialization Tests ----
        [Test]
        public void SerializeToPixelData_SinglePixel_CorrectColorAndPosition()
        {
            Color32 color = new Color32(255, 128, 0, 255);
            _canvas.SetCell(3, 5, color);

            _canvas.SerializeToPixelData();

            Assert.AreEqual(1, _outputData.PixelCount);
            Color32? retrieved = _outputData.GetPixel(3, 5);
            Assert.IsTrue(retrieved.HasValue);
            Assert.AreEqual(color.r, retrieved.Value.r);
            Assert.AreEqual(color.g, retrieved.Value.g);
            Assert.AreEqual(color.b, retrieved.Value.b);
        }

        [Test]
        public void SerializeToPixelData_ClearsExistingOutputData()
        {
            _outputData.SetPixel(0, 0, new Color32(100, 100, 100, 255));
            Assert.AreEqual(1, _outputData.PixelCount);

            _canvas.SerializeToPixelData();

            Assert.AreEqual(0, _outputData.PixelCount);
        }

        [Test]
        public void SerializeToPixelData_AfterFill_WritesAllFilledCells()
        {
            _canvas.SetActiveTool(CanvasTool.Fill);
            _canvas.SetActiveColor(0);
            _canvas.ApplyToolAt(0, 0);

            _canvas.SerializeToPixelData();

            Assert.AreEqual(64, _outputData.PixelCount);
        }
    }
}
