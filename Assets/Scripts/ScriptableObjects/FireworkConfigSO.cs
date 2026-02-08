// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Firework Config", menuName = "Hanabi Canvas/Config/Firework Config")]
    public class FireworkConfigSO : ScriptableObject
    {
        // ---- Constants ----
        private const float MIN_BURST_RADIUS = 0.1f;
        private const float MIN_PARTICLE_SIZE = 0.01f;
        private const float MIN_FORMATION_SCALE = 0.01f;

        // ---- Serialized Fields ----
        [Header("Phases")]
        [Tooltip("Ordered firework phases (Burst, Steer, Hold, Fade)")]
        [SerializeField] private FireworkPhaseSO[] _phases;

        [Header("Burst")]
        [Tooltip("How far particles travel during the initial explosion")]
        [Min(0.1f)]
        [SerializeField] private float _burstRadius = 5f;

        [Tooltip("Velocity drag multiplier per frame during burst (0 = instant stop, 1 = no drag)")]
        [Range(0f, 1f)]
        [SerializeField] private float _burstDrag = 0.98f;

        [Header("Steer")]
        [Tooltip("How aggressively pattern particles seek their formation position")]
        [Range(0f, 20f)]
        [SerializeField] private float _steerStrength = 8f;

        [Tooltip("Easing curve for steer convergence. X = phase progress, Y = steer influence")]
        [SerializeField] private AnimationCurve _steerCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Velocity drag multiplier per frame for debris during steer (0 = instant stop, 1 = no drag)")]
        [Range(0f, 1f)]
        [SerializeField] private float _steerDebrisDrag = 0.95f;

        [Header("Hold")]
        [Tooltip("Intensity of sparkle effect during hold phase")]
        [Range(0f, 5f)]
        [SerializeField] private float _holdSparkleIntensity = 1f;

        [Tooltip("Scale of the jitter offset applied to pattern particles during hold")]
        [Min(0f)]
        [SerializeField] private float _holdJitterScale = 0.01f;

        [Header("Fade")]
        [Tooltip("Downward drift speed during fade phase")]
        [Min(0f)]
        [SerializeField] private float _fadeGravity = 2f;

        [Header("Debris")]
        [Tooltip("Number of extra non-pattern particles")]
        [Range(0, 500)]
        [SerializeField] private int _debrisParticleCount = 200;

        [Tooltip("Speed multiplier for debris particles")]
        [Min(0f)]
        [SerializeField] private float _debrisSpeedMultiplier = 1.5f;

        [Tooltip("Colors used for debris particles")]
        [SerializeField] private Color32[] _debrisColors = new Color32[]
        {
            new Color32(255, 200, 100, 255),
            new Color32(255, 150, 50, 255),
            new Color32(255, 255, 200, 255)
        };

        [Header("Particles")]
        [Tooltip("Base size of each particle quad")]
        [Min(0.01f)]
        [SerializeField] private float _particleSize = 0.1f;

        [Tooltip("Size multiplier applied during fade phase (0 = shrink to nothing, 1 = no change)")]
        [Range(0f, 1f)]
        [SerializeField] private float _particleSizeFadeMultiplier = 0.5f;

        [Header("Formation")]
        [Tooltip("Scale factor for converting pixel grid positions to world formation positions")]
        [Min(0.01f)]
        [SerializeField] private float _formationScale = 0.1f;

        // ---- Properties ----
        public FireworkPhaseSO[] Phases => _phases;
        public int PhaseCount => _phases != null ? _phases.Length : 0;
        public float BurstRadius => _burstRadius;
        public float BurstDrag => _burstDrag;
        public float SteerStrength => _steerStrength;
        public AnimationCurve SteerCurve => _steerCurve;
        public float SteerDebrisDrag => _steerDebrisDrag;
        public float HoldSparkleIntensity => _holdSparkleIntensity;
        public float HoldJitterScale => _holdJitterScale;
        public float FadeGravity => _fadeGravity;
        public int DebrisParticleCount => _debrisParticleCount;
        public float DebrisSpeedMultiplier => _debrisSpeedMultiplier;
        public Color32[] DebrisColors => _debrisColors;
        public float ParticleSize => _particleSize;
        public float ParticleSizeFadeMultiplier => _particleSizeFadeMultiplier;
        public float FormationScale => _formationScale;

        // ---- Public Methods ----
        public FireworkPhaseSO GetPhase(int index)
        {
            if (_phases == null || index < 0 || index >= _phases.Length)
            {
                return null;
            }
            return _phases[index];
        }

        // ---- Validation ----
        private void OnValidate()
        {
            _burstRadius = Mathf.Max(MIN_BURST_RADIUS, _burstRadius);
            _particleSize = Mathf.Max(MIN_PARTICLE_SIZE, _particleSize);
            _formationScale = Mathf.Max(MIN_FORMATION_SCALE, _formationScale);

            if (_phases == null || _phases.Length == 0)
            {
                Debug.LogWarning("[FireworkConfigSO] No phases assigned.", this);
            }
            else
            {
                for (int i = 0; i < _phases.Length; i++)
                {
                    if (_phases[i] == null)
                    {
                        Debug.LogWarning(
                            $"[FireworkConfigSO] Phase at index {i} is null.", this);
                    }
                }
            }
        }
    }
}
