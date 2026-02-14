// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HanabiCanvas.Tests.PlayMode
{
    public class PaletteUITests
    {
        // ---- Private Fields ----
        private GameObject _canvasObject;
        private GameObject _paletteObject;
        private GameObject _buttonPrefab;
        private PaletteUI _paletteUI;

        // ---- SOs ----
        private ColorPaletteSO _palette;
        private ColorVariableSO _currentColor;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _palette = ScriptableObject.CreateInstance<ColorPaletteSO>();
            _currentColor = ScriptableObject.CreateInstance<ColorVariableSO>();

            // Create a Canvas parent (required for UI components)
            _canvasObject = new GameObject("Canvas");
            _canvasObject.AddComponent<Canvas>();
            _canvasObject.AddComponent<CanvasScaler>();
            _canvasObject.AddComponent<GraphicRaycaster>();

            // Create button prefab template (not a real prefab, just a GO used as template)
            _buttonPrefab = new GameObject("ButtonPrefab");
            _buttonPrefab.AddComponent<RectTransform>();
            _buttonPrefab.AddComponent<Image>();
            _buttonPrefab.AddComponent<Button>();
            _buttonPrefab.SetActive(false);

            // Create PaletteUI (inactive -> AddComponent -> wire -> activate)
            _paletteObject = new GameObject("PaletteUI");
            _paletteObject.SetActive(false);
            _paletteObject.transform.SetParent(_canvasObject.transform);

            RectTransform container = _paletteObject.AddComponent<RectTransform>();
            _paletteUI = _paletteObject.AddComponent<PaletteUI>();

#if UNITY_EDITOR
            SerializedObject so = new SerializedObject(_paletteUI);
            so.FindProperty("_palette").objectReferenceValue = _palette;
            so.FindProperty("_currentColor").objectReferenceValue = _currentColor;
            so.FindProperty("_colorButtonContainer").objectReferenceValue = container;
            so.FindProperty("_colorButtonPrefab").objectReferenceValue = _buttonPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif

            _paletteObject.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            if (_paletteObject != null)
            {
                Object.DestroyImmediate(_paletteObject);
            }

            if (_canvasObject != null)
            {
                Object.DestroyImmediate(_canvasObject);
            }

            if (_buttonPrefab != null)
            {
                Object.DestroyImmediate(_buttonPrefab);
            }

            Object.DestroyImmediate(_palette);
            Object.DestroyImmediate(_currentColor);
        }

        // ---- Tests ----

        [UnityTest]
        public IEnumerator Start_WithPalette_CreatesCorrectButtonCount()
        {
            // Wait one frame for Start() to execute
            yield return null;

            // Container is the PaletteUI's own RectTransform
            int childCount = _paletteObject.transform.childCount;
            Assert.AreEqual(_palette.Count, childCount,
                $"Expected {_palette.Count} buttons, but found {childCount}.");
        }

        [UnityTest]
        public IEnumerator ClickButton_SetsCurrentColor()
        {
            // Wait one frame for Start() to execute
            yield return null;

            // Click the third button (index 2)
            int targetIndex = 2;
            Transform buttonTransform = _paletteObject.transform.GetChild(targetIndex);
            Button button = buttonTransform.GetComponent<Button>();
            button.onClick.Invoke();

            Color expectedColor = _palette[targetIndex];
            Assert.AreEqual(expectedColor, _currentColor.Value,
                $"Expected color {expectedColor} but got {_currentColor.Value}.");
        }
    }
}
