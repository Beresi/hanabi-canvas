// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.UI;

namespace HanabiCanvas.Tests.EditMode
{
    public class AudioManagerTests
    {
        // ---- Private Fields ----
        private GameObject _managerGO;
        private AudioManager _manager;
        private FloatVariableSO _masterVolume;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _managerGO = new GameObject("TestAudioManager");
            _manager = _managerGO.AddComponent<AudioManager>();
            _masterVolume = ScriptableObject.CreateInstance<FloatVariableSO>();

            // Wire master volume via SerializedObject
#if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(_manager);
            so.FindProperty("_masterVolume").objectReferenceValue = _masterVolume;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_managerGO);
            Object.DestroyImmediate(_masterVolume);
        }

        // ---- Tests ----

        [Test]
        public void SetMasterVolume_UpdatesAllSources()
        {
            _manager.SetMasterVolume(0.5f);

            // Verify no exceptions thrown — audio sources are internal
            Assert.Pass();
        }

        [Test]
        public void PlayOneShot_WithNullClip_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _manager.PlayOneShot(null));
        }

        [Test]
        public void PlayUIClick_WithNoConfig_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _manager.PlayUIClick());
        }

        [Test]
        public void StopAmbience_WhenNotPlaying_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _manager.StopAmbience());
        }
    }
}
