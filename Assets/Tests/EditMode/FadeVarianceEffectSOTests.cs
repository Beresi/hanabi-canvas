// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Tests.EditMode
{
    public class FadeVarianceEffectSOTests
    {
        // ---- Private Fields ----
        private FadeVarianceEffectSO _effect;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _effect = ScriptableObject.CreateInstance<FadeVarianceEffectSO>();

            SerializedObject so = new SerializedObject(_effect);
            so.FindProperty("_minFadeStart").floatValue = 0.2f;
            so.FindProperty("_maxFadeStart").floatValue = 0.8f;
            so.FindProperty("_fadeDuration").floatValue = 0.2f;
            so.FindProperty("_fadeCurve").animationCurveValue =
                AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_effect);
        }

        // ---- OnValidate Tests ----
        [Test]
        public void OnValidate_MinExceedsMax_MaxClampedToMin()
        {
            FadeVarianceEffectSO testEffect = ScriptableObject.CreateInstance<FadeVarianceEffectSO>();

            SerializedObject so = new SerializedObject(testEffect);
            so.FindProperty("_minFadeStart").floatValue = 0.8f;
            so.FindProperty("_maxFadeStart").floatValue = 0.3f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.GreaterOrEqual(testEffect.MaxFadeStart, testEffect.MinFadeStart,
                "MaxFadeStart should be >= MinFadeStart after OnValidate");

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
                Color = new Color(1f, 1f, 1f, 0.8f),
                Size = 0.2f,
                Life = 0f,
                MaxLife = 2f,
                RandomSeed = 0f,
                BaseColor = Color.white
            };

            float originalAlpha = particles[0].Color.a;

            _effect.UpdateEffect(particles, 1, 0.016f, 1.0f);

            Assert.AreEqual(originalAlpha, particles[0].Color.a, 0.001f,
                "Dead particle alpha should not change");
        }

        [Test]
        public void UpdateEffect_BeforeFadeWindow_FullBrightness()
        {
            // Particle with RandomSeed=0 → fadeStart=minFadeStart=0.2
            // At progress=0.1 (before 0.2), should be full brightness
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = new Color(0.5f, 0.5f, 0.5f, 0.3f),
                Size = 0.2f,
                Life = 1.8f,
                MaxLife = 2f,
                RandomSeed = 0f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 0.1f);

            // progress=0.1, fadeStart=0.2 → not yet fading → brightness=1.0
            Assert.AreEqual(1f, particles[0].Color.a, 0.01f,
                "Before fade window, alpha should be overridden to 1.0");
            Assert.AreEqual(0.5f, particles[0].Color.r, 0.01f,
                "Before fade window, RGB should be unchanged");
        }

        [Test]
        public void UpdateEffect_AfterFadeStart_ParticleDims()
        {
            // RandomSeed=0 → fadeStart=0.2, fadeDuration=0.2
            // At progress=0.35 (75% through individual fade), should be dimmed
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = new Color(1f, 1f, 1f, 1f),
                Size = 0.2f,
                Life = 1.3f,
                MaxLife = 2f,
                RandomSeed = 0f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 0.7f);

            Assert.Less(particles[0].Color.a, 1f,
                "After fade starts, particle should be dimmed");
            Assert.Less(particles[0].Color.r, 1f,
                "RGB should also be dimmed for additive blending");
        }

        [Test]
        public void UpdateEffect_AfterFadeComplete_FullyDimmed()
        {
            // RandomSeed=0 → fadeStart=0.2, fadeDuration=0.2 → done at progress=0.4
            // At progress=0.9 (well past), should be fully dimmed
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = new Color(1f, 1f, 1f, 1f),
                Size = 0.2f,
                Life = 0.2f,
                MaxLife = 2f,
                RandomSeed = 0f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 1.8f);

            Assert.AreEqual(0f, particles[0].Color.a, 0.01f,
                "After fade completes, particle should be fully dimmed");
        }

        [Test]
        public void UpdateEffect_LowRandomSeed_FadesBefore_HighRandomSeed()
        {
            // At progress=0.5:
            // Particle 0 (RandomSeed=0): fadeStart=0.2, has been fading for 0.3/0.2=1.0 → fully dimmed
            // Particle 1 (RandomSeed=1): fadeStart=0.8, hasn't started → full brightness
            FireworkParticle[] particles = new FireworkParticle[2];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = new Color(1f, 1f, 1f, 1f),
                Size = 0.2f,
                Life = 1f,
                MaxLife = 2f,
                RandomSeed = 0f,
                BaseColor = Color.white
            };
            particles[1] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = new Color(1f, 1f, 1f, 1f),
                Size = 0.2f,
                Life = 1f,
                MaxLife = 2f,
                RandomSeed = 1f,
                BaseColor = Color.white
            };

            _effect.UpdateEffect(particles, 2, 0.016f, 1.0f);

            // Particle 0 should be fully dimmed, particle 1 should be bright
            Assert.Less(particles[0].Color.a, 0.01f,
                "Low RandomSeed particle should be fully dimmed at progress=0.5");
            Assert.AreEqual(1f, particles[1].Color.a, 0.01f,
                "High RandomSeed particle should still be bright at progress=0.5");
        }

        [Test]
        public void UpdateEffect_DimsRGBProportionally()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            Color inputColor = new Color(0.8f, 0.4f, 0.6f, 1f);
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = inputColor,
                Size = 0.2f,
                Life = 1.3f,
                MaxLife = 2f,
                RandomSeed = 0f,
                BaseColor = inputColor
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 0.7f);

            // Verify all channels dim by the same factor
            float rRatio = particles[0].Color.r / inputColor.r;
            float gRatio = particles[0].Color.g / inputColor.g;
            float bRatio = particles[0].Color.b / inputColor.b;
            Assert.AreEqual(rRatio, gRatio, 0.001f,
                "R and G should dim by the same factor");
            Assert.AreEqual(gRatio, bRatio, 0.001f,
                "G and B should dim by the same factor");
        }
    }
}
