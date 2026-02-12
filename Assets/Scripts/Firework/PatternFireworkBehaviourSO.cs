// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Firework behaviour that forms the player's pixel-art pattern.
    /// Particles converge from origin to target at a constant speed (closer = arrives sooner),
    /// hold briefly, then drift outward along their approach direction with gravity.
    /// Fade runs concurrently starting after a configurable delay past hold-end.
    /// </summary>
    [CreateAssetMenu(fileName = "New Pattern Behaviour", menuName = "Hanabi Canvas/Firework Behaviours/Pattern")]
    public class PatternFireworkBehaviourSO : FireworkBehaviourSO
    {
        // ---- Constants ----
        private const float MIN_CONVERGE_SPEED = 0.1f;
        private const float MIN_HOLD_DURATION = 0f;
        private const float MIN_FADE_DURATION = 0.01f;
        private const float MIN_FADE_DELAY = 0f;
        private const float MIN_DRIFT_SPEED = 0f;
        private const float MIN_GRAVITY = 0f;
        private const float MIN_PARTICLE_SIZE = 0.01f;
        private const float MIN_PATTERN_SCALE = 0.01f;
        private const float ZERO_DISTANCE_THRESHOLD = 0.0001f;

        // ---- Serialized Fields ----

        [Header("Convergence")]
        [Tooltip("Speed at which particles move toward target (units/sec). Higher = faster convergence.")]
        [Min(MIN_CONVERGE_SPEED)]
        [SerializeField] private float _convergeSpeed = 12.0f;

        [Tooltip("Position easing curve during convergence (x: 0=start, 1=arrived). " +
                 "Linear = constant speed, ease-in/out = acceleration/deceleration.")]
        [SerializeField] private AnimationCurve _formationCurve =
            new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 2f, 0f));

        [Header("Hold")]
        [Tooltip("Duration in seconds to hold at target position after all particles arrive")]
        [Min(MIN_HOLD_DURATION)]
        [SerializeField] private float _holdDuration = 1.5f;

        [Header("Drift")]
        [Tooltip("Speed of outward drift after hold ends (units/sec). Very slow to preserve pattern shape.")]
        [Min(MIN_DRIFT_SPEED)]
        [SerializeField] private float _driftSpeed = 0.3f;

        [Tooltip("Downward pull during drift phase (units/sec squared)")]
        [Min(MIN_GRAVITY)]
        [SerializeField] private float _gravity = 2.0f;

        [Header("Fade")]
        [Tooltip("Seconds after hold-end before fade begins. Runs concurrently with drift.")]
        [Min(MIN_FADE_DELAY)]
        [SerializeField] private float _fadeDelay = 0.2f;

        [Tooltip("Duration in seconds for the fade-out")]
        [Min(MIN_FADE_DURATION)]
        [SerializeField] private float _fadeDuration = 0.8f;

        [Header("Appearance")]
        [Tooltip("Base size of each particle quad in world units")]
        [Min(MIN_PARTICLE_SIZE)]
        [SerializeField] private float _particleSize = 0.2f;

        [Tooltip("World units per pixel — controls how large the formed pattern appears")]
        [Min(MIN_PATTERN_SCALE)]
        [SerializeField] private float _patternScale = 0.3f;

        [Tooltip("Size multiplier over normalized lifetime (x: 0=birth, 1=death)")]
        [SerializeField] private AnimationCurve _sizeOverLife = AnimationCurve.Constant(0f, 1f, 1f);

        [Tooltip("Alpha multiplier during fade phase (x: 0=fade start, 1=fade end)")]
        [SerializeField] private AnimationCurve _alphaOverLife = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        // ---- Public Properties ----

        /// <summary>Speed at which particles converge toward targets (units/sec).</summary>
        public float ConvergeSpeed => _convergeSpeed;

        /// <summary>Position easing curve during convergence.</summary>
        public AnimationCurve FormationCurve => _formationCurve;

        /// <summary>Duration in seconds to hold at target position.</summary>
        public float HoldDuration => _holdDuration;

        /// <summary>Speed of outward drift after hold ends (units/sec).</summary>
        public float DriftSpeed => _driftSpeed;

        /// <summary>Downward pull during drift phase (units/sec squared).</summary>
        public float Gravity => _gravity;

        /// <summary>Seconds after hold-end before fade begins.</summary>
        public float FadeDelay => _fadeDelay;

        /// <summary>Duration in seconds for the fade-out.</summary>
        public float FadeDuration => _fadeDuration;

        /// <summary>Base size of each particle quad in world units.</summary>
        public float ParticleSize => _particleSize;

        /// <summary>World units per pixel.</summary>
        public float PatternScale => _patternScale;

        /// <summary>Size multiplier over normalized lifetime.</summary>
        public AnimationCurve SizeOverLife => _sizeOverLife;

        /// <summary>Alpha multiplier during fade phase.</summary>
        public AnimationCurve AlphaOverLife => _alphaOverLife;

        // ---- Unity Callbacks ----

        private void OnValidate()
        {
            _convergeSpeed = Mathf.Max(_convergeSpeed, MIN_CONVERGE_SPEED);
            _holdDuration = Mathf.Max(_holdDuration, MIN_HOLD_DURATION);
            _fadeDuration = Mathf.Max(_fadeDuration, MIN_FADE_DURATION);
            _fadeDelay = Mathf.Max(_fadeDelay, MIN_FADE_DELAY);
            _driftSpeed = Mathf.Max(_driftSpeed, MIN_DRIFT_SPEED);
            _gravity = Mathf.Max(_gravity, MIN_GRAVITY);
            _particleSize = Mathf.Max(_particleSize, MIN_PARTICLE_SIZE);
            _patternScale = Mathf.Max(_patternScale, MIN_PATTERN_SCALE);
        }

        // ---- FireworkBehaviourSO Implementation ----

        /// <inheritdoc/>
        public override int GetParticleCount(FireworkRequest request)
        {
            return request.Pattern != null ? request.Pattern.Length : 0;
        }

        /// <inheritdoc/>
        public override void InitializeParticles(FireworkParticle[] particles, int count, FireworkRequest request)
        {
            // Center offset: Width / 2f to center the pattern at origin
            float halfWidth = request.PatternWidth / 2f;
            float halfHeight = request.PatternHeight / 2f;

            // Pass 1: Compute targets and find max distance for global timeline
            float maxDistanceSq = 0f;
            for (int i = 0; i < count; i++)
            {
                PixelEntry pixel = request.Pattern[i];

                // Map grid coordinates to world-space offset from origin
                float worldX = (pixel.X - halfWidth) * _patternScale;
                float worldY = (pixel.Y - halfHeight) * _patternScale;
                Vector3 target = request.Position + new Vector3(worldX, worldY, 0f);

                particles[i].TargetPosition = target;

                float dx = target.x - request.Position.x;
                float dy = target.y - request.Position.y;
                float distSq = dx * dx + dy * dy;
                if (distSq > maxDistanceSq)
                {
                    maxDistanceSq = distSq;
                }
            }

            // Compute global timeline
            float maxDistance = Mathf.Sqrt(maxDistanceSq);
            float maxFormationTime = maxDistance / _convergeSpeed;
            float totalLife = maxFormationTime + _holdDuration + _fadeDelay + _fadeDuration;

            // Pass 2: Set per-particle state with known totalLife
            for (int i = 0; i < count; i++)
            {
                Vector3 toTarget = particles[i].TargetPosition - request.Position;
                float distance = toTarget.magnitude;

                // Store normalized approach direction in Velocity (for drift phase)
                // Zero-distance particles get zero vector (they're already at target)
                Vector3 direction = distance > ZERO_DISTANCE_THRESHOLD
                    ? toTarget / distance
                    : Vector3.zero;

                Color32 c32 = request.Pattern[i].Color;

                particles[i].Position = request.Position;
                particles[i].StartPosition = request.Position;
                particles[i].Velocity = direction;
                particles[i].Color = new Color(c32.r / 255f, c32.g / 255f, c32.b / 255f, 1f);
                particles[i].Size = _particleSize;
                particles[i].Life = totalLife;
                particles[i].MaxLife = totalLife;
            }
        }

        /// <inheritdoc/>
        public override void UpdateParticles(FireworkParticle[] particles, int count, float deltaTime)
        {
            if (count == 0)
            {
                return;
            }

            // Derive global timeline from the first particle's MaxLife
            // totalLife was encoded as: maxFormationTime + holdDuration + fadeDelay + fadeDuration
            float totalLife = particles[0].MaxLife;
            float maxFormationTime = totalLife - _holdDuration - _fadeDelay - _fadeDuration;
            if (maxFormationTime < 0f)
            {
                maxFormationTime = 0f;
            }
            float holdEndTime = maxFormationTime + _holdDuration;
            float fadeStartTime = holdEndTime + _fadeDelay;

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

                // Compute elapsed time since birth
                float elapsed = particles[i].MaxLife - particles[i].Life;

                // Per-particle formation time based on distance
                Vector3 toTarget = particles[i].TargetPosition - particles[i].StartPosition;
                float myDistance = toTarget.magnitude;
                float myFormationTime = myDistance / _convergeSpeed;

                // ---- POSITIONAL UPDATE ----

                if (elapsed < myFormationTime)
                {
                    // STATE: CONVERGE — Lerp from origin to target, eased by curve
                    float t = myFormationTime > 0f ? elapsed / myFormationTime : 1f;
                    float curveT = _formationCurve.Evaluate(t);

                    particles[i].Position = Vector3.Lerp(
                        particles[i].StartPosition,
                        particles[i].TargetPosition,
                        curveT);
                }
                else if (elapsed < holdEndTime)
                {
                    // STATE: HOLD — at target, no movement
                    particles[i].Position = particles[i].TargetPosition;
                }
                else
                {
                    // STATE: DRIFT — approach direction + gravity
                    float driftElapsed = elapsed - holdEndTime;

                    // Direction stored in Velocity field (normalized at init)
                    Vector3 driftOffset = particles[i].Velocity * (_driftSpeed * driftElapsed);
                    Vector3 gravityOffset = Vector3.down * (0.5f * _gravity * driftElapsed * driftElapsed);

                    particles[i].Position = particles[i].TargetPosition + driftOffset + gravityOffset;
                }

                // ---- SIZE UPDATE ----
                // sizeOverLife uses overall normalized progress (0 at birth, 1 at death)
                float overallProgress = totalLife > 0f ? elapsed / totalLife : 1f;
                particles[i].Size = _particleSize * _sizeOverLife.Evaluate(overallProgress);

                // ---- ALPHA UPDATE (concurrent with all positional states) ----
                Color c = particles[i].Color;
                if (elapsed >= fadeStartTime)
                {
                    float fadeElapsed = elapsed - fadeStartTime;
                    float fadeProgress = _fadeDuration > 0f ? fadeElapsed / _fadeDuration : 1f;
                    fadeProgress = Mathf.Clamp01(fadeProgress);

                    particles[i].Color = new Color(c.r, c.g, c.b,
                        _alphaOverLife.Evaluate(fadeProgress));
                }
                else
                {
                    // Before fade: ensure full alpha
                    particles[i].Color = new Color(c.r, c.g, c.b, 1f);
                }
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
