// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HanabiCanvas.Tests.PlayMode
{
    public class PatternGalleryUITests
    {
        // ---- Private Fields ----
        private GameObject _canvasObject;
        private GameObject _galleryObject;
        private GameObject _thumbnailPrefab;
        private GameObject _thumbnailContainerObj;
        private PatternGalleryUI _galleryUI;
        private RectTransform _thumbnailContainer;

        // ---- SOs ----
        private PatternListSO _patternLibrary;
        private IntVariableSO _selectedPatternIndex;
        private GameEventSO _onLaunchPattern;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _patternLibrary = ScriptableObject.CreateInstance<PatternListSO>();
            _selectedPatternIndex = ScriptableObject.CreateInstance<IntVariableSO>();
            _onLaunchPattern = ScriptableObject.CreateInstance<GameEventSO>();

            _canvasObject = new GameObject("Canvas");
            _canvasObject.AddComponent<Canvas>();
            _canvasObject.AddComponent<CanvasScaler>();
            _canvasObject.AddComponent<GraphicRaycaster>();

            _thumbnailPrefab = new GameObject("ThumbnailPrefab");
            _thumbnailPrefab.AddComponent<RectTransform>();
            _thumbnailPrefab.AddComponent<RawImage>();
            _thumbnailPrefab.AddComponent<Button>();
            _thumbnailPrefab.SetActive(false);

            _galleryObject = new GameObject("PatternGalleryUI");
            _galleryObject.SetActive(false);
            _galleryObject.transform.SetParent(_canvasObject.transform);
            _galleryObject.AddComponent<RectTransform>();

            _thumbnailContainerObj = new GameObject("ThumbnailContainer");
            _thumbnailContainerObj.AddComponent<RectTransform>();
            _thumbnailContainerObj.transform.SetParent(_galleryObject.transform);
            _thumbnailContainer = _thumbnailContainerObj.GetComponent<RectTransform>();

            _galleryUI = _galleryObject.AddComponent<PatternGalleryUI>();

#if UNITY_EDITOR
            SerializedObject so = new SerializedObject(_galleryUI);
            so.FindProperty("_patternLibrary").objectReferenceValue = _patternLibrary;
            so.FindProperty("_selectedPatternIndex").objectReferenceValue = _selectedPatternIndex;
            so.FindProperty("_onLaunchPattern").objectReferenceValue = _onLaunchPattern;
            so.FindProperty("_thumbnailContainer").objectReferenceValue = _thumbnailContainer;
            so.FindProperty("_thumbnailPrefab").objectReferenceValue = _thumbnailPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif

            _galleryObject.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            if (_galleryObject != null)
            {
                Object.DestroyImmediate(_galleryObject);
            }

            if (_canvasObject != null)
            {
                Object.DestroyImmediate(_canvasObject);
            }

            if (_thumbnailPrefab != null)
            {
                Object.DestroyImmediate(_thumbnailPrefab);
            }

            Object.DestroyImmediate(_patternLibrary);
            Object.DestroyImmediate(_selectedPatternIndex);
            Object.DestroyImmediate(_onLaunchPattern);
        }

        // ---- Tests ----

        [UnityTest]
        public IEnumerator OnItemAdded_CreatesNewThumbnail()
        {
            yield return null;

            _patternLibrary.Add(CreateTestPattern());

            Assert.AreEqual(1, _thumbnailContainer.childCount,
                "Expected 1 thumbnail after adding a pattern.");
        }

        [UnityTest]
        public IEnumerator ClickThumbnail_SetsIndexAndRaisesEvent()
        {
            yield return null;

            _patternLibrary.Add(CreateTestPattern());
            _patternLibrary.Add(CreateTestPattern());
            _patternLibrary.Add(CreateTestPattern());

            bool isEventRaised = false;
            System.Action listener = () => isEventRaised = true;
            _onLaunchPattern.Register(listener);

            Transform secondThumbnail = _thumbnailContainer.GetChild(1);
            Button button = secondThumbnail.GetComponent<Button>();
            button.onClick.Invoke();

            Assert.AreEqual(1, _selectedPatternIndex.Value,
                "Expected selected pattern index to be 1 after clicking second thumbnail.");
            Assert.IsTrue(isEventRaised,
                "Expected OnLaunchPattern event to be raised after clicking thumbnail.");

            _onLaunchPattern.Unregister(listener);
        }

        // ---- Helpers ----

        private FireworkPattern CreateTestPattern()
        {
            return new FireworkPattern
            {
                Pixels = new PixelEntry[]
                {
                    new PixelEntry(0, 0, new Color32(255, 0, 0, 255)),
                },
                Width = 32,
                Height = 32
            };
        }
    }
}
