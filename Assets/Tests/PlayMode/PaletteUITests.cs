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

            // Create button prefab template matching ColorButton hierarchy
            _buttonPrefab = new GameObject("ButtonPrefab");
            _buttonPrefab.AddComponent<RectTransform>();
            ColorButton colorButton = _buttonPrefab.AddComponent<ColorButton>();

            // Frame child with Image
            GameObject frameChild = new GameObject("Frame");
            frameChild.transform.SetParent(_buttonPrefab.transform, false);
            frameChild.AddComponent<RectTransform>();
            Image frameImage = frameChild.AddComponent<Image>();

            // ColorFill child with Image + Button
            GameObject colorFillChild = new GameObject("ColorFill");
            colorFillChild.transform.SetParent(_buttonPrefab.transform, false);
            colorFillChild.AddComponent<RectTransform>();
            Image colorFillImage = colorFillChild.AddComponent<Image>();
            Button button = colorFillChild.AddComponent<Button>();

            // Wire ColorButton serialized fields
#if UNITY_EDITOR
            SerializedObject colorButtonSO = new SerializedObject(colorButton);
            colorButtonSO.FindProperty("_frameImage").objectReferenceValue = frameImage;
            colorButtonSO.FindProperty("_colorFillImage").objectReferenceValue = colorFillImage;
            colorButtonSO.FindProperty("_button").objectReferenceValue = button;
            colorButtonSO.ApplyModifiedPropertiesWithoutUndo();
#endif

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
            yield return null;

            int childCount = _paletteObject.transform.childCount;
            Assert.AreEqual(_palette.Count, childCount,
                $"Expected {_palette.Count} buttons, but found {childCount}.");
        }

        [UnityTest]
        public IEnumerator Start_FirstButtonSelectedByDefault_SetsFirstColor()
        {
            yield return null;

            Color expectedColor = _palette[0];
            Assert.AreEqual(expectedColor, _currentColor.Value,
                $"Expected first color {expectedColor} but got {_currentColor.Value}.");
        }

        [UnityTest]
        public IEnumerator ClickButton_SetsCurrentColor()
        {
            yield return null;

            int targetIndex = 2;
            Transform buttonTransform = _paletteObject.transform.GetChild(targetIndex);
            ColorButton colorButton = buttonTransform.GetComponent<ColorButton>();
            Button button = buttonTransform.GetComponentInChildren<Button>();
            button.onClick.Invoke();

            Color expectedColor = _palette[targetIndex];
            Assert.AreEqual(expectedColor, _currentColor.Value,
                $"Expected color {expectedColor} but got {_currentColor.Value}.");
        }

        [UnityTest]
        public IEnumerator ClickSecondButton_DeselectsFirst_SelectsSecond()
        {
            yield return null;

            // First button should be selected (scale 1.1)
            Transform firstButton = _paletteObject.transform.GetChild(0);
            Assert.AreEqual(Vector3.one * 1.1f, firstButton.localScale,
                "First button should be selected (scale 1.1) after Start.");

            // Click second button
            Transform secondButton = _paletteObject.transform.GetChild(1);
            Button button = secondButton.GetComponentInChildren<Button>();
            button.onClick.Invoke();

            // First should be deselected (scale 1.0)
            Assert.AreEqual(Vector3.one, firstButton.localScale,
                "First button should be deselected (scale 1.0) after clicking second.");

            // Second should be selected (scale 1.1)
            Assert.AreEqual(Vector3.one * 1.1f, secondButton.localScale,
                "Second button should be selected (scale 1.1) after clicking it.");
        }
    }
}
