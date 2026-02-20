// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.GameFlow;

namespace HanabiCanvas.Tests.EditMode
{
    public class AppStateVariableSOTests
    {
        // ---- Private Fields ----
        private AppStateVariableSO _appStateVariable;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _appStateVariable = ScriptableObject.CreateInstance<AppStateVariableSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_appStateVariable);
        }

        // ---- Tests ----
        [Test]
        public void Value_AfterCreation_EqualsDefaultInitialValue()
        {
            Assert.AreEqual(AppState.Menu, _appStateVariable.Value);
        }

        [Test]
        public void Value_SetNewValue_UpdatesValue()
        {
            _appStateVariable.Value = AppState.Playing;

            Assert.AreEqual(AppState.Playing, _appStateVariable.Value);
        }

        [Test]
        public void Value_SetNewValue_FiresOnValueChanged()
        {
            AppState _receivedValue = AppState.Menu;
            _appStateVariable.OnValueChanged += (value) => _receivedValue = value;

            _appStateVariable.Value = AppState.Playing;

            Assert.AreEqual(AppState.Playing, _receivedValue);
        }

        [Test]
        public void Value_SetSameValue_DoesNotFireOnValueChanged()
        {
            _appStateVariable.Value = AppState.Slideshow;

            bool _wasFired = false;
            _appStateVariable.OnValueChanged += (value) => _wasFired = true;

            _appStateVariable.Value = AppState.Slideshow;

            Assert.IsFalse(_wasFired);
        }

        [Test]
        public void ResetToInitial_AfterValueChanged_RestoresInitialValue()
        {
            SerializedObject so = new SerializedObject(_appStateVariable);
            so.FindProperty("initialValue").enumValueIndex = (int)AppState.Settings;
            so.ApplyModifiedPropertiesWithoutUndo();

            _appStateVariable.Value = AppState.Playing;
            _appStateVariable.ResetToInitial();

            Assert.AreEqual(AppState.Settings, _appStateVariable.Value);
        }
    }
}
