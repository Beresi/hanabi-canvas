// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.GameFlow;

namespace HanabiCanvas.Tests.EditMode
{
    public class GameFlowControllerTests
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

        [Test]
        public void SetAppState_ToPlaying_UpdatesVariable()
        {
            _controller.SetAppState(AppState.Playing);

            Assert.AreEqual(AppState.Playing, _appState.Value);
        }

        [Test]
        public void SetAppState_ToSlideshow_UpdatesVariable()
        {
            _controller.SetAppState(AppState.Slideshow);

            Assert.AreEqual(AppState.Slideshow, _appState.Value);
        }

        [Test]
        public void SetAppState_ToSettings_UpdatesVariable()
        {
            _controller.SetAppState(AppState.Settings);

            Assert.AreEqual(AppState.Settings, _appState.Value);
        }

        [Test]
        public void SetAppState_FromSettings_RestoresMenu()
        {
            _appState.Value = AppState.Menu;
            _controller.SetAppState(AppState.Settings);
            _controller.RestorePreviousState();

            Assert.AreEqual(AppState.Menu, _appState.Value);
        }

        [Test]
        public void SetPlayingMode_Challenge_SetsVariable()
        {
            _controller.SetPlayingMode(true);

            Assert.IsTrue(_isChallengeMode.Value);
        }

        [Test]
        public void SetPlayingMode_Free_SetsVariable()
        {
            _controller.SetPlayingMode(false);

            Assert.IsFalse(_isChallengeMode.Value);
        }

        [Test]
        public void SetAppState_ToMenu_FromPlaying_UpdatesVariable()
        {
            _controller.SetAppState(AppState.Playing);
            _controller.SetAppState(AppState.Menu);

            Assert.AreEqual(AppState.Menu, _appState.Value);
        }
    }
}
