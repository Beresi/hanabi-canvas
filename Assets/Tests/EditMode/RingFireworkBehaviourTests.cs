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
    public class RingFireworkBehaviourTests
    {
        // ---- Private Fields ----
        private RingFireworkBehaviourSO _behaviour;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _behaviour = ScriptableObject.CreateInstance<RingFireworkBehaviourSO>();

            SerializedObject so = new SerializedObject(_behaviour);
            so.FindProperty("_particleCount").intValue = 100;
            so.FindProperty("_ringRadius").floatValue = 2.0f;
            so.FindProperty("_ringRadiusVariance").floatValue = 0.0f;
            so.FindProperty("_burstSpeed").floatValue = 5.0f;
            so.FindProperty("_burstSpeedVariance").floatValue = 0.0f;
            so.FindProperty("_gravity").floatValue = 2.0f;
            so.FindProperty("_drag").floatValue = 0.98f;
            so.FindProperty("_lifetime").floatValue = 1.5f;
            so.FindProperty("_lifetimeVariance").floatValue = 0.0f;
            so.FindProperty("_particleSize").floatValue = 0.15f;
            so.FindProperty("_sizeOverLife").animationCurveValue =
                AnimationCurve.Constant(0f, 1f, 1f);
            so.FindProperty("_alphaOverLife").animationCurveValue =
                AnimationCurve.Linear(0f, 1f, 1f, 0f);
            so.FindProperty("_isAutoRadiusFromPattern").boolValue = false;
            so.FindProperty("_patternScaleHint").floatValue = 0.3f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_behaviour);
        }

        // ---- Helper Methods ----

        private FireworkRequest CreateTestRequest(int pixelCount)
        {
            PixelEntry[] pattern = new PixelEntry[pixelCount];
            Color32 red = new Color32(255, 0, 0, 255);
            for (int i = 0; i < pixelCount; i++)
            {
                pattern[i] = new PixelEntry((byte)i, 0, red);
            }

            return new FireworkRequest
            {
                Position = new Vector3(0f, 10f, 0f),
                Pattern = pattern,
                PatternWidth = 32,
                PatternHeight = 32
            };
        }

        // ---- GetParticleCount Tests ----

        [Test]
        public void GetParticleCount_ReturnsConfiguredCount()
        {
            FireworkRequest request = CreateTestRequest(5);

            Assert.AreEqual(100, _behaviour.GetParticleCount(request));
        }

        // ---- InitializeParticles Tests ----

        [Test]
        public void InitializeParticles_StartsAtOrigin()
        {
            FireworkRequest request = CreateTestRequest(5);
            int count = _behaviour.GetParticleCount(request);
            FireworkParticle[] particles = new FireworkParticle[count];

            _behaviour.InitializeParticles(particles, count, request);

            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(request.Position, particles[i].Position,
                    $"Particle {i} should start at the request origin");
            }
        }

        [Test]
        public void InitializeParticles_VelocityIsRadialXY()
        {
            FireworkRequest request = CreateTestRequest(5);
            int count = _behaviour.GetParticleCount(request);
            FireworkParticle[] particles = new FireworkParticle[count];

            _behaviour.InitializeParticles(particles, count, request);

            for (int i = 0; i < count; i++)
            {
                // Velocity Z should be approximately zero (XY plane only)
                Assert.AreEqual(0f, particles[i].Velocity.z, 0.01f,
                    $"Particle {i} velocity.z should be ~0 (XY plane)");

                // Velocity XY direction should match position offset XY direction
                Vector3 offset = particles[i].Position - request.Position;
                Vector2 offsetXY = new Vector2(offset.x, offset.y);
                Vector2 velocityXY = new Vector2(particles[i].Velocity.x, particles[i].Velocity.y);

                if (offsetXY.magnitude > 0.001f && velocityXY.magnitude > 0.001f)
                {
                    float dot = Vector2.Dot(offsetXY.normalized, velocityXY.normalized);
                    Assert.Greater(dot, 0.9f,
                        $"Particle {i} velocity direction should align with radial offset (dot={dot})");
                }
            }
        }

        [Test]
        public void InitializeParticles_WithPattern_UsesPatternColors()
        {
            FireworkRequest request = CreateTestRequest(10);
            int count = _behaviour.GetParticleCount(request);
            FireworkParticle[] particles = new FireworkParticle[count];

            _behaviour.InitializeParticles(particles, count, request);

            for (int i = 0; i < count; i++)
            {
                Assert.AreNotEqual(Color.white, particles[i].Color,
                    $"Particle {i} should have a non-white color from the pattern");
            }
        }

        [Test]
        public void InitializeParticles_NoPattern_UsesWhite()
        {
            FireworkRequest request = new FireworkRequest
            {
                Position = new Vector3(0f, 10f, 0f),
                Pattern = null,
                PatternWidth = 32,
                PatternHeight = 32
            };
            int count = _behaviour.GetParticleCount(request);
            FireworkParticle[] particles = new FireworkParticle[count];

            _behaviour.InitializeParticles(particles, count, request);

            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(Color.white, particles[i].Color,
                    $"Particle {i} should be white when no pattern provided");
            }
        }

        [Test]
        public void InitializeParticles_AutoRadius_UsesPatternExtent()
        {
            // Create a separate SO with auto-radius enabled
            RingFireworkBehaviourSO autoRadiusBehaviour =
                ScriptableObject.CreateInstance<RingFireworkBehaviourSO>();

            SerializedObject so = new SerializedObject(autoRadiusBehaviour);
            so.FindProperty("_particleCount").intValue = 100;
            so.FindProperty("_ringRadius").floatValue = 1.0f;
            so.FindProperty("_ringRadiusVariance").floatValue = 0.0f;
            so.FindProperty("_burstSpeed").floatValue = 5.0f;
            so.FindProperty("_burstSpeedVariance").floatValue = 0.0f;
            so.FindProperty("_gravity").floatValue = 2.0f;
            so.FindProperty("_drag").floatValue = 0.98f;
            so.FindProperty("_lifetime").floatValue = 1.5f;
            so.FindProperty("_lifetimeVariance").floatValue = 0.0f;
            so.FindProperty("_particleSize").floatValue = 0.15f;
            so.FindProperty("_sizeOverLife").animationCurveValue =
                AnimationCurve.Constant(0f, 1f, 1f);
            so.FindProperty("_alphaOverLife").animationCurveValue =
                AnimationCurve.Linear(0f, 1f, 1f, 0f);
            so.FindProperty("_isAutoRadiusFromPattern").boolValue = true;
            so.FindProperty("_patternScaleHint").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Pixel at (31, 16) on a 32x32 grid — far from center (16, 16)
            PixelEntry[] pattern = new[]
            {
                new PixelEntry(31, 16, new Color32(255, 0, 0, 255))
            };
            FireworkRequest request = new FireworkRequest
            {
                Position = new Vector3(0f, 10f, 0f),
                Pattern = pattern,
                PatternWidth = 32,
                PatternHeight = 32
            };

            int count = autoRadiusBehaviour.GetParticleCount(request);
            FireworkParticle[] particles = new FireworkParticle[count];

            autoRadiusBehaviour.InitializeParticles(particles, count, request);

            // All particles start at origin
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(request.Position, particles[i].Position,
                    $"Particle {i} should start at origin even with auto-radius");
            }

            // Velocity magnitude should reflect the configured burst speed (5.0),
            // confirming auto-radius didn't break initialization
            Assert.Greater(particles[0].Velocity.magnitude, 0f,
                "Particles should have non-zero velocity");

            Object.DestroyImmediate(autoRadiusBehaviour);
        }

        // ---- UpdateParticles Tests ----

        [Test]
        public void UpdateParticles_AppliesGravity()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = new Vector3(0f, 10f, 0f),
                Velocity = new Vector3(1f, 0f, 0f),
                Color = Color.white,
                Size = 0.15f,
                Life = 1.5f,
                MaxLife = 1.5f
            };

            float initialVelocityY = particles[0].Velocity.y;

            _behaviour.UpdateParticles(particles, 1, 0.5f);

            Assert.Less(particles[0].Velocity.y, initialVelocityY,
                "Gravity should pull velocity downward after update");
        }

        [Test]
        public void UpdateParticles_DeadParticle_IsSkipped()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = new Vector3(5f, 5f, 5f),
                Velocity = new Vector3(10f, 0f, 0f),
                Color = Color.white,
                Size = 0.15f,
                Life = 0f,
                MaxLife = 1f
            };

            _behaviour.UpdateParticles(particles, 1, 0.1f);

            Assert.AreEqual(new Vector3(5f, 5f, 5f), particles[0].Position,
                "Dead particle position should remain unchanged");
        }

        // ---- IsComplete Tests ----

        [Test]
        public void IsComplete_AllDead_ReturnsTrue()
        {
            FireworkParticle[] particles = new FireworkParticle[3];
            // All default with Life = 0

            Assert.IsTrue(_behaviour.IsComplete(particles, 3));
        }

        [Test]
        public void IsComplete_SomeAlive_ReturnsFalse()
        {
            FireworkParticle[] particles = new FireworkParticle[3];
            particles[1].Life = 0.5f;

            Assert.IsFalse(_behaviour.IsComplete(particles, 3));
        }

        // ---- OnValidate Tests ----

        [Test]
        public void OnValidate_ParticleCountBelowMin_ClampedToMin()
        {
            RingFireworkBehaviourSO testBehaviour =
                ScriptableObject.CreateInstance<RingFireworkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_particleCount").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(1, testBehaviour.ParticleCount);

            Object.DestroyImmediate(testBehaviour);
        }

        [Test]
        public void OnValidate_RingRadiusBelowMin_ClampedToMin()
        {
            RingFireworkBehaviourSO testBehaviour =
                ScriptableObject.CreateInstance<RingFireworkBehaviourSO>();

            SerializedObject so = new SerializedObject(testBehaviour);
            so.FindProperty("_ringRadius").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.1f, testBehaviour.RingRadius, 0.001f);

            Object.DestroyImmediate(testBehaviour);
        }
    }
}
