// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class CameraConfigSOTests
    {
        // ---- Private Fields ----
        private CameraConfigSO _config;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<CameraConfigSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        // ---- Tests ----
        [Test]
        public void OnValidate_TransitionDurationBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_transitionDuration").floatValue = 0.01f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_config.TransitionDuration, 0.1f);
        }

        [Test]
        public void OnValidate_ShakeIntensityNegative_ClampsToZero()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_burstShakeIntensity").floatValue = -1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_config.BurstShakeIntensity, 0f);
        }

        [Test]
        public void OnValidate_ShakeDurationNegative_ClampsToZero()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_burstShakeDuration").floatValue = -5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_config.BurstShakeDuration, 0f);
        }

        [Test]
        public void CanvasViewRotation_FromEulerAngles_ReturnsCorrectQuaternion()
        {
            SerializedObject so = new SerializedObject(_config);
            SerializedProperty rotProp = so.FindProperty("_canvasViewRotationEuler");
            rotProp.vector3Value = new Vector3(45f, 90f, 0f);
            so.ApplyModifiedPropertiesWithoutUndo();

            Quaternion expected = Quaternion.Euler(45f, 90f, 0f);
            Quaternion actual = _config.CanvasViewRotation;

            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.001f));
            Assert.That(actual.w, Is.EqualTo(expected.w).Within(0.001f));
        }

        [Test]
        public void TransitionCurve_Default_EvaluatesEaseInOut()
        {
            Assert.That(_config.TransitionCurve.Evaluate(0f), Is.EqualTo(0f).Within(0.001f));
            Assert.That(_config.TransitionCurve.Evaluate(1f), Is.EqualTo(1f).Within(0.001f));
        }
    }
}
