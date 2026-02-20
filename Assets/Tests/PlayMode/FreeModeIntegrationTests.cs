// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Modes;

namespace HanabiCanvas.Tests.PlayMode
{
    public class FreeModeIntegrationTests
    {
        // ---- Private Fields ----
        private GameObject _controllerGO;
        private FreeModeController _controller;
        private BoolVariableSO _isCanvasInputEnabled;
        private GameEventSO _onFireworkComplete;
        private GameEventSO _onCanvasCleared;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _controllerGO = new GameObject("TestFreeModeController");
            _controllerGO.SetActive(false);
            _controller = _controllerGO.AddComponent<FreeModeController>();

            _isCanvasInputEnabled = ScriptableObject.CreateInstance<BoolVariableSO>();
            _onFireworkComplete = ScriptableObject.CreateInstance<GameEventSO>();
            _onCanvasCleared = ScriptableObject.CreateInstance<GameEventSO>();

            _controller.Initialize(
                null, null, null, null,
                _isCanvasInputEnabled,
                _onFireworkComplete,
                _onCanvasCleared);

            _controllerGO.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_controllerGO);
            Object.DestroyImmediate(_isCanvasInputEnabled);
            Object.DestroyImmediate(_onFireworkComplete);
            Object.DestroyImmediate(_onCanvasCleared);
        }

        // ---- Tests ----

        [UnityTest]
        public IEnumerator Activate_EnablesCanvasInput()
        {
            yield return null;

            _controller.Activate();

            Assert.IsTrue(_isCanvasInputEnabled.Value);
            Assert.IsTrue(_controller.IsActive);
        }

        [UnityTest]
        public IEnumerator Deactivate_DisablesCanvasInput()
        {
            yield return null;

            _controller.Activate();
            _controller.Deactivate();

            Assert.IsFalse(_isCanvasInputEnabled.Value);
            Assert.IsFalse(_controller.IsActive);
        }
    }
}
