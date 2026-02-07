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
    public class FireworkConfigSOTests
    {
        // ---- Private Fields ----
        private FireworkConfigSO _fireworkConfig;
        private FireworkPhaseSO[] _phases;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _fireworkConfig = ScriptableObject.CreateInstance<FireworkConfigSO>();
            _phases = new FireworkPhaseSO[4];
            for (int i = 0; i < 4; i++)
            {
                _phases[i] = ScriptableObject.CreateInstance<FireworkPhaseSO>();
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_fireworkConfig);
            for (int i = 0; i < _phases.Length; i++)
            {
                if (_phases[i] != null)
                {
                    Object.DestroyImmediate(_phases[i]);
                }
            }
        }

        // ---- Tests ----
        [Test]
        public void PhaseCount_NoPhasesSet_ReturnsZero()
        {
            Assert.AreEqual(0, _fireworkConfig.PhaseCount);
        }

        [Test]
        public void PhaseCount_WithPhases_ReturnsCorrectCount()
        {
            SerializedObject so = new SerializedObject(_fireworkConfig);
            SerializedProperty phasesProp = so.FindProperty("_phases");

            phasesProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                phasesProp.GetArrayElementAtIndex(i).objectReferenceValue = _phases[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(4, _fireworkConfig.PhaseCount);
        }

        [Test]
        public void GetPhase_ValidIndex_ReturnsPhase()
        {
            SerializedObject so = new SerializedObject(_fireworkConfig);
            SerializedProperty phasesProp = so.FindProperty("_phases");

            phasesProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                phasesProp.GetArrayElementAtIndex(i).objectReferenceValue = _phases[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            FireworkPhaseSO result = _fireworkConfig.GetPhase(0);

            Assert.IsNotNull(result);
            Assert.AreEqual(_phases[0], result);
        }

        [Test]
        public void GetPhase_NegativeIndex_ReturnsNull()
        {
            SerializedObject so = new SerializedObject(_fireworkConfig);
            SerializedProperty phasesProp = so.FindProperty("_phases");

            phasesProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                phasesProp.GetArrayElementAtIndex(i).objectReferenceValue = _phases[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.IsNull(_fireworkConfig.GetPhase(-1));
        }

        [Test]
        public void GetPhase_IndexOutOfRange_ReturnsNull()
        {
            SerializedObject so = new SerializedObject(_fireworkConfig);
            SerializedProperty phasesProp = so.FindProperty("_phases");

            phasesProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                phasesProp.GetArrayElementAtIndex(i).objectReferenceValue = _phases[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.IsNull(_fireworkConfig.GetPhase(10));
        }

        [Test]
        public void GetPhase_NullPhases_ReturnsNull()
        {
            Assert.IsNull(_fireworkConfig.GetPhase(0));
        }

        [Test]
        public void OnValidate_BurstRadiusBelowMin_ClampsToMin()
        {
            LogAssert.Expect(LogType.Warning,
                new System.Text.RegularExpressions.Regex(
                    ".*phases.*",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase));

            SerializedObject so = new SerializedObject(_fireworkConfig);
            so.FindProperty("_burstRadius").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(_fireworkConfig.BurstRadius, 0.1f);
        }

        [Test]
        public void BurstRadius_Default_IsPositive()
        {
            Assert.Greater(_fireworkConfig.BurstRadius, 0f);
        }

        [Test]
        public void FormationScale_Default_IsPositive()
        {
            Assert.Greater(_fireworkConfig.FormationScale, 0f);
        }
    }
}
