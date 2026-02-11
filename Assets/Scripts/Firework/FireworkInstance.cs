// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    public class FireworkInstance : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("Configuration")]
        [Tooltip("Firework configuration defining phase timing and particle behavior")]
        [SerializeField] private FireworkConfigSO _config;

        [Header("Active Fireworks")]
        [Tooltip("List SO for tracking all active firework instances")]
        [SerializeField] private FireworkInstanceListSO _activeFireworks;

        // ---- Private Fields ----
        private ParticleData[] _particles;
        private int _particleCount;
        private int _currentPhaseIndex;
        private float _phaseElapsed;
        private bool _isComplete;
        private bool _isInitialized;

        // ---- Properties ----
        public ParticleData[] Particles => _particles;
        public int ParticleCount => _particleCount;
        public bool IsComplete => _isComplete;
        public int CurrentPhaseIndex => _currentPhaseIndex;

        // ---- Unity Methods ----
        private void OnEnable()
        {
            if (_activeFireworks != null)
            {
                _activeFireworks.Add(this);
            }
        }

        private void OnDisable()
        {
            if (_activeFireworks != null)
            {
                _activeFireworks.Remove(this);
            }
        }

        private void Update()
        {
            if (!_isInitialized || _isComplete || _particles == null)
            {
                return;
            }

            FireworkPhaseSO currentPhase = _config.GetPhase(_currentPhaseIndex);
            if (currentPhase == null)
            {
                _isComplete = true;
                return;
            }

            float phaseProgress = currentPhase.ProgressCurve.Evaluate(
                Mathf.Clamp01(_phaseElapsed / currentPhase.Duration));

            switch (_currentPhaseIndex)
            {
                case 0:
                    FireworkUpdater.UpdateBurst(_particles, _particleCount, Time.deltaTime,
                        _config.BurstDrag, _config.SteerStrength, _config.SteerCurve,
                        phaseProgress, _config.FadeGravity, _config.ParticleSizeFadeMultiplier);
                    break;
                case 1:
                    FireworkUpdater.UpdateSteer(_particles, _particleCount, Time.deltaTime,
                        _config.SteerStrength, _config.SteerCurve, phaseProgress,
                        _config.SteerDebrisDrag, _config.FadeGravity,
                        _config.ParticleSizeFadeMultiplier);
                    break;
                case 2:
                    FireworkUpdater.UpdateHold(_particles, _particleCount, Time.deltaTime,
                        _config.HoldSparkleIntensity, _config.HoldJitterScale, Time.time,
                        _config.FadeGravity, _config.ParticleSizeFadeMultiplier);
                    break;
                case 3:
                    FireworkUpdater.UpdateFade(_particles, _particleCount, Time.deltaTime,
                        _config.FadeGravity, _config.ParticleSizeFadeMultiplier,
                        currentPhase.Duration);
                    break;
            }

            _phaseElapsed += Time.deltaTime;
            if (_phaseElapsed >= currentPhase.Duration)
            {
                AdvancePhase();
            }
        }

        // ---- Public Methods ----
        public void Initialize(FireworkConfigSO config, PixelDataSO pixelData, Vector3 origin)
        {
            _config = config;
            InitializeParticles(pixelData, origin);
        }

        public void Initialize(FireworkConfigSO config, PixelDataSO pixelData, Vector3 origin,
            FireworkInstanceListSO activeFireworks)
        {
            _config = config;
            _activeFireworks = activeFireworks;

            if (_activeFireworks != null && !_activeFireworks.Contains(this))
            {
                _activeFireworks.Add(this);
            }

            InitializeParticles(pixelData, origin);
        }

        // ---- Private Methods ----
        private void InitializeParticles(PixelDataSO pixelData, Vector3 origin)
        {
            if (_config == null)
            {
                Debug.LogWarning(
                    $"[{nameof(FireworkInstance)}] FireworkConfigSO is not assigned.", this);
                enabled = false;
                return;
            }

            if (pixelData == null)
            {
                Debug.LogWarning(
                    $"[{nameof(FireworkInstance)}] PixelDataSO is not assigned.", this);
                enabled = false;
                return;
            }

            int patternCount = pixelData.PixelCount;
            int debrisCount = _config.DebrisParticleCount;
            _particleCount = patternCount + debrisCount;

            if (_particleCount == 0)
            {
                _isComplete = true;
                return;
            }

            _particles = new ParticleData[_particleCount];

            float halfWidth = pixelData.Width / 2f;
            float halfHeight = pixelData.Height / 2f;
            float formationScale = _config.FormationScale;

            for (int i = 0; i < patternCount; i++)
            {
                PixelEntry entry = pixelData.GetPixelAt(i);

                float formX = (entry.X - halfWidth) * formationScale;
                float formY = (entry.Y - halfHeight) * formationScale;
                Vector3 formationTarget = origin + new Vector3(formX, formY, 0f);

                _particles[i] = new ParticleData
                {
                    Position = origin,
                    Velocity = Random.onUnitSphere * _config.BurstRadius,
                    Color = (Color)entry.Color,
                    Size = _config.ParticleSize,
                    Life = 1f,
                    FormationTarget = formationTarget,
                    IsPattern = true
                };
            }

            Color32[] debrisColors = _config.DebrisColors;
            bool hasDebrisColors = debrisColors != null && debrisColors.Length > 0;

            for (int i = 0; i < debrisCount; i++)
            {
                int particleIndex = patternCount + i;

                Color debrisColor = hasDebrisColors
                    ? (Color)debrisColors[i % debrisColors.Length]
                    : Color.white;

                _particles[particleIndex] = new ParticleData
                {
                    Position = origin,
                    Velocity = Random.onUnitSphere * _config.BurstRadius * _config.DebrisSpeedMultiplier,
                    Color = debrisColor,
                    Size = _config.ParticleSize,
                    Life = 1f,
                    FormationTarget = Vector3.zero,
                    IsPattern = false
                };
            }

            _currentPhaseIndex = 0;
            _phaseElapsed = 0f;
            _isComplete = false;
            _isInitialized = true;
        }

        private void AdvancePhase()
        {
            _currentPhaseIndex++;
            _phaseElapsed = 0f;

            if (_currentPhaseIndex >= _config.PhaseCount)
            {
                _isComplete = true;
                Destroy(gameObject);
            }
        }
    }
}
