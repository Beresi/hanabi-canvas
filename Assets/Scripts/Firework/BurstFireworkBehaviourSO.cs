// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Concrete firework behaviour that produces a spherical burst of particles
    /// with gravity, drag, and size/alpha curves over lifetime.
    /// </summary>
    [CreateAssetMenu(fileName = "New Burst Behaviour", menuName = "Hanabi Canvas/Firework Behaviours/Burst")]
    public class BurstFireworkBehaviourSO : FireworkBehaviourSO
    {
        // ---- Constants ----
        private const int MIN_PARTICLE_COUNT = 1;
        private const int MAX_PARTICLE_COUNT = 2000;
        private const float MIN_BURST_SPEED = 0.1f;
        private const float MIN_PARTICLE_SIZE = 0.01f;
        private const float MIN_LIFETIME = 0.1f;

        // ---- Serialized Fields ----

        [Header("Burst")]
        [Tooltip("Number of particles spawned per burst")]
        [Range(MIN_PARTICLE_COUNT, MAX_PARTICLE_COUNT)]
        [SerializeField] private int _particleCount = 200;

        [Tooltip("Base outward speed at burst (units/sec)")]
        [Min(MIN_BURST_SPEED)]
        [SerializeField] private float _burstSpeed = 8f;

        [Tooltip("Random variance applied to burst speed (0=uniform, 1=\u00b1full range)")]
        [Range(0f, 1f)]
        [SerializeField] private float _burstSpeedVariance = 0.3f;

        [Tooltip("Minimum burst speed per pixel-radius of the pattern. " +
                 "Ensures burst always covers the pattern. " +
                 "Set >= pattern scale * desired coverage (e.g. 0.3 scale * 2 = 0.6)")]
        [Min(0f)]
        [SerializeField] private float _burstExtentMultiplier = 0.6f;

        [Header("Physics")]
        [Tooltip("Downward acceleration (units/sec\u00b2)")]
        [Min(0f)]
        [SerializeField] private float _gravity = 4f;

        [Tooltip("Velocity multiplier per frame (0=instant stop, 1=no drag)")]
        [Range(0f, 1f)]
        [SerializeField] private float _drag = 0.98f;

        [Header("Lifetime")]
        [Tooltip("Base lifetime of each particle in seconds")]
        [Min(MIN_LIFETIME)]
        [SerializeField] private float _lifetime = 2.0f;

        [Tooltip("Random variance applied to lifetime (0=uniform, 1=\u00b1full range)")]
        [Range(0f, 1f)]
        [SerializeField] private float _lifetimeVariance = 0.2f;

        [Header("Appearance")]
        [Tooltip("Base size of each particle quad in world units")]
        [Min(MIN_PARTICLE_SIZE)]
        [SerializeField] private float _particleSize = 0.15f;

        [Tooltip("Size multiplier over normalized lifetime (x: 0=birth, 1=death)")]
        [SerializeField] private AnimationCurve _sizeOverLife = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Tooltip("Alpha multiplier over normalized lifetime (x: 0=birth, 1=death)")]
        [SerializeField] private AnimationCurve _alphaOverLife = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        // ---- Public Properties ----

        /// <summary>Number of particles spawned per burst.</summary>
        public int ParticleCount => _particleCount;

        /// <summary>Base outward speed at burst (units/sec).</summary>
        public float BurstSpeed => _burstSpeed;

        /// <summary>Random variance applied to burst speed (0=uniform, 1=full range).</summary>
        public float BurstSpeedVariance => _burstSpeedVariance;

        /// <summary>Minimum burst speed per pixel-radius. Ensures burst covers the pattern.</summary>
        public float BurstExtentMultiplier => _burstExtentMultiplier;

        /// <summary>Downward acceleration (units/sec squared).</summary>
        public float Gravity => _gravity;

        /// <summary>Velocity multiplier per frame (0=instant stop, 1=no drag).</summary>
        public float Drag => _drag;

        /// <summary>Base lifetime of each particle in seconds.</summary>
        public float Lifetime => _lifetime;

        /// <summary>Random variance applied to lifetime (0=uniform, 1=full range).</summary>
        public float LifetimeVariance => _lifetimeVariance;

        /// <summary>Base size of each particle quad in world units.</summary>
        public float ParticleSize => _particleSize;

        /// <summary>Size multiplier over normalized lifetime (x: 0=birth, 1=death).</summary>
        public AnimationCurve SizeOverLife => _sizeOverLife;

        /// <summary>Alpha multiplier over normalized lifetime (x: 0=birth, 1=death).</summary>
        public AnimationCurve AlphaOverLife => _alphaOverLife;

        // ---- Unity Callbacks ----

        private void OnValidate()
        {
            _particleCount = Mathf.Clamp(_particleCount, MIN_PARTICLE_COUNT, MAX_PARTICLE_COUNT);
            _burstSpeed = Mathf.Max(_burstSpeed, MIN_BURST_SPEED);
            _particleSize = Mathf.Max(_particleSize, MIN_PARTICLE_SIZE);
            _lifetime = Mathf.Max(_lifetime, MIN_LIFETIME);
        }

        // ---- FireworkBehaviourSO Implementation ----

        /// <inheritdoc/>
        public override int GetParticleCount(FireworkRequest request)
        {
            return _particleCount;
        }

        /// <inheritdoc/>
        public override void InitializeParticles(FireworkParticle[] particles, int count, FireworkRequest request)
        {
            // Build weighted color table from pattern
            bool hasPattern = request.Pattern != null && request.Pattern.Length > 0;
            Color[] colorTable = null;
            if (hasPattern)
            {
                colorTable = BuildWeightedColorTable(request.Pattern);
            }

            // Compute effective burst speed: ensure burst covers the pattern
            float effectiveBurstSpeed = _burstSpeed;
            if (hasPattern && _burstExtentMultiplier > 0f)
            {
                float maxPixelRadius = ComputeMaxPixelRadius(
                    request.Pattern, request.PatternWidth, request.PatternHeight);
                float minBurstSpeed = maxPixelRadius * _burstExtentMultiplier;
                if (minBurstSpeed > effectiveBurstSpeed)
                {
                    effectiveBurstSpeed = minBurstSpeed;
                }
            }

            for (int i = 0; i < count; i++)
            {
                float speedVariance = _burstSpeedVariance;
                float speed = effectiveBurstSpeed * Random.Range(1f - speedVariance, 1f + speedVariance);

                float lifeVariance = _lifetimeVariance;
                float life = _lifetime * Random.Range(1f - lifeVariance, 1f + lifeVariance);

                particles[i].Position = request.Position;
                particles[i].Velocity = Random.onUnitSphere * speed;
                particles[i].Color = hasPattern
                    ? colorTable[Random.Range(0, colorTable.Length)]
                    : Color.white;
                particles[i].Size = _particleSize;
                particles[i].Life = life;
                particles[i].MaxLife = life;
            }
        }

        /// <summary>
        /// Builds a color table where each color appears proportionally to its
        /// pixel frequency in the pattern. Sampling uniformly from this table
        /// produces weighted-random color selection.
        /// </summary>
        private static Color[] BuildWeightedColorTable(PixelEntry[] pattern)
        {
            Color[] table = new Color[pattern.Length];
            for (int i = 0; i < pattern.Length; i++)
            {
                Color32 c32 = pattern[i].Color;
                table[i] = new Color(c32.r / 255f, c32.g / 255f, c32.b / 255f, 1f);
            }

            return table;
        }

        /// <summary>
        /// Computes the maximum distance (in pixels) any pixel is from the grid center.
        /// Used to determine the minimum burst radius needed to cover the pattern.
        /// </summary>
        private static float ComputeMaxPixelRadius(PixelEntry[] pattern, int width, int height)
        {
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            float maxRadiusSq = 0f;

            for (int i = 0; i < pattern.Length; i++)
            {
                float dx = pattern[i].X - halfWidth;
                float dy = pattern[i].Y - halfHeight;
                float distSq = dx * dx + dy * dy;
                if (distSq > maxRadiusSq)
                {
                    maxRadiusSq = distSq;
                }
            }

            return Mathf.Sqrt(maxRadiusSq);
        }

        /// <inheritdoc/>
        public override void UpdateParticles(FireworkParticle[] particles, int count, float deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                if (particles[i].Life <= 0f)
                {
                    continue;
                }

                // Decrement life
                particles[i].Life -= deltaTime;
                if (particles[i].Life < 0f)
                {
                    particles[i].Life = 0f;
                }

                // Apply gravity
                particles[i].Velocity += Vector3.down * _gravity * deltaTime;

                // Apply drag
                particles[i].Velocity *= _drag;

                // Integrate position
                particles[i].Position += particles[i].Velocity * deltaTime;

                // Compute normalized progress (0 at birth, 1 at death)
                float progress = particles[i].MaxLife > 0f
                    ? (particles[i].MaxLife - particles[i].Life) / particles[i].MaxLife
                    : 1f;

                // Set size from curve
                particles[i].Size = _particleSize * _sizeOverLife.Evaluate(progress);

                // Set alpha only (RGB stays constant)
                Color c = particles[i].Color;
                particles[i].Color = new Color(c.r, c.g, c.b, _alphaOverLife.Evaluate(progress));
            }
        }

        /// <inheritdoc/>
        public override bool IsComplete(FireworkParticle[] particles, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (particles[i].Life > 0f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
