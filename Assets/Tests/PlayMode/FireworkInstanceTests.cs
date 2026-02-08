// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Firework;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HanabiCanvas.Tests.PlayMode
{
    public class FireworkInstanceTests
    {
        // ---- Constants ----
        private const float SHORT_PHASE_DURATION = 0.05f;

        // ---- Private Fields ----
        private FireworkConfigSO _config;
        private FireworkPhaseSO[] _phases;
        private PixelDataSO _pixelData;
        private FireworkInstanceListSO _activeFireworks;
        private GameObject _fireworkObject;
        private FireworkInstance _fireworkInstance;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _config = ScriptableObject.CreateInstance<FireworkConfigSO>();
            _pixelData = ScriptableObject.CreateInstance<PixelDataSO>();
            _activeFireworks = ScriptableObject.CreateInstance<FireworkInstanceListSO>();

            _phases = new FireworkPhaseSO[4];
            string[] phaseNames = { "Burst", "Steer", "Hold", "Fade" };

            for (int i = 0; i < 4; i++)
            {
                _phases[i] = ScriptableObject.CreateInstance<FireworkPhaseSO>();

#if UNITY_EDITOR
                SerializedObject phaseSO = new SerializedObject(_phases[i]);
                phaseSO.FindProperty("_phaseName").stringValue = phaseNames[i];
                phaseSO.FindProperty("_duration").floatValue = SHORT_PHASE_DURATION;
                phaseSO.ApplyModifiedPropertiesWithoutUndo();
#endif
            }

#if UNITY_EDITOR
            SerializedObject configSO = new SerializedObject(_config);
            SerializedProperty phasesProp = configSO.FindProperty("_phases");
            phasesProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                phasesProp.GetArrayElementAtIndex(i).objectReferenceValue = _phases[i];
            }
            configSO.FindProperty("_burstRadius").floatValue = 5f;
            configSO.FindProperty("_burstDrag").floatValue = 0.98f;
            configSO.FindProperty("_steerStrength").floatValue = 8f;
            configSO.FindProperty("_steerDebrisDrag").floatValue = 0.95f;
            configSO.FindProperty("_holdSparkleIntensity").floatValue = 1f;
            configSO.FindProperty("_holdJitterScale").floatValue = 0.01f;
            configSO.FindProperty("_fadeGravity").floatValue = 2f;
            configSO.FindProperty("_debrisParticleCount").intValue = 5;
            configSO.FindProperty("_particleSize").floatValue = 0.1f;
            configSO.FindProperty("_particleSizeFadeMultiplier").floatValue = 0.5f;
            configSO.FindProperty("_formationScale").floatValue = 0.5f;
            configSO.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject pixelSO = new SerializedObject(_pixelData);
            pixelSO.FindProperty("_width").intValue = 8;
            pixelSO.FindProperty("_height").intValue = 8;
            pixelSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _pixelData.SetPixel(2, 2, new Color32(255, 0, 0, 255));
            _pixelData.SetPixel(5, 5, new Color32(0, 255, 0, 255));
        }

        [TearDown]
        public void Teardown()
        {
            if (_fireworkObject != null)
            {
                Object.DestroyImmediate(_fireworkObject);
            }
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_pixelData);
            Object.DestroyImmediate(_activeFireworks);
            for (int i = 0; i < _phases.Length; i++)
            {
                if (_phases[i] != null)
                {
                    Object.DestroyImmediate(_phases[i]);
                }
            }
        }

        // ---- Helper Methods ----
        private FireworkInstance CreateAndInitializeFirework()
        {
            _fireworkObject = new GameObject("TestFirework");
            _fireworkInstance = _fireworkObject.AddComponent<FireworkInstance>();
            _fireworkInstance.Initialize(_config, _pixelData, Vector3.zero, _activeFireworks);
            return _fireworkInstance;
        }

        // ---- Phase Transition Tests ----
        [UnityTest]
        public IEnumerator Update_AfterBurstDuration_AdvancesToNextPhase()
        {
            CreateAndInitializeFirework();
            yield return null;

            Assert.AreEqual(0, _fireworkInstance.CurrentPhaseIndex);

            yield return new WaitForSeconds(SHORT_PHASE_DURATION + 0.02f);

            Assert.AreEqual(1, _fireworkInstance.CurrentPhaseIndex,
                "Should advance from Burst to Steer");
        }

        [UnityTest]
        public IEnumerator Update_AfterAllPhases_IsComplete()
        {
            CreateAndInitializeFirework();
            yield return null;

            float totalDuration = SHORT_PHASE_DURATION * 4f + 0.2f;
            yield return new WaitForSeconds(totalDuration);

            Assert.IsTrue(_fireworkInstance == null || _fireworkInstance.IsComplete,
                "Firework should be complete after all phases");
        }

        // ---- Particle Movement Tests ----
        [UnityTest]
        public IEnumerator Update_BurstPhase_ParticlesMove()
        {
            CreateAndInitializeFirework();
            yield return null;

            Vector3[] initialPositions = new Vector3[_fireworkInstance.ParticleCount];
            for (int i = 0; i < _fireworkInstance.ParticleCount; i++)
            {
                initialPositions[i] = _fireworkInstance.Particles[i].Position;
            }

            yield return null;

            bool anyMoved = false;
            for (int i = 0; i < _fireworkInstance.ParticleCount; i++)
            {
                if (_fireworkInstance.Particles[i].Position != initialPositions[i])
                {
                    anyMoved = true;
                    break;
                }
            }

            Assert.IsTrue(anyMoved, "At least some particles should move during Burst phase");
        }

        // ---- Self-Registration Tests ----
        [UnityTest]
        public IEnumerator OnEnable_RegistersInActiveFireworks()
        {
            _fireworkObject = new GameObject("TestFirework");
            _fireworkObject.SetActive(false);
            _fireworkInstance = _fireworkObject.AddComponent<FireworkInstance>();

#if UNITY_EDITOR
            SerializedObject instanceSO = new SerializedObject(_fireworkInstance);
            instanceSO.FindProperty("_activeFireworks").objectReferenceValue = _activeFireworks;
            instanceSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _fireworkObject.SetActive(true);
            yield return null;

            Assert.AreEqual(1, _activeFireworks.Count,
                "FireworkInstance should register in active fireworks list on enable");
        }

        [UnityTest]
        public IEnumerator OnDisable_UnregistersFromActiveFireworks()
        {
            _fireworkObject = new GameObject("TestFirework");
            _fireworkObject.SetActive(false);
            _fireworkInstance = _fireworkObject.AddComponent<FireworkInstance>();

#if UNITY_EDITOR
            SerializedObject instanceSO = new SerializedObject(_fireworkInstance);
            instanceSO.FindProperty("_activeFireworks").objectReferenceValue = _activeFireworks;
            instanceSO.ApplyModifiedPropertiesWithoutUndo();
#endif

            _fireworkObject.SetActive(true);
            yield return null;

            Assert.AreEqual(1, _activeFireworks.Count);

            _fireworkObject.SetActive(false);

            Assert.AreEqual(0, _activeFireworks.Count,
                "FireworkInstance should unregister from active fireworks on disable");
        }

        // ---- Initialization Tests ----
        [UnityTest]
        public IEnumerator Initialize_WithPixels_CreatesCorrectCount()
        {
            CreateAndInitializeFirework();
            yield return null;

            int expectedCount = _pixelData.PixelCount + _config.DebrisParticleCount;
            Assert.AreEqual(expectedCount, _fireworkInstance.ParticleCount);
        }

        [UnityTest]
        public IEnumerator Initialize_StartsNotComplete()
        {
            CreateAndInitializeFirework();
            yield return null;

            Assert.IsFalse(_fireworkInstance.IsComplete);
        }
    }
}
