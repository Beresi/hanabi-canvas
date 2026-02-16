// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Tests.EditMode
{
    public class TrailEffectSOTests
    {
        // ---- Private Fields ----
        private TrailEffectSO _effect;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _effect = ScriptableObject.CreateInstance<TrailEffectSO>();

            SerializedObject so = new SerializedObject(_effect);
            so.FindProperty("_stretchMultiplier").floatValue = 0.15f;
            so.FindProperty("_maxStretchLength").floatValue = 1.0f;
            so.FindProperty("_minVelocityThreshold").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_effect);
        }

        // ---- OnValidate Tests ----
        [Test]
        public void OnValidate_StretchMultiplierBelowMin_ClampedToMin()
        {
            TrailEffectSO testEffect = ScriptableObject.CreateInstance<TrailEffectSO>();

            SerializedObject so = new SerializedObject(testEffect);
            so.FindProperty("_stretchMultiplier").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.01f, testEffect.StretchMultiplier, 0.001f);

            Object.DestroyImmediate(testEffect);
        }

        [Test]
        public void OnValidate_MaxStretchLengthBelowMin_ClampedToMin()
        {
            TrailEffectSO testEffect = ScriptableObject.CreateInstance<TrailEffectSO>();

            SerializedObject so = new SerializedObject(testEffect);
            so.FindProperty("_maxStretchLength").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.1f, testEffect.MaxStretchLength, 0.001f);

            Object.DestroyImmediate(testEffect);
        }

        // ---- Property Getter Tests ----
        [Test]
        public void StretchMultiplier_ReturnsExpectedValue()
        {
            Assert.AreEqual(0.15f, _effect.StretchMultiplier, 0.001f);
        }

        [Test]
        public void MaxStretchLength_ReturnsExpectedValue()
        {
            Assert.AreEqual(1.0f, _effect.MaxStretchLength, 0.001f);
        }

        [Test]
        public void MinVelocityThreshold_ReturnsExpectedValue()
        {
            Assert.AreEqual(0.5f, _effect.MinVelocityThreshold, 0.001f);
        }
    }
}
