// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Firework;
using HanabiCanvas.Runtime.GameFlow;
using HanabiCanvas.Runtime.CameraSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HanabiCanvas.Tests.PlayMode
{
    public class FireworkSessionManagerTests
    {
        // ---- Private Fields ----
        private GameObject _managerObject;
        private FireworkSessionManager _manager;

        private GameObject _cameraControllerObject;
        private CameraController _cameraController;

        private GameObject _spawnPointObject;

        // ---- SOs ----
        private PixelDataSO _pixelData;
        private CanvasConfigSO _canvasConfig;
        private CameraConfigSO _cameraConfig;
        private BoolVariableSO _isCanvasInputEnabled;
        private BoolVariableSO _isFireworkPlaying;
        private GameStateVariableSO _gameState;
        private GameEventSO _onLaunchFirework;
        private FireworkRequestEventSO _onFireworkRequested;
        private GameEventSO _onCanvasCleared;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            // Create SO instances
            _pixelData = ScriptableObject.CreateInstance<PixelDataSO>();
            _canvasConfig = ScriptableObject.CreateInstance<CanvasConfigSO>();
            _cameraConfig = ScriptableObject.CreateInstance<CameraConfigSO>();
            _isCanvasInputEnabled = ScriptableObject.CreateInstance<BoolVariableSO>();
            _isFireworkPlaying = ScriptableObject.CreateInstance<BoolVariableSO>();
            _onLaunchFirework = ScriptableObject.CreateInstance<GameEventSO>();
            _gameState = ScriptableObject.CreateInstance<GameStateVariableSO>();
            _onFireworkRequested = ScriptableObject.CreateInstance<FireworkRequestEventSO>();
            _onCanvasCleared = ScriptableObject.CreateInstance<GameEventSO>();

#if UNITY_EDITOR
            // Configure BoolVariableSO initial value to true
            SerializedObject boolSO = new SerializedObject(_isCanvasInputEnabled);
            boolSO.FindProperty("_initialValue").boolValue = true;
            boolSO.ApplyModifiedPropertiesWithoutUndo();

            // Configure CameraConfigSO with short transition
            SerializedObject camConfigSO = new SerializedObject(_cameraConfig);
            camConfigSO.FindProperty("_transitionDuration").floatValue = 0.1f;
            camConfigSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            // Add test pixels to pixel data
            _pixelData.SetPixel(0, 0, new Color32(255, 0, 0, 255));
            _pixelData.SetPixel(1, 1, new Color32(0, 255, 0, 255));
            _pixelData.SetPixel(2, 2, new Color32(0, 0, 255, 255));

            // Create spawn point
            _spawnPointObject = new GameObject("SpawnPoint");
            _spawnPointObject.transform.position = new Vector3(0f, 10f, 0f);

            // Create CameraController (inactive -> AddComponent -> wire -> activate)
            _cameraControllerObject = new GameObject("CameraController");
            _cameraControllerObject.SetActive(false);
            Camera camera = _cameraControllerObject.AddComponent<Camera>();
            _cameraController = _cameraControllerObject.AddComponent<CameraController>();

#if UNITY_EDITOR
            SerializedObject ccSO = new SerializedObject(_cameraController);
            ccSO.FindProperty("_config").objectReferenceValue = _cameraConfig;
            ccSO.FindProperty("_camera").objectReferenceValue = camera;
            ccSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _cameraControllerObject.SetActive(true);

            // Create FireworkSessionManager (inactive -> AddComponent -> wire -> activate)
            _managerObject = new GameObject("FireworkSessionManager");
            _managerObject.SetActive(false);
            _manager = _managerObject.AddComponent<FireworkSessionManager>();

#if UNITY_EDITOR
            SerializedObject fsmSO = new SerializedObject(_manager);
            fsmSO.FindProperty("_pixelData").objectReferenceValue = _pixelData;
            fsmSO.FindProperty("_canvasConfig").objectReferenceValue = _canvasConfig;
            fsmSO.FindProperty("_cameraController").objectReferenceValue = _cameraController;
            fsmSO.FindProperty("_isCanvasInputEnabled").objectReferenceValue = _isCanvasInputEnabled;
            fsmSO.FindProperty("_fireworkSpawnPoint").objectReferenceValue = _spawnPointObject.transform;
            fsmSO.FindProperty("_onLaunchFirework").objectReferenceValue = _onLaunchFirework;
            fsmSO.FindProperty("_gameState").objectReferenceValue = _gameState;
            fsmSO.FindProperty("_onFireworkRequested").objectReferenceValue = _onFireworkRequested;
            fsmSO.FindProperty("_onCanvasCleared").objectReferenceValue = _onCanvasCleared;
            fsmSO.FindProperty("_isFireworkPlaying").objectReferenceValue = _isFireworkPlaying;
            fsmSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _managerObject.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            if (_managerObject != null)
            {
                Object.DestroyImmediate(_managerObject);
            }

            if (_cameraControllerObject != null)
            {
                Object.DestroyImmediate(_cameraControllerObject);
            }

            if (_spawnPointObject != null)
            {
                Object.DestroyImmediate(_spawnPointObject);
            }

            Object.DestroyImmediate(_pixelData);
            Object.DestroyImmediate(_canvasConfig);
            Object.DestroyImmediate(_cameraConfig);
            Object.DestroyImmediate(_isCanvasInputEnabled);
            Object.DestroyImmediate(_isFireworkPlaying);
            Object.DestroyImmediate(_onLaunchFirework);
            Object.DestroyImmediate(_gameState);
            Object.DestroyImmediate(_onFireworkRequested);
            Object.DestroyImmediate(_onCanvasCleared);
        }

        // ---- Tests ----

        [Test]
        public void Awake_InitialState_IsDrawing()
        {
            Assert.AreEqual(GameState.Drawing, _manager.CurrentState);
        }

        [Test]
        public void HandleLaunchFirework_InDrawingState_TransitionsToLaunching()
        {
            _onLaunchFirework.Raise();

            Assert.AreEqual(GameState.Launching, _manager.CurrentState);
        }

        [UnityTest]
        public IEnumerator HandleLaunchFirework_NotInDrawingState_IsIgnored()
        {
            // First raise transitions to Launching
            _onLaunchFirework.Raise();
            Assert.AreEqual(GameState.Launching, _manager.CurrentState);

            // Wait one frame so Launching transitions to Watching
            yield return null;
            Assert.AreNotEqual(GameState.Launching, _manager.CurrentState);

            // Store the state after transition
            GameState stateAfterTransition = _manager.CurrentState;

            // Second raise should be ignored (not in Drawing state)
            _onLaunchFirework.Raise();
            Assert.AreEqual(stateAfterTransition, _manager.CurrentState);
        }

        [UnityTest]
        public IEnumerator Update_Launching_DisablesCanvasInput()
        {
            // Verify initial state
            Assert.IsTrue(_isCanvasInputEnabled.Value);

            _onLaunchFirework.Raise();

            // Wait one frame for Update to process Launching state
            yield return null;

            Assert.IsFalse(_isCanvasInputEnabled.Value);
        }

        [UnityTest]
        public IEnumerator Update_Launching_RaisesFireworkRequest()
        {
            bool wasRaised = false;
            FireworkRequest capturedRequest = default;

            _onFireworkRequested.Register((FireworkRequest request) =>
            {
                wasRaised = true;
                capturedRequest = request;
            });

            _onLaunchFirework.Raise();

            // Wait one frame for Update to process Launching state
            yield return null;

            Assert.IsTrue(wasRaised, "FireworkRequestEventSO should have been raised.");
            Assert.IsNotNull(capturedRequest.Pattern, "Request pattern should not be null.");
            Assert.AreEqual(3, capturedRequest.Pattern.Length, "Request should contain 3 pixels.");
            Assert.AreEqual(new Vector3(0f, 10f, 0f), capturedRequest.Position);
        }

        [UnityTest]
        public IEnumerator Update_Launching_TransitionsToWatching()
        {
            _onLaunchFirework.Raise();

            // After Raise, state is Launching.
            // After one frame (Update processes Launching), state transitions to Watching.
            yield return null;

            Assert.AreEqual(GameState.Watching, _manager.CurrentState);
        }

        [UnityTest]
        public IEnumerator Update_Launching_EmptyPixelData_ReturnsToDrawing()
        {
            // Clear pixel data to make it empty
            _pixelData.Clear();
            Assert.AreEqual(0, _pixelData.PixelCount);

            // Expect the warning about no pixels
            LogAssert.Expect(LogType.Warning, new Regex(@"No pixels to launch"));

            _onLaunchFirework.Raise();
            Assert.AreEqual(GameState.Launching, _manager.CurrentState);

            // Wait one frame for Update to process Launching state
            yield return null;

            // Empty canvas should return to Drawing
            Assert.AreEqual(GameState.Drawing, _manager.CurrentState);
            Assert.IsTrue(_isCanvasInputEnabled.Value, "Canvas input should be re-enabled.");
        }

        [UnityTest]
        public IEnumerator Update_Watching_FireworkNotPlaying_TransitionsToResetting()
        {
            // _isFireworkPlaying.Value is false by default, so Watching will immediately transition
            _onLaunchFirework.Raise();

            // Frame 1: Launching -> requests firework -> transitions to Watching
            yield return null;
            Assert.AreEqual(GameState.Watching, _manager.CurrentState);

            // Frame 2: Watching -> _isFireworkPlaying.Value is false -> transitions to Resetting
            yield return null;
            Assert.AreEqual(GameState.Resetting, _manager.CurrentState);
        }

        [UnityTest]
        public IEnumerator Update_Resetting_CameraNotTransitioning_TransitionsToDrawing()
        {
            _onLaunchFirework.Raise();

            // Frame 1: Launching -> Watching
            yield return null;

            // Frame 2: Watching -> Resetting (firework not playing)
            yield return null;
            Assert.AreEqual(GameState.Resetting, _manager.CurrentState);

            // Wait for camera transition to finish (0.1s configured)
            // Yield multiple frames to ensure camera transition completes
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            Assert.AreEqual(GameState.Drawing, _manager.CurrentState);
        }

        [UnityTest]
        public IEnumerator Update_Resetting_RaisesCanvasCleared()
        {
            bool wasClearedRaised = false;
            System.Action clearedListener = () => wasClearedRaised = true;
            _onCanvasCleared.Register(clearedListener);

            _onLaunchFirework.Raise();

            // Frame 1: Launching -> Watching
            yield return null;

            // Frame 2: Watching -> Resetting
            yield return null;

            // Wait for camera transition and reset to complete
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            Assert.IsTrue(wasClearedRaised, "Canvas cleared event should have been raised.");

            _onCanvasCleared.Unregister(clearedListener);
        }

        [UnityTest]
        public IEnumerator Update_Resetting_ReEnablesCanvasInput()
        {
            _onLaunchFirework.Raise();

            // Frame 1: Launching -> Watching (input disabled)
            yield return null;
            Assert.IsFalse(_isCanvasInputEnabled.Value);

            // Frame 2: Watching -> Resetting
            yield return null;

            // Wait for camera transition and full reset cycle
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            Assert.AreEqual(GameState.Drawing, _manager.CurrentState);
            Assert.IsTrue(_isCanvasInputEnabled.Value, "Canvas input should be re-enabled after reset.");
        }
    }
}
