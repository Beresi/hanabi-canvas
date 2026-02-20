// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Modes;

namespace HanabiCanvas.Tests.EditMode
{
    public class ChallengeModeControllerTests
    {
        // ---- Private Fields ----
        private GameObject _controllerGO;
        private ChallengeModeController _controller;
        private BoolVariableSO _isCanvasInputEnabled;
        private BoolVariableSO _isSymmetryEnabled;
        private FloatVariableSO _remainingTime;
        private IntVariableSO _uniqueColorCount;
        private IntVariableSO _filledPixelCount;
        private GameEventSO _onFireworkComplete;
        private GameEventSO _onPixelPainted;
        private GameEventSO _onConstraintViolated;
        private GameEventSO _onCanvasCleared;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _controllerGO = new GameObject("TestChallengeModeController");
            _controller = _controllerGO.AddComponent<ChallengeModeController>();
            _isCanvasInputEnabled = ScriptableObject.CreateInstance<BoolVariableSO>();
            _isSymmetryEnabled = ScriptableObject.CreateInstance<BoolVariableSO>();
            _remainingTime = ScriptableObject.CreateInstance<FloatVariableSO>();
            _uniqueColorCount = ScriptableObject.CreateInstance<IntVariableSO>();
            _filledPixelCount = ScriptableObject.CreateInstance<IntVariableSO>();
            _onFireworkComplete = ScriptableObject.CreateInstance<GameEventSO>();
            _onPixelPainted = ScriptableObject.CreateInstance<GameEventSO>();
            _onConstraintViolated = ScriptableObject.CreateInstance<GameEventSO>();
            _onCanvasCleared = ScriptableObject.CreateInstance<GameEventSO>();

            _controller.Initialize(
                null, null, null, null, null,
                _isCanvasInputEnabled, _isSymmetryEnabled,
                _remainingTime, _uniqueColorCount, _filledPixelCount,
                _onFireworkComplete, _onPixelPainted,
                _onConstraintViolated, _onCanvasCleared);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_controllerGO);
            Object.DestroyImmediate(_isCanvasInputEnabled);
            Object.DestroyImmediate(_isSymmetryEnabled);
            Object.DestroyImmediate(_remainingTime);
            Object.DestroyImmediate(_uniqueColorCount);
            Object.DestroyImmediate(_filledPixelCount);
            Object.DestroyImmediate(_onFireworkComplete);
            Object.DestroyImmediate(_onPixelPainted);
            Object.DestroyImmediate(_onConstraintViolated);
            Object.DestroyImmediate(_onCanvasCleared);
        }

        // ---- Tests ----

        [Test]
        public void Activate_WithColorLimit_SetsConstraint()
        {
            ConstraintData[] constraints = new ConstraintData[]
            {
                new ConstraintData(ConstraintType.ColorLimit, 3)
            };
            RequestData request = new RequestData("test-id", "Draw something", constraints);

            _controller.Activate(request);

            Assert.IsTrue(_controller.IsActive);
            Assert.IsTrue(_isCanvasInputEnabled.Value);
        }

        [Test]
        public void CheckConstraints_ColorLimitExceeded_RaisesViolation()
        {
            ConstraintData[] constraints = new ConstraintData[]
            {
                new ConstraintData(ConstraintType.ColorLimit, 2)
            };
            RequestData request = new RequestData("test-id", "Draw something", constraints);

            _controller.Activate(request);
            _uniqueColorCount.Value = 3;

            bool wasViolated = !_controller.ValidateConstraints();

            Assert.IsTrue(wasViolated);
        }

        [Test]
        public void CheckConstraints_WithinLimits_NoViolation()
        {
            ConstraintData[] constraints = new ConstraintData[]
            {
                new ConstraintData(ConstraintType.ColorLimit, 4)
            };
            RequestData request = new RequestData("test-id", "Draw something", constraints);

            _controller.Activate(request);
            _uniqueColorCount.Value = 2;

            bool isValid = _controller.ValidateConstraints();

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Activate_WithSymmetryRequired_EnablesSymmetry()
        {
            ConstraintData[] constraints = new ConstraintData[]
            {
                new ConstraintData(ConstraintType.SymmetryRequired, 0, 0f, true)
            };
            RequestData request = new RequestData("test-id", "Symmetric art", constraints);

            _controller.Activate(request);

            Assert.IsTrue(_isSymmetryEnabled.Value);
        }

        [Test]
        public void Activate_WithTimeLimit_SetsRemainingTime()
        {
            ConstraintData[] constraints = new ConstraintData[]
            {
                new ConstraintData(ConstraintType.TimeLimit, 0, 30f)
            };
            RequestData request = new RequestData("test-id", "Quick draw", constraints);

            _controller.Activate(request);

            Assert.AreEqual(30f, _remainingTime.Value);
        }

        [Test]
        public void Deactivate_ClearsConstraintState()
        {
            ConstraintData[] constraints = new ConstraintData[]
            {
                new ConstraintData(ConstraintType.SymmetryRequired, 0, 0f, true)
            };
            RequestData request = new RequestData("test-id", "Test", constraints);

            _controller.Activate(request);
            _controller.Deactivate();

            Assert.IsFalse(_controller.IsActive);
            Assert.IsFalse(_isSymmetryEnabled.Value);
            Assert.IsFalse(_isCanvasInputEnabled.Value);
        }
    }
}
