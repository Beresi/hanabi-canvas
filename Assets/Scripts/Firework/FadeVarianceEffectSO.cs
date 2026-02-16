// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Per-particle staggered fade-out effect. Each particle gets its own
    /// fade-start time from RandomSeed, creating organic sparkle where
    /// particles randomly wink out one by one.
    /// Overrides the behaviour's alpha — place LAST in the effects array.
    /// </summary>
    [CreateAssetMenu(fileName = "New Fade Variance Effect", menuName = "Hanabi Canvas/Firework Effects/Fade Variance")]
    public class FadeVarianceEffectSO : FireworkEffectSO
    {
        // ---- Serialized Fields ----

        [Header("Fade Window")]
        [Tooltip("Earliest normalized lifetime (0=birth, 1=death) at which the first particle begins dimming")]
        [Range(0f, 0.99f)]
        [SerializeField] private float _minFadeStart = 0.2f;

        [Tooltip("Latest normalized lifetime at which the last particle begins dimming")]
        [Range(0f, 0.99f)]
        [SerializeField] private float _maxFadeStart = 0.8f;

        [Header("Individual Fade")]
        [Tooltip("How long each particle's individual fade takes (in normalized lifetime units)")]
        [Range(0.01f, 1f)]
        [SerializeField] private float _fadeDuration = 0.2f;

        [Tooltip("Shape of each particle's fade (x: 0=fade start, 1=fully dimmed)")]
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // ---- Public Properties ----

        /// <summary>Earliest normalized lifetime at which the first particle begins dimming.</summary>
        public float MinFadeStart => _minFadeStart;

        /// <summary>Latest normalized lifetime at which the last particle begins dimming.</summary>
        public float MaxFadeStart => _maxFadeStart;

        /// <summary>How long each particle's individual fade takes (normalized lifetime units).</summary>
        public float FadeDuration => _fadeDuration;

        /// <summary>Shape of each particle's fade.</summary>
        public AnimationCurve FadeCurve => _fadeCurve;

        // ---- Unity Callbacks ----

        private void OnValidate()
        {
            _minFadeStart = Mathf.Clamp(_minFadeStart, 0f, 0.99f);
            _maxFadeStart = Mathf.Clamp(_maxFadeStart, 0f, 0.99f);
            _fadeDuration = Mathf.Clamp(_fadeDuration, 0.01f, 1f);

            if (_minFadeStart > _maxFadeStart)
            {
                _maxFadeStart = _minFadeStart;
            }
        }

        // ---- FireworkEffectSO Implementation ----

        /// <inheritdoc/>
        public override void InitializeEffect(FireworkParticle[] particles, int count)
        {
            // No-op: RandomSeed already set by behaviour
        }

        /// <inheritdoc/>
        public override void UpdateEffect(FireworkParticle[] particles, int count,
            float deltaTime, float elapsedTime)
        {
            for (int i = 0; i < count; i++)
            {
                if (particles[i].Life <= 0f)
                {
                    continue;
                }

                float progress = particles[i].MaxLife > 0f
                    ? 1f - (particles[i].Life / particles[i].MaxLife)
                    : 1f;

                // Each particle gets its own fade-start time from RandomSeed
                float particleFadeStart = Mathf.Lerp(
                    _minFadeStart, _maxFadeStart, particles[i].RandomSeed);

                float brightness;

                if (progress < particleFadeStart)
                {
                    // Not yet fading — full brightness (overrides behaviour's alpha)
                    brightness = 1f;
                }
                else
                {
                    // Individual rapid fade
                    float fadeElapsed = progress - particleFadeStart;
                    float localProgress = _fadeDuration > 0f
                        ? Mathf.Clamp01(fadeElapsed / _fadeDuration)
                        : 1f;

                    brightness = 1f - _fadeCurve.Evaluate(localProgress);
                }

                // Override alpha and dim RGB — both needed for additive blending
                Color c = particles[i].Color;
                particles[i].Color = new Color(
                    c.r * brightness,
                    c.g * brightness,
                    c.b * brightness,
                    brightness);
            }
        }
    }
}
