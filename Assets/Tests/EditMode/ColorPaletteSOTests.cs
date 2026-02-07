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
        public void Colors_DefaultInstance_HasEightElements()
        {
            Assert.AreEqual(8, _palette.Colors.Length);
        }

        [Test]
        public void OnValidate_ColorsArrayTooShort_ResizesToEight()
        {
            SerializedObject so = new SerializedObject(_palette);
            SerializedProperty colorsProp = so.FindProperty("_colors");

            colorsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                colorsProp.GetArrayElementAtIndex(i).colorValue = Color.red;
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(8, _palette.Colors.Length);
        }

        [Test]
        public void OnValidate_ColorsArrayTooLong_ResizesToEight()
        {
            SerializedObject so = new SerializedObject(_palette);
            SerializedProperty colorsProp = so.FindProperty("_colors");

            colorsProp.arraySize = 12;
            for (int i = 0; i < 12; i++)
            {
                colorsProp.GetArrayElementAtIndex(i).colorValue = new Color(
                    i / 12f, (12 - i) / 12f, 0.5f, 1f);
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(8, _palette.Colors.Length);
        }

        [Test]
        public void OnValidate_ColorWithZeroAlpha_SetsAlphaTo255()
        {
            SerializedObject so = new SerializedObject(_palette);
            SerializedProperty colorsProp = so.FindProperty("_colors");

            SerializedProperty element = colorsProp.GetArrayElementAtIndex(0);
            element.colorValue = new Color(1f, 0f, 0f, 0f);
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(255, _palette.Colors[0].a);
        }

        [Test]
        public void BackgroundColor_Default_HasFullAlpha()
        {
            Assert.AreEqual(255, _palette.BackgroundColor.a);
        }

        [Test]
        public void PaletteName_Default_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_palette.PaletteName));
        }
    }
}
