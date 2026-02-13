// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.CameraSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HanabiCanvas.Tests.PlayMode
{
    public class CameraControllerTests
    {
        // ---- Private Fields ----
        private GameObject _cameraObject;
        private Camera _camera;
        private CameraConfigSO _config;
        private GameObject _controllerObject;
        private CameraController _controller;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _cameraObject = new GameObject("TestCamera");
            _camera = _cameraObject.AddComponent<Camera>();

            _config = ScriptableObject.CreateInstance<CameraConfigSO>();

#if UNITY_EDITOR
            SerializedObject configSO = new SerializedObject(_config);
            configSO.FindProperty("_transitionDuration").floatValue = 0.1f;
            configSO.FindProperty("_canvasViewPosition").vector3Value = new Vector3(0f, 0f, -10f);
            configSO.FindProperty("_canvasViewRotationEuler").vector3Value = Vector3.zero;
            configSO.FindProperty("_skyViewPosition").vector3Value = new Vector3(0f, 10f, -15f);
            configSO.FindProperty("_skyViewRotationEuler").vector3Value = new Vector3(10f, 0f, 0f);
            configSO.FindProperty("_burstShakeIntensity").floatValue = 0.5f;
            configSO.FindProperty("_burstShakeDuration").floatValue = 0.3f;
            configSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _controllerObject = new GameObject("TestCameraController");
            _controllerObject.SetActive(false);
            _controller = _controllerObject.AddComponent<CameraController>();

#if UNITY_EDITOR
            SerializedObject controllerSO = new SerializedObject(_controller);
            controllerSO.FindProperty("_config").objectReferenceValue = _config;
            controllerSO.FindProperty("_camera").objectReferenceValue = _camera;
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

            Object.DestroyImmediate(_config);
        }

        // ---- Tests ----

        [UnityTest]
        public IEnumerator Start_SnapsToCanvasView()
        {
            yield return null;

            Vector3 expectedPos = _config.CanvasViewPosition;
            Quaternion expectedRot = _config.CanvasViewRotation;

            Assert.That(_camera.transform.position.x, Is.EqualTo(expectedPos.x).Within(0.05f));
            Assert.That(_camera.transform.position.y, Is.EqualTo(expectedPos.y).Within(0.05f));
            Assert.That(_camera.transform.position.z, Is.EqualTo(expectedPos.z).Within(0.05f));

            Assert.That(_camera.transform.rotation.x, Is.EqualTo(expectedRot.x).Within(0.05f));
            Assert.That(_camera.transform.rotation.y, Is.EqualTo(expectedRot.y).Within(0.05f));
            Assert.That(_camera.transform.rotation.z, Is.EqualTo(expectedRot.z).Within(0.05f));
            Assert.That(_camera.transform.rotation.w, Is.EqualTo(expectedRot.w).Within(0.05f));
        }

        [UnityTest]
        public IEnumerator TransitionToSkyView_IsTransitioning_ReturnsTrue()
        {
            yield return null;

            _controller.TransitionToSkyView();

            Assert.IsTrue(_controller.IsTransitioning);
        }

        [UnityTest]
        public IEnumerator TransitionToSkyView_AfterDuration_ReachesSkyPosition()
        {
            yield return null;

            _controller.TransitionToSkyView();

            yield return new WaitForSeconds(0.2f);

            Vector3 expectedPos = _config.SkyViewPosition;
            Assert.That(_camera.transform.position.x, Is.EqualTo(expectedPos.x).Within(0.05f));
            Assert.That(_camera.transform.position.y, Is.EqualTo(expectedPos.y).Within(0.05f));
            Assert.That(_camera.transform.position.z, Is.EqualTo(expectedPos.z).Within(0.05f));

            Assert.IsFalse(_controller.IsTransitioning);
        }

        [UnityTest]
        public IEnumerator TransitionToCanvasView_AfterDuration_ReachesCanvasPosition()
        {
            yield return null;

            _controller.TransitionToSkyView();

            yield return new WaitForSeconds(0.2f);

            _controller.TransitionToCanvasView();

            yield return new WaitForSeconds(0.2f);

            Vector3 expectedPos = _config.CanvasViewPosition;
            Assert.That(_camera.transform.position.x, Is.EqualTo(expectedPos.x).Within(0.05f));
            Assert.That(_camera.transform.position.y, Is.EqualTo(expectedPos.y).Within(0.05f));
            Assert.That(_camera.transform.position.z, Is.EqualTo(expectedPos.z).Within(0.05f));
        }

        [UnityTest]
        public IEnumerator IsTransitioning_AfterComplete_ReturnsFalse()
        {
            yield return null;

            _controller.TransitionToSkyView();

            yield return new WaitForSeconds(0.2f);

            Assert.IsFalse(_controller.IsTransitioning);
        }

        [UnityTest]
        public IEnumerator TriggerBurstShake_OffsetsPosition()
        {
            yield return null;

            _controller.TransitionToSkyView();

            yield return new WaitForSeconds(0.2f);

            Vector3 preShakePosition = _camera.transform.position;

            _controller.TriggerBurstShake();

            yield return null;

            Vector3 postShakePosition = _camera.transform.position;

            Assert.That(
                postShakePosition != preShakePosition,
                "Camera position should be offset during shake");
        }
    }
}
