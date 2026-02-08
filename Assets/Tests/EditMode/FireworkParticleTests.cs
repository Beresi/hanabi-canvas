// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Tests.EditMode
{
    public class FireworkParticleTests
    {
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
            float[] durations = { 0.15f, 0.7f, 2.0f, 1.5f };

            for (int i = 0; i < 4; i++)
            {
                _phases[i] = ScriptableObject.CreateInstance<FireworkPhaseSO>();
                SerializedObject phaseSO = new SerializedObject(_phases[i]);
                phaseSO.FindProperty("_phaseName").stringValue = phaseNames[i];
                phaseSO.FindProperty("_duration").floatValue = durations[i];
                phaseSO.ApplyModifiedPropertiesWithoutUndo();
            }

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
            configSO.FindProperty("_debrisParticleCount").intValue = 10;
            configSO.FindProperty("_particleSize").floatValue = 0.1f;
            configSO.FindProperty("_particleSizeFadeMultiplier").floatValue = 0.5f;
            configSO.FindProperty("_formationScale").floatValue = 0.5f;
            configSO.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject pixelSO = new SerializedObject(_pixelData);
            pixelSO.FindProperty("_width").intValue = 8;
            pixelSO.FindProperty("_height").intValue = 8;
            pixelSO.ApplyModifiedPropertiesWithoutUndo();
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
        private FireworkInstance CreateFireworkInstance()
        {
            _fireworkObject = new GameObject("TestFirework");
            _fireworkInstance = _fireworkObject.AddComponent<FireworkInstance>();
            return _fireworkInstance;
        }

        private void AddTestPixels()
        {
            _pixelData.SetPixel(0, 0, new Color32(255, 0, 0, 255));
            _pixelData.SetPixel(4, 4, new Color32(0, 255, 0, 255));
            _pixelData.SetPixel(7, 7, new Color32(0, 0, 255, 255));
        }

        // ---- ParticleData Tests ----
        [Test]
        public void ParticleData_Default_HasZeroValues()
        {
            ParticleData data = new ParticleData();

            Assert.AreEqual(Vector3.zero, data.Position);
            Assert.AreEqual(Vector3.zero, data.Velocity);
            Assert.AreEqual(0f, data.Size);
            Assert.AreEqual(0f, data.Life);
            Assert.IsFalse(data.IsPattern);
        }

        // ---- Coordinate Mapping Tests ----
        [Test]
        public void FormationTarget_CenterPixel_MapsNearOrigin()
        {
            _pixelData.SetPixel(4, 4, new Color32(255, 0, 0, 255));

            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, Vector3.zero, _activeFireworks);

            ParticleData particle = instance.Particles[0];
            float expectedX = (4 - 8 / 2f) * 0.5f;
            float expectedY = (4 - 8 / 2f) * 0.5f;

            Assert.AreEqual(expectedX, particle.FormationTarget.x, 0.001f);
            Assert.AreEqual(expectedY, particle.FormationTarget.y, 0.001f);
            Assert.AreEqual(0f, particle.FormationTarget.z, 0.001f);
        }

        [Test]
        public void FormationTarget_CornerPixel_MapsCorrectly()
        {
            _pixelData.SetPixel(0, 0, new Color32(255, 0, 0, 255));

            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, Vector3.zero, _activeFireworks);

            ParticleData particle = instance.Particles[0];
            float expectedX = (0 - 8 / 2f) * 0.5f;
            float expectedY = (0 - 8 / 2f) * 0.5f;

            Assert.AreEqual(expectedX, particle.FormationTarget.x, 0.001f);
            Assert.AreEqual(expectedY, particle.FormationTarget.y, 0.001f);
        }

        [Test]
        public void FormationTarget_WithOriginOffset_OffsetsCorrectly()
        {
            _pixelData.SetPixel(4, 4, new Color32(255, 0, 0, 255));
            Vector3 origin = new Vector3(10f, 20f, 0f);

            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, origin, _activeFireworks);

            ParticleData particle = instance.Particles[0];
            float expectedX = origin.x + (4 - 8 / 2f) * 0.5f;
            float expectedY = origin.y + (4 - 8 / 2f) * 0.5f;

            Assert.AreEqual(expectedX, particle.FormationTarget.x, 0.001f);
            Assert.AreEqual(expectedY, particle.FormationTarget.y, 0.001f);
        }

        // ---- FireworkUpdater.UpdateBurst Tests ----
        [Test]
        public void UpdateBurst_MovesParticleAlongVelocity()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = Vector3.zero,
                Velocity = new Vector3(10f, 0f, 0f),
                Life = 1f,
                Size = 0.1f
            };

            FireworkUpdater.UpdateBurst(particles, 1, 0.1f, 0.98f);

            Assert.Greater(particles[0].Position.x, 0f);
        }

        [Test]
        public void UpdateBurst_AppliesDrag()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = Vector3.zero,
                Velocity = new Vector3(10f, 0f, 0f),
                Life = 1f,
                Size = 0.1f
            };

            float initialSpeed = particles[0].Velocity.magnitude;
            FireworkUpdater.UpdateBurst(particles, 1, 0.1f, 0.98f);

            Assert.Less(particles[0].Velocity.magnitude, initialSpeed);
        }

        // ---- FireworkUpdater.UpdateSteer Tests ----
        [Test]
        public void UpdateSteer_PatternParticle_SteersTowardTarget()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = new Vector3(5f, 0f, 0f),
                Velocity = new Vector3(1f, 0f, 0f),
                FormationTarget = Vector3.zero,
                IsPattern = true,
                Life = 1f,
                Size = 0.1f
            };

            AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            FireworkUpdater.UpdateSteer(particles, 1, 0.1f, 8f, curve, 0.5f, 0.95f);

            Assert.Less(particles[0].Position.x, 5f,
                "Pattern particle should move toward formation target");
        }

        [Test]
        public void UpdateSteer_DebrisParticle_DoesNotSteer()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = Vector3.zero,
                Velocity = new Vector3(10f, 0f, 0f),
                FormationTarget = new Vector3(-100f, 0f, 0f),
                IsPattern = false,
                Life = 1f,
                Size = 0.1f
            };

            AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            FireworkUpdater.UpdateSteer(particles, 1, 0.1f, 8f, curve, 1f, 0.95f);

            Assert.Greater(particles[0].Position.x, 0f,
                "Debris particle should continue moving in its velocity direction");
        }

        [Test]
        public void UpdateSteer_DebrisParticle_FadesLife()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = Vector3.zero,
                Velocity = Vector3.right,
                IsPattern = false,
                Life = 1f,
                Size = 0.1f
            };

            AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            FireworkUpdater.UpdateSteer(particles, 1, 0.1f, 8f, curve, 0.5f, 0.95f);

            Assert.Less(particles[0].Life, 1f);
        }

        // ---- FireworkUpdater.UpdateHold Tests ----
        [Test]
        public void UpdateHold_PatternParticle_SnapsNearFormation()
        {
            Vector3 target = new Vector3(5f, 5f, 0f);
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = new Vector3(10f, 10f, 0f),
                Velocity = new Vector3(1f, 1f, 0f),
                FormationTarget = target,
                IsPattern = true,
                Life = 1f,
                Size = 0.1f
            };

            FireworkUpdater.UpdateHold(particles, 1, 0.1f, 1f, 0.01f, 0f);

            float distance = Vector3.Distance(particles[0].Position, target);
            Assert.Less(distance, 0.1f, "Pattern particle should snap near formation target");
        }

        [Test]
        public void UpdateHold_DebrisParticle_FadesLife()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = Vector3.zero,
                Velocity = Vector3.right,
                IsPattern = false,
                Life = 1f,
                Size = 0.1f
            };

            FireworkUpdater.UpdateHold(particles, 1, 0.1f, 1f, 0.01f, 0f);

            Assert.Less(particles[0].Life, 1f);
        }

        // ---- FireworkUpdater.UpdateFade Tests ----
        [Test]
        public void UpdateFade_AppliesGravity()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = new Vector3(0f, 10f, 0f),
                Velocity = Vector3.zero,
                Life = 1f,
                Size = 0.1f,
                Color = Color.white
            };

            FireworkUpdater.UpdateFade(particles, 1, 0.1f, 2f, 0.5f, 1.5f);

            Assert.Less(particles[0].Velocity.y, 0f, "Gravity should pull velocity downward");
            Assert.Less(particles[0].Position.y, 10f, "Position should move down");
        }

        [Test]
        public void UpdateFade_ShrinksSizeByMultiplier()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Life = 1f,
                Size = 1f,
                Color = Color.white
            };

            FireworkUpdater.UpdateFade(particles, 1, 0.1f, 0f, 0.5f, 1.5f);

            Assert.Less(particles[0].Size, 1f, "Size should shrink during fade");
        }

        [Test]
        public void UpdateFade_FadesAlpha()
        {
            ParticleData[] particles = new ParticleData[1];
            particles[0] = new ParticleData
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Life = 1f,
                Size = 0.1f,
                Color = Color.white
            };

            FireworkUpdater.UpdateFade(particles, 1, 0.1f, 0f, 0f, 1.5f);

            Assert.Less(particles[0].Color.a, 1f, "Alpha should decrease during fade");
        }

        // ---- FireworkInstance.Initialize Tests ----
        [Test]
        public void Initialize_CreatesCorrectParticleCount()
        {
            AddTestPixels();

            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, Vector3.zero, _activeFireworks);

            int expectedCount = _pixelData.PixelCount + _config.DebrisParticleCount;
            Assert.AreEqual(expectedCount, instance.ParticleCount);
        }

        [Test]
        public void Initialize_PatternParticlesHaveCorrectFormationTargets()
        {
            AddTestPixels();

            FireworkInstance instance = CreateFireworkInstance();
            Vector3 origin = Vector3.zero;
            instance.Initialize(_config, _pixelData, origin, _activeFireworks);

            float halfWidth = _pixelData.Width / 2f;
            float halfHeight = _pixelData.Height / 2f;
            float scale = _config.FormationScale;

            for (int i = 0; i < _pixelData.PixelCount; i++)
            {
                Assert.IsTrue(instance.Particles[i].IsPattern,
                    $"Particle {i} should be a pattern particle");

                PixelEntry entry = _pixelData.GetPixelAt(i);
                float expectedX = origin.x + (entry.X - halfWidth) * scale;
                float expectedY = origin.y + (entry.Y - halfHeight) * scale;
                Vector3 expected = new Vector3(expectedX, expectedY, 0f);

                Assert.AreEqual(expected.x, instance.Particles[i].FormationTarget.x, 0.001f,
                    $"Pattern particle {i} formation X mismatch");
                Assert.AreEqual(expected.y, instance.Particles[i].FormationTarget.y, 0.001f,
                    $"Pattern particle {i} formation Y mismatch");
            }
        }

        [Test]
        public void Initialize_DebrisParticlesAreNotPattern()
        {
            AddTestPixels();

            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, Vector3.zero, _activeFireworks);

            int patternCount = _pixelData.PixelCount;
            for (int i = patternCount; i < instance.ParticleCount; i++)
            {
                Assert.IsFalse(instance.Particles[i].IsPattern,
                    $"Particle {i} should be a debris particle");
            }
        }

        [Test]
        public void Initialize_AllParticlesStartAtOrigin()
        {
            AddTestPixels();
            Vector3 origin = new Vector3(5f, 10f, 0f);

            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, origin, _activeFireworks);

            for (int i = 0; i < instance.ParticleCount; i++)
            {
                Assert.AreEqual(origin, instance.Particles[i].Position,
                    $"Particle {i} should start at origin");
            }
        }

        [Test]
        public void Initialize_AllParticlesHaveBurstVelocity()
        {
            AddTestPixels();

            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, Vector3.zero, _activeFireworks);

            for (int i = 0; i < instance.ParticleCount; i++)
            {
                Assert.Greater(instance.Particles[i].Velocity.magnitude, 0f,
                    $"Particle {i} should have non-zero initial velocity");
            }
        }

        [Test]
        public void Initialize_NoPixels_OnlyDebris()
        {
            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, Vector3.zero, _activeFireworks);

            Assert.AreEqual(_config.DebrisParticleCount, instance.ParticleCount);

            for (int i = 0; i < instance.ParticleCount; i++)
            {
                Assert.IsFalse(instance.Particles[i].IsPattern);
            }
        }

        [Test]
        public void Initialize_StartsAtPhaseZero()
        {
            AddTestPixels();

            FireworkInstance instance = CreateFireworkInstance();
            instance.Initialize(_config, _pixelData, Vector3.zero, _activeFireworks);

            Assert.AreEqual(0, instance.CurrentPhaseIndex);
            Assert.IsFalse(instance.IsComplete);
        }

        // ---- PixelDataSO.GetPixelAt Tests ----
        [Test]
        public void GetPixelAt_ValidIndex_ReturnsCorrectPixel()
        {
            _pixelData.SetPixel(3, 5, new Color32(255, 128, 0, 255));
            _pixelData.SetPixel(7, 2, new Color32(0, 255, 0, 255));

            PixelEntry first = _pixelData.GetPixelAt(0);
            Assert.AreEqual(3, first.X);
            Assert.AreEqual(5, first.Y);
            Assert.AreEqual(255, first.Color.r);

            PixelEntry second = _pixelData.GetPixelAt(1);
            Assert.AreEqual(7, second.X);
            Assert.AreEqual(2, second.Y);
        }
    }
}
