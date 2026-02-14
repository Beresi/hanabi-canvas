// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.Text.RegularExpressions;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class ColorPaletteSOTests
    {
        // ---- Private Fields ----
        private ColorPaletteSO _palette;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _palette = ScriptableObject.CreateInstance<ColorPaletteSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_palette);
        }

        // ---- Tests ----
        [Test]
        public void Indexer_ValidIndex_ReturnsColor()
        {
            Color color = _palette[0];

            Assert.AreNotEqual(Color.clear, color);
        }

        [Test]
        public void Indexer_OutOfBounds_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex(".*"));

            Color color = _palette[999];

            Assert.AreEqual(Color.white, color);
        }

        [Test]
        public void Indexer_NegativeIndex_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex(".*"));

            Color color = _palette[-1];

            Assert.AreEqual(Color.white, color);
        }

        [Test]
        public void Count_DefaultPalette_ReturnsColorCount()
        {
            Assert.AreEqual(8, _palette.Count);
        }

        [Test]
        public void OnValidate_ForcesAlphaToOne()
        {
            SerializedObject so = new SerializedObject(_palette);
            SerializedProperty colorsProperty = so.FindProperty("_colors");

            SerializedProperty firstColor = colorsProperty.GetArrayElementAtIndex(0);
            firstColor.colorValue = new Color(1f, 0f, 0f, 0.5f);
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(1f, _palette[0].a);
        }
    }
}
