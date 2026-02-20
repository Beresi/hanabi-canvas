// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class SlideshowConfigSOTests
    {
        // ---- Private Fields ----
        private SlideshowConfigSO _config;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<SlideshowConfigSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        // ---- Tests ----
        [Test]
        public void OnValidate_DisplayDurationBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_artworkDisplayDuration").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_config.ArtworkDisplayDuration, 1f);
        }

        [Test]
        public void OnValidate_TransitionDurationBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_transitionDuration").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_config.TransitionDuration, 0.1f);
        }

        [Test]
        public void IsLooping_Default_IsTrue()
        {
            Assert.IsTrue(_config.IsLooping);
        }

        [Test]
        public void IsShuffled_Default_IsFalse()
        {
            Assert.IsFalse(_config.IsShuffled);
        }
    }
}
