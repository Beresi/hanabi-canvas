// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Tests.EditMode
{
    public class GameEventSOTests
    {
        // ---- Private Types ----
        private class IntGameEventSO : GameEventSO<int> { }

        // ---- Private Fields ----
        private GameEventSO _gameEvent;
        private IntGameEventSO _intGameEvent;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _gameEvent = ScriptableObject.CreateInstance<GameEventSO>();
            _intGameEvent = ScriptableObject.CreateInstance<IntGameEventSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameEvent);
            Object.DestroyImmediate(_intGameEvent);
        }

        // ---- Tests (Parameterless) ----
        [Test]
        public void Raise_WithRegisteredListener_InvokesListener()
        {
            bool _wasInvoked = false;
            _gameEvent.Register(() => _wasInvoked = true);

            _gameEvent.Raise();

            Assert.IsTrue(_wasInvoked);
        }

        [Test]
        public void Raise_AfterUnregister_DoesNotInvokeListener()
        {
            bool _wasInvoked = false;
            System.Action listener = () => _wasInvoked = true;
            _gameEvent.Register(listener);
            _gameEvent.Unregister(listener);

            _gameEvent.Raise();

            Assert.IsFalse(_wasInvoked);
        }

        [Test]
        public void Raise_WithMultipleListeners_InvokesAll()
        {
            int _invokeCount = 0;
            _gameEvent.Register(() => _invokeCount++);
            _gameEvent.Register(() => _invokeCount++);
            _gameEvent.Register(() => _invokeCount++);

            _gameEvent.Raise();

            Assert.AreEqual(3, _invokeCount);
        }

        [Test]
        public void Raise_WithNoListeners_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _gameEvent.Raise());
        }

        [Test]
        public void Raise_ListenerUnregistersDuringRaise_DoesNotThrow()
        {
            System.Action listener = null;
            listener = () => _gameEvent.Unregister(listener);
            _gameEvent.Register(listener);

            Assert.DoesNotThrow(() => _gameEvent.Raise());
        }

        // ---- Tests (Typed) ----
        [Test]
        public void Raise_TypedEvent_PassesValueToListener()
        {
            int _receivedValue = 0;
            _intGameEvent.Register((value) => _receivedValue = value);

            _intGameEvent.Raise(42);

            Assert.AreEqual(42, _receivedValue);
        }

        [Test]
        public void Raise_TypedEvent_AfterUnregister_DoesNotInvoke()
        {
            int _receivedValue = 0;
            System.Action<int> listener = (value) => _receivedValue = value;
            _intGameEvent.Register(listener);
            _intGameEvent.Unregister(listener);

            _intGameEvent.Raise(42);

            Assert.AreEqual(0, _receivedValue);
        }
    }
}
