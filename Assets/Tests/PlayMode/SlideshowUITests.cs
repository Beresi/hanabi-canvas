// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.GameFlow;
using HanabiCanvas.Runtime.UI;

namespace HanabiCanvas.Tests.PlayMode
{
    public class SlideshowUITests
    {
        // ---- Private Fields ----
        private GameObject _uiGO;
        private SlideshowUI _slideshowUI;
        private AppStateVariableSO _appState;
        private GameEventSO _onSlideshowExitRequested;
        private Button _exitButton;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _appState = ScriptableObject.CreateInstance<AppStateVariableSO>();
            _onSlideshowExitRequested = ScriptableObject.CreateInstance<GameEventSO>();

            _uiGO = new GameObject("TestSlideshowUI");
            _uiGO.SetActive(false);

            GameObject exitGO = new GameObject("ExitButton");
            exitGO.transform.SetParent(_uiGO.transform);
            _exitButton = exitGO.AddComponent<Button>();

            _slideshowUI = _uiGO.AddComponent<SlideshowUI>();

#if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(_slideshowUI);
            so.FindProperty("_appState").objectReferenceValue = _appState;
            so.FindProperty("_onSlideshowExitRequested").objectReferenceValue = _onSlideshowExitRequested;
            so.FindProperty("_exitButton").objectReferenceValue = _exitButton;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif

            _appState.Value = AppState.Slideshow;
            _uiGO.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_uiGO);
            Object.DestroyImmediate(_appState);
            Object.DestroyImmediate(_onSlideshowExitRequested);
        }

        // ---- Tests ----

        [UnityTest]
        public IEnumerator AppStateSlideshow_UIIsVisible()
        {
            yield return null;

            Assert.IsTrue(_uiGO.activeSelf);
        }

        [UnityTest]
        public IEnumerator ExitButton_RaisesExitEvent()
        {
            yield return null;

            bool wasRaised = false;
            _onSlideshowExitRequested.Register(() => wasRaised = true);

            _exitButton.onClick.Invoke();

            Assert.IsTrue(wasRaised);
        }
    }
}
