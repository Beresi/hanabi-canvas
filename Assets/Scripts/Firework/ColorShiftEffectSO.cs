// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Warm-to-cool color transition effect. Lerps particle RGB from their
    /// original <see cref="FireworkParticle.BaseColor"/> toward a configurable
    /// fade color over the particle's lifetime, driven by an AnimationCurve.
    /// </summary>
    [CreateAssetMenu(fileName = "New Color Shift Effect", menuName = "Hanabi Canvas/Firework Effects/Color Shift")]
    public class ColorShiftEffectSO : FireworkEffectSO
    {
        // ---- Serialized Fields ----

        [Tooltip("Target color to shift toward over lifetime")]
        [SerializeField] private Color _fadeColor = new Color(0.3f, 0.5f, 1.0f, 1f);

        [Tooltip("Shift progression over normalized lifetime (x: 0=birth, 1=death)")]
        [SerializeField] private AnimationCurve _shiftCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // ---- Public Properties ----

        /// <summary>Target color to shift toward over lifetime.</summary>
        public Color FadeColor => _fadeColor;

        /// <summary>Shift progression curve over normalized lifetime.</summary>
        public AnimationCurve ShiftCurve => _shiftCurve;

        // ---- FireworkEffectSO Implementation ----

        /// <inheritdoc/>
        public override void InitializeEffect(FireworkParticle[] particles, int count)
        {
            // No-op: BaseColor already set by behaviour
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

                // progress: 0 at birth, 1 at death
                float progress = particles[i].MaxLife > 0f
                    ? (particles[i].MaxLife - particles[i].Life) / particles[i].MaxLife
                    : 1f;

                float shiftAmount = _shiftCurve.Evaluate(progress);

                Color baseColor = particles[i].BaseColor;
                float r = Mathf.Lerp(baseColor.r, _fadeColor.r, shiftAmount);
                float g = Mathf.Lerp(baseColor.g, _fadeColor.g, shiftAmount);
                float b = Mathf.Lerp(baseColor.b, _fadeColor.b, shiftAmount);

                // Preserve current alpha (from behaviour's own update)
                particles[i].Color = new Color(r, g, b, particles[i].Color.a);
            }
        }
    }
}
