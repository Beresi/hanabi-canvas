// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Tests.EditMode
{
    public class ColorShiftEffectSOTests
    {
        // ---- Private Fields ----
        private ColorShiftEffectSO _effect;

        // ---- Setup / Teardown ----
        [SetUp]
        public void Setup()
        {
            _effect = ScriptableObject.CreateInstance<ColorShiftEffectSO>();

            SerializedObject so = new SerializedObject(_effect);
            so.FindProperty("_fadeColor").colorValue = new Color(0.3f, 0.5f, 1.0f, 1f);
            so.FindProperty("_shiftCurve").animationCurveValue =
                AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_effect);
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
                Color = new Color(1f, 0f, 0f, 1f),
                Size = 0.2f,
                Life = 0f,
                MaxLife = 1f,
                RandomSeed = 0.5f,
                BaseColor = new Color(1f, 0f, 0f, 1f)
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 1.0f);

            Assert.AreEqual(1f, particles[0].Color.r, 0.001f,
                "Dead particle color should not change");
            Assert.AreEqual(0f, particles[0].Color.g, 0.001f,
                "Dead particle color should not change");
        }

        [Test]
        public void UpdateEffect_AtBirth_ColorsUnchanged()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            Color baseColor = new Color(1f, 0f, 0f, 1f);
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = baseColor,
                Size = 0.2f,
                Life = 2f,
                MaxLife = 2f,
                RandomSeed = 0.5f,
                BaseColor = baseColor
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 0f);

            // At birth, progress=0, EaseInOut(0)=0, so no shift
            Assert.AreEqual(baseColor.r, particles[0].Color.r, 0.01f,
                "At birth, color should be unchanged from base");
            Assert.AreEqual(baseColor.g, particles[0].Color.g, 0.01f,
                "At birth, color should be unchanged from base");
            Assert.AreEqual(baseColor.b, particles[0].Color.b, 0.01f,
                "At birth, color should be unchanged from base");
        }

        [Test]
        public void UpdateEffect_AtDeath_ColorsApproachFadeColor()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            Color baseColor = new Color(1f, 0f, 0f, 1f);
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = baseColor,
                Size = 0.2f,
                Life = 0.001f,
                MaxLife = 2f,
                RandomSeed = 0.5f,
                BaseColor = baseColor
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 2f);

            // Near death, progress ~1, EaseInOut(1)=1, fully shifted to fade color
            Color fadeColor = _effect.FadeColor;
            Assert.AreEqual(fadeColor.r, particles[0].Color.r, 0.05f,
                "Near death, color should approach fade color R");
            Assert.AreEqual(fadeColor.g, particles[0].Color.g, 0.05f,
                "Near death, color should approach fade color G");
            Assert.AreEqual(fadeColor.b, particles[0].Color.b, 0.05f,
                "Near death, color should approach fade color B");
        }

        [Test]
        public void UpdateEffect_PreservesAlpha()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = new Color(1f, 0f, 0f, 0.5f),
                Size = 0.2f,
                Life = 1f,
                MaxLife = 2f,
                RandomSeed = 0.5f,
                BaseColor = new Color(1f, 0f, 0f, 1f)
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 1f);

            Assert.AreEqual(0.5f, particles[0].Color.a, 0.001f,
                "Alpha should be preserved from behaviour's update, not overwritten");
        }

        [Test]
        public void UpdateEffect_UsesBaseColorNotCurrentColor()
        {
            FireworkParticle[] particles = new FireworkParticle[1];
            // BaseColor is red, but current Color has been modified to green
            Color baseColor = new Color(1f, 0f, 0f, 1f);
            Color modifiedColor = new Color(0f, 1f, 0f, 1f);
            particles[0] = new FireworkParticle
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Color = modifiedColor,
                Size = 0.2f,
                Life = 1f,
                MaxLife = 2f,
                RandomSeed = 0.5f,
                BaseColor = baseColor
            };

            _effect.UpdateEffect(particles, 1, 0.016f, 1f);

            // At progress=0.5, EaseInOut curve is ~0.5
            // Should lerp from BaseColor (red) toward fadeColor, NOT from green
            // fadeColor = (0.3, 0.5, 1.0), baseColor = (1, 0, 0)
            // At ~50% shift: R should be between 1.0 and 0.3 (roughly 0.65)
            // If it incorrectly used current color (green), R would be near 0.15
            Assert.Greater(particles[0].Color.r, 0.4f,
                "Shift should use BaseColor (red), not current Color (green)");
        }
    }
}
