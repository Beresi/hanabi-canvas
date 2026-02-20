// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.GameFlow;

namespace HanabiCanvas.Tests.PlayMode
{
    public class GameFlowIntegrationTests
    {
        // ---- Private Fields ----
        private GameObject _controllerGO;
        private GameFlowController _controller;
        private AppStateVariableSO _appState;
        private BoolVariableSO _isChallengeMode;
        private GameEventSO _onSlideshowComplete;
        private GameEventSO _onSlideshowExitRequested;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _controllerGO = new GameObject("TestGameFlowController");
            _controllerGO.SetActive(false);
            _controller = _controllerGO.AddComponent<GameFlowController>();

            _appState = ScriptableObject.CreateInstance<AppStateVariableSO>();
            _isChallengeMode = ScriptableObject.CreateInstance<BoolVariableSO>();
            _onSlideshowComplete = ScriptableObject.CreateInstance<GameEventSO>();
            _onSlideshowExitRequested = ScriptableObject.CreateInstance<GameEventSO>();

            _controller.Initialize(
                _appState, null, null, null,
                _isChallengeMode,
                _onSlideshowComplete,
                _onSlideshowExitRequested);

            _controllerGO.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_controllerGO);
            Object.DestroyImmediate(_appState);
            Object.DestroyImmediate(_isChallengeMode);
            Object.DestroyImmediate(_onSlideshowComplete);
            Object.DestroyImmediate(_onSlideshowExitRequested);
        }

        // ---- Tests ----

        [UnityTest]
        public IEnumerator MenuToPlaying_TransitionsCorrectly()
        {
            yield return null;

            _controller.SetAppState(AppState.Playing);

            Assert.AreEqual(AppState.Playing, _appState.Value);
        }

        [UnityTest]
        public IEnumerator PlayingToMenu_TransitionsCorrectly()
        {
            yield return null;

            _controller.SetAppState(AppState.Playing);
            _controller.SetAppState(AppState.Menu);

            Assert.AreEqual(AppState.Menu, _appState.Value);
        }

        [UnityTest]
        public IEnumerator SettingsOverlay_RestoresPreviousState()
        {
            yield return null;

            _controller.SetAppState(AppState.Playing);
            _controller.SetAppState(AppState.Settings);

            Assert.AreEqual(AppState.Settings, _appState.Value);

            _controller.RestorePreviousState();

            Assert.AreEqual(AppState.Playing, _appState.Value);
        }
    }
}
