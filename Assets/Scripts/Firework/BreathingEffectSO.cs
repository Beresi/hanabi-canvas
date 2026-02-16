// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Size pulsation and HDR emissive glow effect. Applies sinusoidal size
    /// modulation and color intensity variation using per-particle RandomSeed
    /// for phase offset, producing organic breathing visuals.
    /// </summary>
    [CreateAssetMenu(fileName = "New Breathing Effect", menuName = "Hanabi Canvas/Firework Effects/Breathing")]
    public class BreathingEffectSO : FireworkEffectSO
    {
        // ---- Constants ----
        private const float TWO_PI = 6.2831853f;
        private const float MIN_FREQUENCY = 0.1f;

        // ---- Serialized Fields ----

        [Header("Size Pulsation")]
        [Tooltip("Frequency of size pulsation in Hz")]
        [Min(MIN_FREQUENCY)]
        [SerializeField] private float _sizeFrequency = 3f;

        [Tooltip("Fraction of current size used as pulsation amplitude")]
        [Range(0f, 1f)]
        [SerializeField] private float _sizeAmplitude = 0.2f;

        [Header("Emissive Glow")]
        [Tooltip("Base HDR multiplier applied to particle color")]
        [Min(1f)]
        [SerializeField] private float _emissiveBase = 1.5f;

        [Tooltip("Frequency of emissive glow oscillation in Hz")]
        [Min(MIN_FREQUENCY)]
        [SerializeField] private float _emissiveFrequency = 2f;

        [Tooltip("Amplitude added to emissive base")]
        [Min(0f)]
        [SerializeField] private float _emissiveAmplitude = 1.0f;

        // ---- Public Properties ----

        /// <summary>Frequency of size pulsation in Hz.</summary>
        public float SizeFrequency => _sizeFrequency;

        /// <summary>Fraction of current size used as pulsation amplitude.</summary>
        public float SizeAmplitude => _sizeAmplitude;

        /// <summary>Base HDR multiplier applied to particle color.</summary>
        public float EmissiveBase => _emissiveBase;

        /// <summary>Frequency of emissive glow oscillation in Hz.</summary>
        public float EmissiveFrequency => _emissiveFrequency;

        /// <summary>Amplitude added to emissive base.</summary>
        public float EmissiveAmplitude => _emissiveAmplitude;

        // ---- Unity Callbacks ----

        private void OnValidate()
        {
            _sizeFrequency = Mathf.Max(_sizeFrequency, MIN_FREQUENCY);
            _emissiveBase = Mathf.Max(_emissiveBase, 1f);
            _emissiveFrequency = Mathf.Max(_emissiveFrequency, MIN_FREQUENCY);
            _emissiveAmplitude = Mathf.Max(_emissiveAmplitude, 0f);
        }

        // ---- FireworkEffectSO Implementation ----

        /// <inheritdoc/>
        public override void InitializeEffect(FireworkParticle[] particles, int count)
        {
            // No-op: RandomSeed already set by behaviour
        }

        /// <inheritdoc/>
        public override void UpdateEffect(FireworkParticle[] particles, int count, float deltaTime, float elapsedTime)
        {
            for (int i = 0; i < count; i++)
            {
                if (particles[i].Life <= 0f)
                {
                    continue;
                }

                float phaseOffset = particles[i].RandomSeed * TWO_PI;

                // Size pulsation
                float sizeWave = Mathf.Sin(elapsedTime * _sizeFrequency * TWO_PI + phaseOffset);
                particles[i].Size *= (1f + sizeWave * _sizeAmplitude);

                // Emissive glow
                float emissiveWave = Mathf.Sin(elapsedTime * _emissiveFrequency * TWO_PI + phaseOffset);
                float emissiveIntensity = _emissiveBase + emissiveWave * _emissiveAmplitude;
                if (emissiveIntensity < 0f)
                {
                    emissiveIntensity = 0f;
                }

                Color c = particles[i].Color;
                particles[i].Color = new Color(
                    c.r * emissiveIntensity,
                    c.g * emissiveIntensity,
                    c.b * emissiveIntensity,
                    c.a);
            }
        }
    }
}
