// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class RocketPathSOTests
    {
        // ---- Constants ----
        private static readonly Vector3 SPAWN = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 DESTINATION = new Vector3(0f, 20f, 0f);

        // ---- Private Fields ----
        private StraightRocketPathSO _straightPath;
        private ArcRocketPathSO _arcPath;
        private CurveRocketPathSO _curvePath;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _straightPath = ScriptableObject.CreateInstance<StraightRocketPathSO>();
            SerializedObject straightSO = new SerializedObject(_straightPath);
            straightSO.FindProperty("_speed").floatValue = 10f;
            straightSO.FindProperty("_speedCurve").animationCurveValue =
                AnimationCurve.Linear(0f, 0f, 1f, 1f);
            straightSO.ApplyModifiedPropertiesWithoutUndo();

            _arcPath = ScriptableObject.CreateInstance<ArcRocketPathSO>();
            SerializedObject arcSO = new SerializedObject(_arcPath);
            arcSO.FindProperty("_speed").floatValue = 10f;
            arcSO.FindProperty("_arcHeight").floatValue = 5f;
            arcSO.FindProperty("_progressCurve").animationCurveValue =
                AnimationCurve.Linear(0f, 0f, 1f, 1f);
            arcSO.FindProperty("_arcCurve").animationCurveValue =
                new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.5f, 1f),
                    new Keyframe(1f, 0f)
                );
            arcSO.ApplyModifiedPropertiesWithoutUndo();

            _curvePath = ScriptableObject.CreateInstance<CurveRocketPathSO>();
            SerializedObject curveSO = new SerializedObject(_curvePath);
            curveSO.FindProperty("_speed").floatValue = 10f;
            curveSO.FindProperty("_controlPoint1Offset").vector3Value = new Vector3(0f, 5f, 0f);
            curveSO.FindProperty("_controlPoint2Offset").vector3Value = new Vector3(0f, 3f, 0f);
            curveSO.FindProperty("_progressCurve").animationCurveValue =
                AnimationCurve.Linear(0f, 0f, 1f, 1f);
            curveSO.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_straightPath);
            Object.DestroyImmediate(_arcPath);
            Object.DestroyImmediate(_curvePath);
        }

        // ---- StraightRocketPathSO Tests ----
        [Test]
        public void StraightPath_Evaluate_AtZero_ReturnsSpawn()
        {
            Vector3 result = _straightPath.Evaluate(SPAWN, DESTINATION, 0f);

            Assert.AreEqual(SPAWN.x, result.x, 0.001f);
            Assert.AreEqual(SPAWN.y, result.y, 0.001f);
            Assert.AreEqual(SPAWN.z, result.z, 0.001f);
        }

        [Test]
        public void StraightPath_Evaluate_AtOne_ReturnsDestination()
        {
            Vector3 result = _straightPath.Evaluate(SPAWN, DESTINATION, 1f);

            Assert.AreEqual(DESTINATION.x, result.x, 0.001f);
            Assert.AreEqual(DESTINATION.y, result.y, 0.001f);
            Assert.AreEqual(DESTINATION.z, result.z, 0.001f);
        }

        [Test]
        public void StraightPath_GetFlightDuration_PositiveValue()
        {
            float duration = _straightPath.GetFlightDuration(SPAWN, DESTINATION);

            Assert.Greater(duration, 0f);
        }

        // ---- ArcRocketPathSO Tests ----
        [Test]
        public void ArcPath_Evaluate_AtZero_ReturnsSpawn()
        {
            Vector3 result = _arcPath.Evaluate(SPAWN, DESTINATION, 0f);

            Assert.AreEqual(SPAWN.x, result.x, 0.001f);
            Assert.AreEqual(SPAWN.y, result.y, 0.001f);
            Assert.AreEqual(SPAWN.z, result.z, 0.001f);
        }

        [Test]
        public void ArcPath_Evaluate_AtOne_ReturnsDestination()
        {
            Vector3 result = _arcPath.Evaluate(SPAWN, DESTINATION, 1f);

            Assert.AreEqual(DESTINATION.x, result.x, 0.001f);
            Assert.AreEqual(DESTINATION.y, result.y, 0.001f);
            Assert.AreEqual(DESTINATION.z, result.z, 0.001f);
        }

        [Test]
        public void ArcPath_Evaluate_AtHalf_HasVerticalOffset()
        {
            Vector3 result = _arcPath.Evaluate(SPAWN, DESTINATION, 0.5f);

            // The midpoint of a straight lerp would be (0, 10, 0).
            // The arc adds _arcHeight * _arcCurve(0.5) = 5 * 1 = 5 on top.
            float straightMidpointY = Vector3.Lerp(SPAWN, DESTINATION, 0.5f).y;

            Assert.Greater(result.y, straightMidpointY,
                "Arc path at t=0.5 should have higher Y than a straight lerp midpoint");
        }

        [Test]
        public void ArcPath_GetFlightDuration_PositiveValue()
        {
            float duration = _arcPath.GetFlightDuration(SPAWN, DESTINATION);

            Assert.Greater(duration, 0f);
        }

        // ---- CurveRocketPathSO Tests ----
        [Test]
        public void CurvePath_Evaluate_AtZero_ReturnsSpawn()
        {
            Vector3 result = _curvePath.Evaluate(SPAWN, DESTINATION, 0f);

            Assert.AreEqual(SPAWN.x, result.x, 0.001f);
            Assert.AreEqual(SPAWN.y, result.y, 0.001f);
            Assert.AreEqual(SPAWN.z, result.z, 0.001f);
        }

        [Test]
        public void CurvePath_Evaluate_AtOne_ReturnsDestination()
        {
            Vector3 result = _curvePath.Evaluate(SPAWN, DESTINATION, 1f);

            Assert.AreEqual(DESTINATION.x, result.x, 0.001f);
            Assert.AreEqual(DESTINATION.y, result.y, 0.001f);
            Assert.AreEqual(DESTINATION.z, result.z, 0.001f);
        }

        [Test]
        public void CurvePath_GetFlightDuration_PositiveValue()
        {
            float duration = _curvePath.GetFlightDuration(SPAWN, DESTINATION);

            Assert.Greater(duration, 0f);
        }

        // ---- Base Class EvaluateVelocity Test ----
        [Test]
        public void EvaluateVelocity_AtHalf_ReturnsNonZero()
        {
            Vector3 velocity = _straightPath.EvaluateVelocity(SPAWN, DESTINATION, 0.5f);

            Assert.Greater(velocity.magnitude, 0f,
                "Velocity at midpoint should be non-zero for a moving path");
        }
    }
}
