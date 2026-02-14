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
    public class IntVariableSOTests
    {
        // ---- Private Fields ----
        private IntVariableSO _intVariable;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _intVariable = ScriptableObject.CreateInstance<IntVariableSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_intVariable);
        }

        // ---- Tests ----
        [Test]
        public void Value_AfterCreation_EqualsDefaultInitialValue()
        {
            Assert.AreEqual(0, _intVariable.Value);
        }

        [Test]
        public void Value_SetNewValue_UpdatesValue()
        {
            _intVariable.Value = 42;

            Assert.AreEqual(42, _intVariable.Value);
        }

        [Test]
        public void Value_SetNewValue_FiresOnValueChanged()
        {
            int _receivedValue = -1;
            _intVariable.OnValueChanged += (value) => _receivedValue = value;

            _intVariable.Value = 10;

            Assert.AreEqual(10, _receivedValue);
        }

        [Test]
        public void Value_SetSameValue_DoesNotFireOnValueChanged()
        {
            _intVariable.Value = 5;

            bool _wasFired = false;
            _intVariable.OnValueChanged += (value) => _wasFired = true;

            _intVariable.Value = 5;

            Assert.IsFalse(_wasFired);
        }

        [Test]
        public void ResetToInitial_AfterValueChanged_RestoresInitialValue()
        {
            SerializedObject so = new SerializedObject(_intVariable);
            so.FindProperty("initialValue").intValue = 7;
            so.ApplyModifiedPropertiesWithoutUndo();

            _intVariable.Value = 100;
            _intVariable.ResetToInitial();

            Assert.AreEqual(7, _intVariable.Value);
        }

        [Test]
        public void ResetToInitial_AfterValueChanged_ValueMatchesInitial()
        {
            SerializedObject so = new SerializedObject(_intVariable);
            so.FindProperty("initialValue").intValue = 3;
            so.ApplyModifiedPropertiesWithoutUndo();

            _intVariable.ResetToInitial();
            _intVariable.Value = 99;
            _intVariable.ResetToInitial();

            Assert.AreEqual(3, _intVariable.Value);
        }
    }
}
