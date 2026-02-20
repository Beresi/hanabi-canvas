// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Tests.EditMode
{
    public class GravityVarianceEffectSOTests
    {
        // ---- Private Fields ----
        private GravityVarianceEffectSO _effect;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _effect = ScriptableObject.CreateInstance<GravityVarianceEffectSO>();

            SerializedObject so = new SerializedObject(_effect);
            so.FindProperty("_minGravityMultiplier").floatValue = 0.5f;
            so.FindProperty("_maxGravityMultiplier").floatValue = 2.0f;
            so.FindProperty("_baseGravity").floatValue = 3.0f;
            so.FindProperty("_startDelay").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_effect);
        }

        // ---- OnValidate Tests ----
        [Test]
        public void OnValidate_MinGravityBelowZero_ClampedToZero()
        {
            GravityVarianceEffectSO testEffect = ScriptableObject.CreateInstance<GravityVarianceEffectSO>();

            SerializedObject so = new SerializedObject(testEffect);
            so.FindProperty("_minGravityMultiplier").floatValue = -1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0f, testEffect.MinGravityMultiplier, 0.001f);

            Object.DestroyImmediate(testEffect);
        }

        // ---- UpdateEffect Tests ----
        [Test]
        public void UpdateEffect_DeadParticle_IsSkipped()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 0f,
                MaxLife = 1f,
                RandomSeed = 0.5f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 1, 0.1f, 1.0f);

            Assert.AreEqual(0f, particles[0].Position.y, 0.001f,
                "Dead particle position should not change");
        }

        [Test]
        public void UpdateEffect_AppliesDownwardDisplacement()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 0.5f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 1, 0.1f, 0.5f);

            Assert.Less(particles[0].Position.y, 0f,
                "Gravity variance should displace particle downward");
        }

        [Test]
        public void UpdateEffect_DifferentRandomSeeds_ProduceDifferentPositions()
        {
            FireworkParticle[] particles = new FireworkParticle[2];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 0.0f,
                BaseColor = Color.white
            };
            particles[1] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 1.0f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 2, 0.1f, 0.5f);

            Assert.AreNotEqual(particles[0].Position.y, particles[1].Position.y,
                "Particles with different RandomSeeds should have different vertical positions");
        }

        [Test]
        public void UpdateEffect_HighRandomSeed_FallsFaster()
        {
            FireworkParticle[] particles = new FireworkParticle[2];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 0.0f,
                BaseColor = Color.white
            };
            particles[1] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 1.0f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 2, 0.1f, 0.5f);

            // RandomSeed=1 maps to maxGravityMultiplier=2.0 (stronger pull)
            // RandomSeed=0 maps to minGravityMultiplier=0.5 (weaker pull)
            Assert.Less(particles[1].Position.y, particles[0].Position.y,
                "Higher RandomSeed should fall further (stronger gravity)");
        }

        [Test]
        public void UpdateEffect_DisplacementAcceleratesOverTime()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 0.5f,
                BaseColor = Color.white
            };

            // Apply at elapsedTime=0.5
            _effect.UpdateEffect(particles, 1, 0.1f, 0.5f);
            float earlyDisplacement = -particles[0].Position.y;

            // Reset and apply at elapsedTime=1.5
            particles[0].Position = Vector3.zero;
            _effect.UpdateEffect(particles, 1, 0.1f, 1.5f);
            float lateDisplacement = -particles[0].Position.y;

            Assert.Greater(lateDisplacement, earlyDisplacement,
                "Gravity displacement should be larger at later elapsed times");
        }
    }
}
