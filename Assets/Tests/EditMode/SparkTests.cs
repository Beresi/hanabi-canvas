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
    public class SparkTests
    {
        // ---- Private Fields ----
        private BurstSparkBehaviourSO _behaviour;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _behaviour = ScriptableObject.CreateInstance<BurstSparkBehaviourSO>();

            SerializedObject behaviourSO = new SerializedObject(_behaviour);
            behaviourSO.FindProperty("_particleCount").intValue = 50;
            behaviourSO.FindProperty("_burstSpeed").floatValue = 10f;
            behaviourSO.FindProperty("_burstSpeedVariance").floatValue = 0f;
            behaviourSO.FindProperty("_gravity").floatValue = 5f;
            behaviourSO.FindProperty("_drag").floatValue = 0.95f;
            behaviourSO.FindProperty("_lifetime").floatValue = 2f;
            behaviourSO.FindProperty("_lifetimeVariance").floatValue = 0f;
            behaviourSO.FindProperty("_particleSize").floatValue = 0.2f;
            behaviourSO.FindProperty("_sizeOverLife").animationCurveValue =
                AnimationCurve.Linear(0f, 1f, 1f, 0f);
            behaviourSO.FindProperty("_alphaOverLife").animationCurveValue =
                AnimationCurve.Linear(0f, 1f, 1f, 0f);
            behaviourSO.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_behaviour);
        }

        // ---- Helper Methods ----
        private SparkParticle CreateAliveParticle(Vector3 position, Vector3 velocity, float life = 1f)
        {
            return new SparkParticle
            {
                Position = position,
                Velocity = velocity,
                Color = Color.white,
                Size = 0.2f,
                Life = life,
                MaxLife = life
            };
        }

        // ---- SparkParticle Tests ----
        [Test]
        public void SparkParticle_Default_HasZeroValues()
        {
            SparkParticle particle = new SparkParticle();

            Assert.AreEqual(Vector3.zero, particle.Position);
            Assert.AreEqual(Vector3.zero, particle.Velocity);
            Assert.AreEqual(0f, particle.Size);
            Assert.AreEqual(0f, particle.Life);
            Assert.AreEqual(0f, particle.MaxLife);
        }

        // ---- BurstSparkBehaviourSO Validation Tests ----
        [Test]
        public void OnValidate_ParticleCountBelowMin_ClampedToMin()
        {
            BurstSparkBehaviourSO testBehaviour = ScriptableObject.CreateInstance<BurstSparkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_particleCount").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(1, testBehaviour.ParticleCount);

            Object.DestroyImmediate(testBehaviour);
        }

        [Test]
        public void OnValidate_BurstSpeedBelowMin_ClampedToMin()
        {
            BurstSparkBehaviourSO testBehaviour = ScriptableObject.CreateInstance<BurstSparkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_burstSpeed").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.1f, testBehaviour.BurstSpeed, 0.001f);

            Object.DestroyImmediate(testBehaviour);
        }

        [Test]
        public void OnValidate_LifetimeBelowMin_ClampedToMin()
        {
            BurstSparkBehaviourSO testBehaviour = ScriptableObject.CreateInstance<BurstSparkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_lifetime").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.1f, testBehaviour.Lifetime, 0.001f);

            Object.DestroyImmediate(testBehaviour);
        }

        [Test]
        public void OnValidate_ParticleSizeBelowMin_ClampedToMin()
        {
            BurstSparkBehaviourSO testBehaviour = ScriptableObject.CreateInstance<BurstSparkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_particleSize").floatValue = -1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.01f, testBehaviour.ParticleSize, 0.001f);

            Object.DestroyImmediate(testBehaviour);
        }

        // ---- BurstSparkBehaviourSO UpdateParticles Tests ----
        [Test]
        public void UpdateParticles_DeadParticle_IsSkipped()
        {
            SparkParticle[] particles = new SparkParticle[1];
            particles[0] = new SparkParticle
            {
                Position = Vector3.zero,
                Velocity = new Vector3(10f, 0f, 0f),
                Color = Color.white,
                Size = 0.2f,
                Life = 0f,
                MaxLife = 1f
            };

            _behaviour.UpdateParticles(particles, 1, 0.1f);

            Assert.AreEqual(Vector3.zero, particles[0].Position,
                "Dead particle should not be moved");
        }

        [Test]
        public void UpdateParticles_Gravity_PullsVelocityDown()
        {
            SparkParticle[] particles = new SparkParticle[1];
            particles[0] = CreateAliveParticle(new Vector3(0f, 10f, 0f), Vector3.zero);

            _behaviour.UpdateParticles(particles, 1, 0.1f);

            Assert.Less(particles[0].Velocity.y, 0f,
                "Gravity should pull velocity downward");
        }

        [Test]
        public void UpdateParticles_Drag_ReducesSpeed()
        {
            SparkParticle[] particles = new SparkParticle[1];
            particles[0] = CreateAliveParticle(Vector3.zero, new Vector3(10f, 0f, 0f));

            float initialMagnitude = particles[0].Velocity.magnitude;

            _behaviour.UpdateParticles(particles, 1, 0.1f);

            Assert.Less(particles[0].Velocity.magnitude, initialMagnitude,
                "Drag should reduce velocity magnitude");
        }

        [Test]
        public void UpdateParticles_PositionIntegration_MovesParticle()
        {
            SparkParticle[] particles = new SparkParticle[1];
            particles[0] = CreateAliveParticle(Vector3.zero, new Vector3(10f, 0f, 0f));

            _behaviour.UpdateParticles(particles, 1, 0.1f);

            Assert.Greater(particles[0].Position.x, 0f,
                "Particle should move in the direction of its velocity");
        }

        [Test]
        public void UpdateParticles_LifeDecay_ReducesLife()
        {
            SparkParticle[] particles = new SparkParticle[1];
            particles[0] = CreateAliveParticle(Vector3.zero, Vector3.zero);

            _behaviour.UpdateParticles(particles, 1, 0.1f);

            Assert.Less(particles[0].Life, 1f,
                "Life should decrease after update");
        }

        [Test]
        public void UpdateParticles_LifeDecay_ClampsToZero()
        {
            SparkParticle[] particles = new SparkParticle[1];
            particles[0] = CreateAliveParticle(Vector3.zero, Vector3.zero, 0.05f);

            _behaviour.UpdateParticles(particles, 1, 0.1f);

            Assert.AreEqual(0f, particles[0].Life,
                "Life should be clamped to zero, not negative");
        }

        [Test]
        public void UpdateParticles_SizeOverLife_ScalesSizeByProgress()
        {
            SparkParticle[] particles = new SparkParticle[1];
            particles[0] = new SparkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f
            };

            // dt=1 -> life goes from 2 to 1, progress = (2-1)/2 = 0.5
            // size = baseSize * sizeCurve(0.5) = 0.2 * 0.5 = 0.1
            _behaviour.UpdateParticles(particles, 1, 1f);

            Assert.AreEqual(0.1f, particles[0].Size, 0.01f,
                "Size should be baseSize * sizeOverLife curve value at progress 0.5");
        }

        [Test]
        public void UpdateParticles_AlphaOverLife_FadesAlpha()
        {
            SparkParticle[] particles = new SparkParticle[1];
            particles[0] = new SparkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f
            };

            // dt=1 -> life goes from 2 to 1, progress = (2-1)/2 = 0.5
            _behaviour.UpdateParticles(particles, 1, 1f);

            Assert.Less(particles[0].Color.a, 1f,
                "Alpha should be less than 1 after partial lifetime");
            Assert.AreEqual(0.5f, particles[0].Color.a, 0.01f,
                "Alpha should match alphaOverLife curve value at progress 0.5");
        }

        // ---- BurstSparkBehaviourSO GetParticleCount / IsComplete Tests ----
        [Test]
        public void GetParticleCount_ReturnsConfiguredCount()
        {
            SparkRequest request = new SparkRequest
            {
                Position = Vector3.zero,
                Pattern = new[] { new PixelEntry(0, 0, new Color32(255, 0, 0, 255)) }
            };

            Assert.AreEqual(50, _behaviour.GetParticleCount(request));
        }

        [Test]
        public void IsComplete_AllDead_ReturnsTrue()
        {
            SparkParticle[] particles = new SparkParticle[3];
            // All default with Life = 0

            Assert.IsTrue(_behaviour.IsComplete(particles, 3));
        }

        [Test]
        public void IsComplete_SomeAlive_ReturnsFalse()
        {
            SparkParticle[] particles = new SparkParticle[3];
            particles[1].Life = 0.5f;

            Assert.IsFalse(_behaviour.IsComplete(particles, 3));
        }

        [Test]
        public void InitializeParticles_SetsPositionAndLife()
        {
            SparkParticle[] particles = new SparkParticle[5];
            PixelEntry[] pattern = new[]
            {
                new PixelEntry(0, 0, new Color32(255, 0, 0, 255)),
                new PixelEntry(1, 0, new Color32(255, 0, 0, 255))
            };
            SparkRequest request = new SparkRequest
            {
                Position = new Vector3(1f, 2f, 3f),
                Pattern = pattern
            };

            _behaviour.InitializeParticles(particles, 5, request);

            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(request.Position, particles[i].Position,
                    $"Particle {i} should be at request position");
                Assert.Greater(particles[i].Life, 0f,
                    $"Particle {i} should have positive life");
                Assert.AreEqual(particles[i].Life, particles[i].MaxLife,
                    $"Particle {i} Life should equal MaxLife");
            }
        }

        [Test]
        public void InitializeParticles_WithPattern_UsesPatternColors()
        {
            SparkParticle[] particles = new SparkParticle[10];
            // All pixels are the same color — every particle should get that color
            Color32 red = new Color32(255, 0, 0, 255);
            PixelEntry[] pattern = new[]
            {
                new PixelEntry(0, 0, red),
                new PixelEntry(1, 0, red),
                new PixelEntry(2, 0, red)
            };
            SparkRequest request = new SparkRequest
            {
                Position = Vector3.zero,
                Pattern = pattern
            };

            _behaviour.InitializeParticles(particles, 10, request);

            Color expectedColor = new Color(1f, 0f, 0f, 1f);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(expectedColor.r, particles[i].Color.r, 0.01f,
                    $"Particle {i} R channel should match pattern color");
                Assert.AreEqual(expectedColor.g, particles[i].Color.g, 0.01f,
                    $"Particle {i} G channel should match pattern color");
                Assert.AreEqual(expectedColor.b, particles[i].Color.b, 0.01f,
                    $"Particle {i} B channel should match pattern color");
            }
        }

        [Test]
        public void InitializeParticles_NoPattern_UsesWhite()
        {
            SparkParticle[] particles = new SparkParticle[5];
            SparkRequest request = new SparkRequest
            {
                Position = Vector3.zero,
                Pattern = null
            };

            _behaviour.InitializeParticles(particles, 5, request);

            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(Color.white, particles[i].Color,
                    $"Particle {i} should be white when no pattern provided");
            }
        }
    }
}
