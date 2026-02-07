// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class PixelDataSOTests
    {
        // ---- Private Fields ----
        private PixelDataSO _pixelData;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _pixelData = ScriptableObject.CreateInstance<PixelDataSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_pixelData);
        }

        // ---- Tests ----
        [Test]
        public void SetPixel_ValidCoordinate_IncrementsCount()
        {
            _pixelData.SetPixel(0, 0, new Color32(255, 255, 255, 255));

            Assert.AreEqual(1, _pixelData.PixelCount);
        }

        [Test]
        public void SetPixel_SameCoordinateTwice_DoesNotDuplicateCount()
        {
            _pixelData.SetPixel(0, 0, new Color32(255, 0, 0, 255));
            _pixelData.SetPixel(0, 0, new Color32(0, 255, 0, 255));

            Assert.AreEqual(1, _pixelData.PixelCount);
        }

        [Test]
        public void SetPixel_SameCoordinateTwice_UpdatesColor()
        {
            _pixelData.SetPixel(0, 0, new Color32(255, 0, 0, 255));
            _pixelData.SetPixel(0, 0, new Color32(0, 0, 255, 255));

            Color32? result = _pixelData.GetPixel(0, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual((byte)0, result.Value.r);
            Assert.AreEqual((byte)0, result.Value.g);
            Assert.AreEqual((byte)255, result.Value.b);
        }

        [Test]
        public void SetPixel_OutOfBounds_DoesNotAdd()
        {
            LogAssert.Expect(LogType.Warning,
                new System.Text.RegularExpressions.Regex(
                    ".*out of bounds.*",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase));

            _pixelData.SetPixel(255, 0, new Color32(255, 255, 255, 255));

            Assert.AreEqual(0, _pixelData.PixelCount);
        }

        [Test]
        public void GetPixel_FilledCell_ReturnsColor()
        {
            Color32 expected = new Color32(128, 64, 32, 255);
            _pixelData.SetPixel(5, 5, expected);

            Color32? result = _pixelData.GetPixel(5, 5);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected.r, result.Value.r);
            Assert.AreEqual(expected.g, result.Value.g);
            Assert.AreEqual(expected.b, result.Value.b);
        }

        [Test]
        public void GetPixel_EmptyCell_ReturnsNull()
        {
            Color32? result = _pixelData.GetPixel(0, 0);

            Assert.IsNull(result);
        }

        [Test]
        public void RemovePixel_ExistingPixel_DecrementsCount()
        {
            _pixelData.SetPixel(1, 1, new Color32(255, 0, 0, 255));
            _pixelData.RemovePixel(1, 1);

            Assert.AreEqual(0, _pixelData.PixelCount);
        }

        [Test]
        public void RemovePixel_NonexistentPixel_DoesNotChangeCount()
        {
            _pixelData.RemovePixel(0, 0);

            Assert.AreEqual(0, _pixelData.PixelCount);
        }

        [Test]
        public void Clear_AfterAddingPixels_ResetsCount()
        {
            _pixelData.SetPixel(0, 0, new Color32(255, 0, 0, 255));
            _pixelData.SetPixel(1, 1, new Color32(0, 255, 0, 255));
            _pixelData.SetPixel(2, 2, new Color32(0, 0, 255, 255));

            _pixelData.Clear();

            Assert.AreEqual(0, _pixelData.PixelCount);
        }

        [Test]
        public void ToJson_WithPixels_ReturnsNonEmptyString()
        {
            _pixelData.SetPixel(0, 0, new Color32(255, 0, 0, 255));
            _pixelData.SetPixel(1, 1, new Color32(0, 255, 0, 255));

            string json = _pixelData.ToJson();

            Assert.IsFalse(string.IsNullOrEmpty(json));
        }

        [Test]
        public void FromJson_ValidJson_PopulatesData()
        {
            _pixelData.SetPixel(0, 0, new Color32(255, 0, 0, 255));
            _pixelData.SetPixel(3, 7, new Color32(0, 255, 0, 255));
            _pixelData.SetPixel(10, 10, new Color32(0, 0, 255, 255));

            string json = _pixelData.ToJson();
            int originalWidth = _pixelData.Width;
            int originalHeight = _pixelData.Height;
            int originalCount = _pixelData.PixelCount;

            PixelDataSO other = ScriptableObject.CreateInstance<PixelDataSO>();
            other.FromJson(json);

            Assert.AreEqual(originalWidth, other.Width);
            Assert.AreEqual(originalHeight, other.Height);
            Assert.AreEqual(originalCount, other.PixelCount);

            Object.DestroyImmediate(other);
        }

        [Test]
        public void OnValidate_WidthBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_pixelData);
            so.FindProperty("_width").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_pixelData.Width, 1);
        }

        [Test]
        public void OnValidate_HeightAboveMax_ClampsToMax()
        {
            SerializedObject so = new SerializedObject(_pixelData);
            so.FindProperty("_height").intValue = 300;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.LessOrEqual(_pixelData.Height, 255);
        }
    }
}
