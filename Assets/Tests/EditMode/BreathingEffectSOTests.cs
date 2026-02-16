// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Tests.EditMode
{
    public class BreathingEffectSOTests
    {
        // ---- Private Fields ----
        private BreathingEffectSO _effect;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _effect = ScriptableObject.CreateInstance<BreathingEffectSO>();

            SerializedObject so = new SerializedObject(_effect);
            so.FindProperty("_sizeFrequency").floatValue = 3f;
            so.FindProperty("_sizeAmplitude").floatValue = 0.2f;
            so.FindProperty("_emissiveBase").floatValue = 1.5f;
            so.FindProperty("_emissiveFrequency").floatValue = 2f;
            so.FindProperty("_emissiveAmplitude").floatValue = 1.0f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_effect);
        }

        // ---- OnValidate Tests ----
        [Test]
        public void OnValidate_SizeFrequencyBelowMin_ClampedToMin()
        {
            BreathingEffectSO testEffect = ScriptableObject.CreateInstance<BreathingEffectSO>();

            SerializedObject so = new SerializedObject(testEffect);
            so.FindProperty("_sizeFrequency").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.1f, testEffect.SizeFrequency, 0.001f);

            Object.DestroyImmediate(testEffect);
        }

        [Test]
        public void OnValidate_EmissiveBaseBelowMin_ClampedToMin()
        {
            BreathingEffectSO testEffect = ScriptableObject.CreateInstance<BreathingEffectSO>();

            SerializedObject so = new SerializedObject(testEffect);
            so.FindProperty("_emissiveBase").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(1f, testEffect.EmissiveBase, 0.001f);

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
                Color = new Color(0.5f, 0.5f, 0.5f, 1f),
                Size = 0.2f,
                Life = 0f,
                MaxLife = 1f,
                RandomSeed = 0.5f,
                BaseColor = new Color(0.5f, 0.5f, 0.5f, 1f)
            };

            float originalSize = particles[0].Size;
            Color originalColor = particles[0].Color;

            _effect.UpdateEffect(particles, 1, 0.016f, 1.0f);

            Assert.AreEqual(originalSize, particles[0].Size, 0.001f,
                "Dead particle size should not change");
            Assert.AreEqual(originalColor.r, particles[0].Color.r, 0.001f,
                "Dead particle color should not change");
        }

        [Test]
        public void UpdateEffect_AppliesSizePulsation_ModifiesSize()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 1.0f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 0f,
                BaseColor = Color.white
            };

            // elapsedTime=0.25 → sin(0.25 * 3 * 2π) = sin(1.5π) = -1 → clear pulsation
            _effect.UpdateEffect(particles, 1, 0.016f, 0.25f);

            // Size should be modified by pulsation (not exactly 1.0)
            Assert.AreNotEqual(1.0f, particles[0].Size,
                "Size should be modified by breathing pulsation");
        }

        [Test]
        public void UpdateEffect_AppliesEmissiveGlow_IncreasesRGB()
        {
            // Set up with high emissive base to guarantee increase
            BreathingEffectSO highEmissive = ScriptableObject.CreateInstance<BreathingEffectSO>();

            SerializedObject so = new SerializedObject(highEmissive);
            so.FindProperty("_sizeFrequency").floatValue = 3f;
            so.FindProperty("_sizeAmplitude").floatValue = 0f;
            so.FindProperty("_emissiveBase").floatValue = 2f;
            so.FindProperty("_emissiveFrequency").floatValue = 2f;
            so.FindProperty("_emissiveAmplitude").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = new Color(0.5f, 0.5f, 0.5f, 1f),
                Size = 1.0f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 0f,
                BaseColor = new Color(0.5f, 0.5f, 0.5f, 1f)
            };

            highEmissive.UpdateEffect(particles, 1, 0.016f, 0.5f);

            Assert.Greater(particles[0].Color.r, 0.5f,
                "Emissive glow with base=2 should increase RGB values");

            Object.DestroyImmediate(highEmissive);
        }

        [Test]
        public void UpdateEffect_DifferentRandomSeeds_ProduceDifferentSizes()
        {
            FireworkParticle[] particles = new FireworkParticle[2];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = Color.white,
                Size = 1.0f,
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
                Size = 1.0f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 0.5f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 2, 0.016f, 0.5f);

            Assert.AreNotEqual(particles[0].Size, particles[1].Size,
                "Particles with different RandomSeeds should have different sizes");
        }
    }
}
