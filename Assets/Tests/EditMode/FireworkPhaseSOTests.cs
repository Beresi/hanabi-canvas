// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class FireworkPhaseSOTests
    {
        // ---- Private Fields ----
        private FireworkPhaseSO _phase;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _phase = ScriptableObject.CreateInstance<FireworkPhaseSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_phase);
        }

        // ---- Tests ----
        [Test]
        public void Duration_Default_IsPositive()
        {
            Assert.Greater(_phase.Duration, 0f);
        }

        [Test]
        public void OnValidate_DurationBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_phase);
            so.FindProperty("_duration").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_phase.Duration, 0.01f);
        }

        [Test]
        public void OnValidate_DurationNegative_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_phase);
            so.FindProperty("_duration").floatValue = -5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_phase.Duration, 0.01f);
        }

        [Test]
        public void PhaseName_Default_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_phase.PhaseName));
        }

        [Test]
        public void ProgressCurve_Default_IsNotNull()
        {
            Assert.IsNotNull(_phase.ProgressCurve);
        }

        [Test]
        public void ProgressCurve_Default_EvaluatesZeroAtStart()
        {
            Assert.AreEqual(0f, _phase.ProgressCurve.Evaluate(0f), 0.001f);
        }

        [Test]
        public void ProgressCurve_Default_EvaluatesOneAtEnd()
        {
            Assert.AreEqual(1f, _phase.ProgressCurve.Evaluate(1f), 0.001f);
        }
    }
}
