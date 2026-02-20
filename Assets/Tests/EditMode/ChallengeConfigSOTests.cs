// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class ChallengeConfigSOTests
    {
        // ---- Private Fields ----
        private ChallengeConfigSO _config;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<ChallengeConfigSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        // ---- Tests ----
        [Test]
        public void OnValidate_MaxActiveRequestsBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_maxActiveRequests").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_config.MaxActiveRequests, 1);
        }

        [Test]
        public void OnValidate_MaxActiveRequestsAboveMax_ClampsToMax()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_maxActiveRequests").intValue = 100;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.LessOrEqual(_config.MaxActiveRequests, 10);
        }

        [Test]
        public void OnValidate_DefaultTimeLimitBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_defaultTimeLimit").floatValue = 1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_config.DefaultTimeLimit, 5f);
        }

        [Test]
        public void OnValidate_DefaultColorLimitBelowMin_ClampsToMin()
        {
            SerializedObject so = new SerializedObject(_config);
            so.FindProperty("_defaultColorLimit").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_config.DefaultColorLimit, 1);
        }

        [Test]
        public void PredefinedRequests_Default_IsNullOrEmpty()
        {
            RequestData[] requests = _config.PredefinedRequests;

            Assert.IsTrue(requests == null || requests.Length == 0);
        }
    }
}
