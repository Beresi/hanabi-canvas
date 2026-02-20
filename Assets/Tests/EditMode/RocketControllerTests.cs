// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Tests.EditMode
{
    public class RocketControllerTests
    {
        // ---- Private Fields ----
        private GameObject _controllerObject;
        private RocketController _controller;
        private RocketConfigSO _rocketConfig;
        private StraightRocketPathSO _straightPath;
        private FireworkRequestEventSO _onRocketLaunchRequested;
        private FireworkRequestEventSO _onFireworkRequested;
        private BoolVariableSO _isRocketAscending;

        // ---- Setup / Teardown ----

        [SetUp]
        public void SetUp()
        {
            _controllerObject = new GameObject("RocketController");
            _controllerObject.AddComponent<MeshFilter>();
            _controllerObject.AddComponent<MeshRenderer>();
            _controller = _controllerObject.AddComponent<RocketController>();

            _rocketConfig = ScriptableObject.CreateInstance<RocketConfigSO>();
            _straightPath = ScriptableObject.CreateInstance<StraightRocketPathSO>();
            _onRocketLaunchRequested = ScriptableObject.CreateInstance<FireworkRequestEventSO>();
            _onFireworkRequested = ScriptableObject.CreateInstance<FireworkRequestEventSO>();
            _isRocketAscending = ScriptableObject.CreateInstance<BoolVariableSO>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_controllerObject != null)
            {
                Object.DestroyImmediate(_controllerObject);
            }

            Object.DestroyImmediate(_rocketConfig);
            Object.DestroyImmediate(_straightPath);
            Object.DestroyImmediate(_onRocketLaunchRequested);
            Object.DestroyImmediate(_onFireworkRequested);
            Object.DestroyImmediate(_isRocketAscending);
        }

        // ---- Helper Methods ----

        private void InitializeControllerWithConfig()
        {
            // Wire a path into the config
            SerializedObject configSO = new SerializedObject(_rocketConfig);
            SerializedProperty pathProp = configSO.FindProperty("_pathBehaviours");
            pathProp.ClearArray();
            pathProp.InsertArrayElementAtIndex(0);
            pathProp.GetArrayElementAtIndex(0).objectReferenceValue = _straightPath;
            configSO.ApplyModifiedPropertiesWithoutUndo();

            _controller.Initialize(
                _rocketConfig,
                _onRocketLaunchRequested,
                _onFireworkRequested,
                _isRocketAscending);
        }

        private void InitializeControllerWithoutConfig()
        {
            _controller.Initialize(
                null,
                _onRocketLaunchRequested,
                _onFireworkRequested,
                _isRocketAscending);
        }

        private void InitializeControllerWithoutPath()
        {
            // Config with empty path array
            SerializedObject configSO = new SerializedObject(_rocketConfig);
            SerializedProperty pathProp = configSO.FindProperty("_pathBehaviours");
            pathProp.ClearArray();
            configSO.ApplyModifiedPropertiesWithoutUndo();

            _controller.Initialize(
                _rocketConfig,
                _onRocketLaunchRequested,
                _onFireworkRequested,
                _isRocketAscending);
        }

        private FireworkRequest CreateTestRequest()
        {
            return new FireworkRequest
            {
                Position = new Vector3(0f, 5f, 0f),
                Pattern = new PixelEntry[]
                {
                    new PixelEntry(0, 0, new Color32(255, 0, 0, 255)),
                    new PixelEntry(1, 1, new Color32(0, 255, 0, 255))
                },
                PatternWidth = 32,
                PatternHeight = 32
            };
        }

        // ---- Fallback: No Config ----

        [Test]
        public void HandleRocketLaunchRequested_NoConfig_RaisesFireworkRequestImmediately()
        {
            InitializeControllerWithoutConfig();

            bool wasRaised = false;
            FireworkRequest capturedRequest = default;
            System.Action<FireworkRequest> listener = (FireworkRequest req) =>
            {
                wasRaised = true;
                capturedRequest = req;
            };
            _onFireworkRequested.Register(listener);

            FireworkRequest testRequest = CreateTestRequest();

            LogAssert.Expect(LogType.Warning, new Regex(@"No RocketConfigSO assigned"));
            _onRocketLaunchRequested.Raise(testRequest);

            Assert.IsTrue(wasRaised, "FireworkRequestEventSO should have been raised immediately.");
            Assert.AreEqual(testRequest.Position, capturedRequest.Position);
            Assert.AreEqual(testRequest.Pattern.Length, capturedRequest.Pattern.Length);
            Assert.IsFalse(_controller.IsFlying, "Rocket should not be flying without config.");

            _onFireworkRequested.Unregister(listener);
        }

        // ---- Fallback: Config But No Path ----

        [Test]
        public void HandleRocketLaunchRequested_NoPath_RaisesFireworkRequestImmediately()
        {
            InitializeControllerWithoutPath();

            bool wasRaised = false;
            System.Action<FireworkRequest> listener = (FireworkRequest req) =>
            {
                wasRaised = true;
            };
            _onFireworkRequested.Register(listener);

            LogAssert.Expect(LogType.Warning, new Regex(@"returned no path"));
            _onRocketLaunchRequested.Raise(CreateTestRequest());

            Assert.IsTrue(wasRaised, "FireworkRequestEventSO should have been raised immediately.");
            Assert.IsFalse(_controller.IsFlying, "Rocket should not be flying without path.");

            _onFireworkRequested.Unregister(listener);
        }

        // ---- Launch: IsFlying ----

        [Test]
        public void HandleRocketLaunchRequested_ValidConfig_SetsIsFlying()
        {
            InitializeControllerWithConfig();

            _onRocketLaunchRequested.Raise(CreateTestRequest());

            Assert.IsTrue(_controller.IsFlying, "Rocket should be flying after valid launch request.");
        }

        // ---- Launch: IsRocketAscending ----

        [Test]
        public void HandleRocketLaunchRequested_ValidConfig_SetsIsRocketAscendingTrue()
        {
            InitializeControllerWithConfig();

            _onRocketLaunchRequested.Raise(CreateTestRequest());

            Assert.IsTrue(_isRocketAscending.Value, "IsRocketAscending should be true during flight.");
        }

        // ---- Head Position ----

        [Test]
        public void HandleRocketLaunchRequested_ValidConfig_HeadPositionAtSpawn()
        {
            // Configure known spawn position
            SerializedObject configSO = new SerializedObject(_rocketConfig);
            SerializedProperty spawnProp = configSO.FindProperty("_spawnPositions");
            spawnProp.ClearArray();
            spawnProp.InsertArrayElementAtIndex(0);
            spawnProp.GetArrayElementAtIndex(0).vector3Value = new Vector3(0f, -5f, 0f);
            configSO.ApplyModifiedPropertiesWithoutUndo();

            InitializeControllerWithConfig();

            _onRocketLaunchRequested.Raise(CreateTestRequest());

            Vector3 headPos = _controller.HeadPosition;
            Assert.AreEqual(-5f, headPos.y, 0.001f, "Head should start at spawn position.");
        }

        // ---- HasAliveTrailParticles ----

        [Test]
        public void IsFlying_BeforeLaunch_IsFalse()
        {
            InitializeControllerWithConfig();

            Assert.IsFalse(_controller.IsFlying, "Rocket should not be flying before any launch.");
        }
    }
}
