// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Firework;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HanabiCanvas.Tests.PlayMode
{
    public class RocketControllerIntegrationTests
    {
        // ---- Private Fields ----
        private GameObject _controllerObject;
        private RocketController _controller;
        private GameObject _cameraObject;

        // ---- SOs ----
        private RocketConfigSO _rocketConfig;
        private StraightRocketPathSO _straightPath;
        private FireworkRequestEventSO _onRocketLaunchRequested;
        private FireworkRequestEventSO _onFireworkRequested;
        private BoolVariableSO _isRocketAscending;

        // ---- Setup / Teardown ----

        [SetUp]
        public void SetUp()
        {
            // Create a camera so Camera.main is available
            _cameraObject = new GameObject("MainCamera");
            _cameraObject.tag = "MainCamera";
            _cameraObject.AddComponent<Camera>();

            // Create SO instances
            _rocketConfig = ScriptableObject.CreateInstance<RocketConfigSO>();
            _straightPath = ScriptableObject.CreateInstance<StraightRocketPathSO>();
            _onRocketLaunchRequested = ScriptableObject.CreateInstance<FireworkRequestEventSO>();
            _onFireworkRequested = ScriptableObject.CreateInstance<FireworkRequestEventSO>();
            _isRocketAscending = ScriptableObject.CreateInstance<BoolVariableSO>();

#if UNITY_EDITOR
            // Configure straight path with very fast speed for quick test completion
            SerializedObject pathSO = new SerializedObject(_straightPath);
            pathSO.FindProperty("_speed").floatValue = 1000f;
            pathSO.ApplyModifiedPropertiesWithoutUndo();

            // Configure rocket config with known spawn/destination and path
            SerializedObject configSO = new SerializedObject(_rocketConfig);

            SerializedProperty spawnProp = configSO.FindProperty("_spawnPositions");
            spawnProp.ClearArray();
            spawnProp.InsertArrayElementAtIndex(0);
            spawnProp.GetArrayElementAtIndex(0).vector3Value = new Vector3(0f, 0f, 0f);

            SerializedProperty destProp = configSO.FindProperty("_destinationPositions");
            destProp.ClearArray();
            destProp.InsertArrayElementAtIndex(0);
            destProp.GetArrayElementAtIndex(0).vector3Value = new Vector3(0f, 5f, 0f);

            SerializedProperty pathProp = configSO.FindProperty("_pathBehaviours");
            pathProp.ClearArray();
            pathProp.InsertArrayElementAtIndex(0);
            pathProp.GetArrayElementAtIndex(0).objectReferenceValue = _straightPath;

            // Short trail lifetime for quick cleanup
            configSO.FindProperty("_trailLifetime").floatValue = 0.05f;
            configSO.FindProperty("_trailParticleCount").intValue = 5;

            configSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            // Create RocketController (inactive -> AddComponent -> wire -> activate)
            _controllerObject = new GameObject("RocketController");
            _controllerObject.SetActive(false);
            _controllerObject.AddComponent<MeshFilter>();
            _controllerObject.AddComponent<MeshRenderer>();
            _controller = _controllerObject.AddComponent<RocketController>();

#if UNITY_EDITOR
            SerializedObject controllerSO = new SerializedObject(_controller);
            controllerSO.FindProperty("_rocketConfig").objectReferenceValue = _rocketConfig;
            controllerSO.FindProperty("_onRocketLaunchRequested").objectReferenceValue = _onRocketLaunchRequested;
            controllerSO.FindProperty("_onFireworkRequested").objectReferenceValue = _onFireworkRequested;
            controllerSO.FindProperty("_isRocketAscending").objectReferenceValue = _isRocketAscending;
            controllerSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _controllerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_controllerObject != null)
            {
                Object.DestroyImmediate(_controllerObject);
            }

            if (_cameraObject != null)
            {
                Object.DestroyImmediate(_cameraObject);
            }

            Object.DestroyImmediate(_rocketConfig);
            Object.DestroyImmediate(_straightPath);
            Object.DestroyImmediate(_onRocketLaunchRequested);
            Object.DestroyImmediate(_onFireworkRequested);
            Object.DestroyImmediate(_isRocketAscending);
        }

        // ---- Helper Methods ----

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

        // ---- Tests ----

        [UnityTest]
        public IEnumerator RocketLaunch_FullCycle_RaisesFireworkRequestOnArrival()
        {
            bool wasRaised = false;
            _onFireworkRequested.Register((FireworkRequest req) =>
            {
                wasRaised = true;
            });

            _onRocketLaunchRequested.Raise(CreateTestRequest());

            // With speed 1000 and distance 5, flight takes ~0.005s
            // Wait a few frames for the rocket to arrive
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            Assert.IsTrue(wasRaised, "FireworkRequestEventSO should have been raised after rocket arrived.");
        }

        [UnityTest]
        public IEnumerator RocketLaunch_PositionSetToDestination_MatchesConfigDestination()
        {
            FireworkRequest capturedRequest = default;
            _onFireworkRequested.Register((FireworkRequest req) =>
            {
                capturedRequest = req;
            });

            _onRocketLaunchRequested.Raise(CreateTestRequest());

            // Wait for arrival
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            // The destination position is (0, 5, 0) as configured
            Assert.AreEqual(0f, capturedRequest.Position.x, 0.01f,
                "Request position X should match destination.");
            Assert.AreEqual(5f, capturedRequest.Position.y, 0.01f,
                "Request position Y should match destination.");
            Assert.AreEqual(0f, capturedRequest.Position.z, 0.01f,
                "Request position Z should match destination.");
        }

        [UnityTest]
        public IEnumerator RocketLaunch_IsRocketAscending_TrueWhileFlying()
        {
            _onRocketLaunchRequested.Raise(CreateTestRequest());

            // Check immediately after launch (same frame, before Update runs)
            Assert.IsTrue(_isRocketAscending.Value,
                "IsRocketAscending should be true immediately after launch.");
            Assert.IsTrue(_controller.IsFlying,
                "IsFlying should be true immediately after launch.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator RocketLaunch_IsRocketAscending_FalseOnArrival()
        {
            _onRocketLaunchRequested.Raise(CreateTestRequest());
            Assert.IsTrue(_isRocketAscending.Value);

            // Wait for arrival
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            Assert.IsFalse(_isRocketAscending.Value,
                "IsRocketAscending should be false after rocket arrives.");
            Assert.IsFalse(_controller.IsFlying,
                "IsFlying should be false after rocket arrives.");
        }

        [UnityTest]
        public IEnumerator RocketLaunch_PatternPreserved_RequestContainsOriginalPattern()
        {
            FireworkRequest capturedRequest = default;
            _onFireworkRequested.Register((FireworkRequest req) =>
            {
                capturedRequest = req;
            });

            FireworkRequest testRequest = CreateTestRequest();
            _onRocketLaunchRequested.Raise(testRequest);

            // Wait for arrival
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            Assert.IsNotNull(capturedRequest.Pattern,
                "Pattern should not be null in forwarded request.");
            Assert.AreEqual(2, capturedRequest.Pattern.Length,
                "Pattern should have same number of entries as original request.");
            Assert.AreEqual(32, capturedRequest.PatternWidth,
                "PatternWidth should be preserved.");
            Assert.AreEqual(32, capturedRequest.PatternHeight,
                "PatternHeight should be preserved.");
        }
    }
}
