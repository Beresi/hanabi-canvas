// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class CanvasConfigSOTests
    {
        // ---- Private Fields ----
        private CanvasConfigSO _canvasConfig;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _canvasConfig = ScriptableObject.CreateInstance<CanvasConfigSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_canvasConfig);
        }

        // ---- Tests ----
        [Test]
        public void GridWidth_Default_Is32()
        {
            Assert.AreEqual(32, _canvasConfig.GridWidth);
        }

        [Test]
        public void GridHeight_Default_Is32()
        {
            Assert.AreEqual(32, _canvasConfig.GridHeight);
        }

        [Test]
        public void CellSize_Default_IsPositive()
        {
            Assert.Greater(_canvasConfig.CellSize, 0f);
        }

        [Test]
        public void OnValidate_GridWidthBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_canvasConfig);
            so.FindProperty("_gridWidth").intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(8, _canvasConfig.GridWidth);
        }

        [Test]
        public void OnValidate_GridWidthAboveMax_ClampsToMax()
        {
            SerializedObject so = new SerializedObject(_canvasConfig);
            so.FindProperty("_gridWidth").intValue = 100;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(64, _canvasConfig.GridWidth);
        }

        [Test]
        public void OnValidate_GridHeightBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_canvasConfig);
            so.FindProperty("_gridHeight").intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(8, _canvasConfig.GridHeight);
        }

        [Test]
        public void OnValidate_CellSizeBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_canvasConfig);
            so.FindProperty("_cellSize").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_canvasConfig.CellSize, 0.01f);
        }
    }
}
