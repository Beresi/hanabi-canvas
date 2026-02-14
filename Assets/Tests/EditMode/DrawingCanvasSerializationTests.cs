// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class DrawingCanvasSerializationTests
    {
        // ---- Private Fields ----
        private DrawingCanvasConfigSO _config;
        private GameObject _canvasGo;
        private DrawingCanvas _canvas;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<DrawingCanvasConfigSO>();
            _canvasGo = new GameObject("TestCanvas");
            _canvas = _canvasGo.AddComponent<DrawingCanvas>();

            var so = new SerializedObject(_canvas);
            so.FindProperty("_config").objectReferenceValue = _config;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_canvasGo);
            Object.DestroyImmediate(_config);
        }

        // ---- Helpers ----
        private void SetTexture(Texture2D texture)
        {
            FieldInfo field = typeof(DrawingCanvas).GetField(
                "_texture", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_canvas, texture);
        }

        private Texture2D CreateClearTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        // ---- Tests ----
        [Test]
        public void GetFireworkPattern_EmptyTexture_ReturnsZeroPixels()
        {
            int gridSize = _config.GridSize;
            Texture2D tex = CreateClearTexture(gridSize);
            SetTexture(tex);

            FireworkPattern pattern = _canvas.GetFireworkPattern();

            Assert.AreEqual(0, pattern.Pixels.Length);
            Assert.AreEqual(gridSize, pattern.Width);
            Assert.AreEqual(gridSize, pattern.Height);

            Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetFireworkPattern_WithPaintedPixels_ReturnsCorrectCount()
        {
            int gridSize = _config.GridSize;
            Texture2D tex = CreateClearTexture(gridSize);
            tex.SetPixel(0, 0, Color.red);
            tex.SetPixel(5, 10, Color.blue);
            tex.SetPixel(31, 31, Color.green);
            tex.Apply();
            SetTexture(tex);

            FireworkPattern pattern = _canvas.GetFireworkPattern();

            Assert.AreEqual(3, pattern.Pixels.Length);

            Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetFireworkPattern_WithPaintedPixels_ReturnsCorrectCoordinates()
        {
            int gridSize = _config.GridSize;
            Texture2D tex = CreateClearTexture(gridSize);
            tex.SetPixel(7, 3, Color.red);
            tex.Apply();
            SetTexture(tex);

            FireworkPattern pattern = _canvas.GetFireworkPattern();

            Assert.AreEqual(1, pattern.Pixels.Length);
            Assert.AreEqual(7, pattern.Pixels[0].X);
            Assert.AreEqual(3, pattern.Pixels[0].Y);

            Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetFireworkPattern_WithPaintedPixels_ReturnsCorrectColors()
        {
            int gridSize = _config.GridSize;
            Texture2D tex = CreateClearTexture(gridSize);
            tex.SetPixel(0, 0, Color.red);
            tex.Apply();
            SetTexture(tex);

            FireworkPattern pattern = _canvas.GetFireworkPattern();

            Assert.AreEqual(1, pattern.Pixels.Length);
            Assert.AreEqual((byte)255, pattern.Pixels[0].Color.r);
            Assert.AreEqual((byte)0, pattern.Pixels[0].Color.g);
            Assert.AreEqual((byte)0, pattern.Pixels[0].Color.b);
            Assert.AreEqual((byte)255, pattern.Pixels[0].Color.a);

            Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetFireworkPattern_IgnoresTransparentPixels()
        {
            int gridSize = _config.GridSize;
            Texture2D tex = CreateClearTexture(gridSize);
            tex.SetPixel(0, 0, Color.red);
            tex.SetPixel(1, 1, new Color(0f, 0f, 0f, 0f));
            tex.Apply();
            SetTexture(tex);

            FireworkPattern pattern = _canvas.GetFireworkPattern();

            Assert.AreEqual(1, pattern.Pixels.Length);

            Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetFireworkPattern_DimensionsMatchConfig()
        {
            int gridSize = _config.GridSize;
            Texture2D tex = CreateClearTexture(gridSize);
            SetTexture(tex);

            FireworkPattern pattern = _canvas.GetFireworkPattern();

            Assert.AreEqual(gridSize, pattern.Width);
            Assert.AreEqual(gridSize, pattern.Height);

            Object.DestroyImmediate(tex);
        }
    }
}
