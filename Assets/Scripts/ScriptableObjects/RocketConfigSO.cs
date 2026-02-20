// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Configuration SO holding all rocket tuning data: spawn/destination positions,
    /// path behaviours, head visuals, and trail particle settings.
    /// </summary>
    [CreateAssetMenu(fileName = "New Rocket Config", menuName = "Hanabi Canvas/Config/Rocket Config")]
    public class RocketConfigSO : ScriptableObject
    {
        // ---- Constants ----
        private const float MIN_HEAD_SIZE = 0.01f;
        private const float MIN_HEAD_EMISSIVE = 0f;
        private const int MIN_TRAIL_PARTICLE_COUNT = 1;
        private const float MIN_TRAIL_PARTICLE_SIZE = 0.01f;
        private const float MIN_TRAIL_LIFETIME = 0.01f;
        private const float MIN_TRAIL_SPREAD = 0f;
        private const float MIN_TRAIL_GRAVITY = 0f;
        private const float MIN_TRAIL_DRIFT_SPEED = 0f;
        private static readonly Vector3 FALLBACK_SPAWN_POSITION = new Vector3(0f, -5f, 0f);
        private static readonly Vector3 FALLBACK_DESTINATION_POSITION = new Vector3(0f, 10f, 0f);

        // ---- Serialized Fields ----
        [Header("Spawn Positions")]
        [Tooltip("World positions the rocket can launch from (random pick)")]
        [SerializeField] private Vector3[] _spawnPositions = new Vector3[] { new Vector3(0f, -5f, 0f) };

        [Header("Destination Positions")]
        [Tooltip("World positions the rocket flies toward (random pick)")]
        [SerializeField] private Vector3[] _destinationPositions = new Vector3[] { new Vector3(0f, 10f, 0f) };

        [Header("Path Behaviours")]
        [Tooltip("Rocket path strategies (random pick per launch)")]
        [SerializeField] private RocketPathSO[] _pathBehaviours;

        [Header("Rocket Head")]
        [Tooltip("Size of the rocket head particle in world units")]
        [Min(0.01f)]
        [SerializeField] private float _headSize = 0.4f;

        [Tooltip("Color of the rocket head")]
        [SerializeField] private Color _headColor = new Color(1f, 0.9f, 0.6f, 1f);

        [Tooltip("HDR emissive multiplier for the rocket head (values > 1 trigger Bloom)")]
        [Min(0f)]
        [SerializeField] private float _headEmissive = 2.0f;

        [Header("Trail")]
        [Tooltip("Number of trail spark particles")]
        [Min(1)]
        [SerializeField] private int _trailParticleCount = 30;

        [Tooltip("Size of each trail spark in world units")]
        [Min(0.01f)]
        [SerializeField] private float _trailParticleSize = 0.15f;

        [Tooltip("Lifetime of each trail spark in seconds")]
        [Min(0.01f)]
        [SerializeField] private float _trailLifetime = 0.4f;

        [Tooltip("Random offset radius for trail spawn position")]
        [Min(0f)]
        [SerializeField] private float _trailSpread = 0.08f;

        [Tooltip("Color of trail sparks")]
        [SerializeField] private Color _trailColor = new Color(1f, 0.6f, 0.2f, 1f);

        [Tooltip("Trail alpha over normalized lifetime (0=birth, 1=death)")]
        [SerializeField] private AnimationCurve _trailAlphaOverLife = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Tooltip("Trail size multiplier over normalized lifetime")]
        [SerializeField] private AnimationCurve _trailSizeOverLife = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);

        [Header("Trail Physics")]
        [Tooltip("Downward acceleration applied to trail sparks")]
        [Min(0f)]
        [SerializeField] private float _trailGravity = 2f;

        [Tooltip("Backward drift speed for trail sparks")]
        [Min(0f)]
        [SerializeField] private float _trailDriftSpeed = 1f;

        // ---- Properties ----

        /// <summary>World positions the rocket can launch from.</summary>
        public Vector3[] SpawnPositions => _spawnPositions;

        /// <summary>World positions the rocket flies toward.</summary>
        public Vector3[] DestinationPositions => _destinationPositions;

        /// <summary>Rocket path strategies available for selection.</summary>
        public RocketPathSO[] PathBehaviours => _pathBehaviours;

        /// <summary>Size of the rocket head particle in world units.</summary>
        public float HeadSize => _headSize;

        /// <summary>Color of the rocket head.</summary>
        public Color HeadColor => _headColor;

        /// <summary>HDR emissive multiplier for the rocket head.</summary>
        public float HeadEmissive => _headEmissive;

        /// <summary>Number of trail spark particles.</summary>
        public int TrailParticleCount => _trailParticleCount;

        /// <summary>Size of each trail spark in world units.</summary>
        public float TrailParticleSize => _trailParticleSize;

        /// <summary>Lifetime of each trail spark in seconds.</summary>
        public float TrailLifetime => _trailLifetime;

        /// <summary>Random offset radius for trail spawn position.</summary>
        public float TrailSpread => _trailSpread;

        /// <summary>Color of trail sparks.</summary>
        public Color TrailColor => _trailColor;

        /// <summary>Trail alpha over normalized lifetime.</summary>
        public AnimationCurve TrailAlphaOverLife => _trailAlphaOverLife;

        /// <summary>Trail size multiplier over normalized lifetime.</summary>
        public AnimationCurve TrailSizeOverLife => _trailSizeOverLife;

        /// <summary>Downward acceleration applied to trail sparks.</summary>
        public float TrailGravity => _trailGravity;

        /// <summary>Backward drift speed for trail sparks.</summary>
        public float TrailDriftSpeed => _trailDriftSpeed;

        // ---- Public Methods ----

        /// <summary>Returns a random spawn position, or a fallback if empty.</summary>
        public Vector3 GetRandomSpawnPosition()
        {
            if (_spawnPositions == null || _spawnPositions.Length == 0)
            {
                return FALLBACK_SPAWN_POSITION;
            }
            return _spawnPositions[Random.Range(0, _spawnPositions.Length)];
        }

        /// <summary>Returns a random destination position, or a fallback if empty.</summary>
        public Vector3 GetRandomDestinationPosition()
        {
            if (_destinationPositions == null || _destinationPositions.Length == 0)
            {
                return FALLBACK_DESTINATION_POSITION;
            }
            return _destinationPositions[Random.Range(0, _destinationPositions.Length)];
        }

        /// <summary>Returns a random path SO from the configured list, or null if empty.</summary>
        public RocketPathSO GetRandomPath()
        {
            if (_pathBehaviours == null || _pathBehaviours.Length == 0)
            {
                return null;
            }
            return _pathBehaviours[Random.Range(0, _pathBehaviours.Length)];
        }

        // ---- Unity Methods ----
        private void OnValidate()
        {
            _headSize = Mathf.Max(MIN_HEAD_SIZE, _headSize);
            _headEmissive = Mathf.Max(MIN_HEAD_EMISSIVE, _headEmissive);
            _trailParticleCount = Mathf.Max(MIN_TRAIL_PARTICLE_COUNT, _trailParticleCount);
            _trailParticleSize = Mathf.Max(MIN_TRAIL_PARTICLE_SIZE, _trailParticleSize);
            _trailLifetime = Mathf.Max(MIN_TRAIL_LIFETIME, _trailLifetime);
            _trailSpread = Mathf.Max(MIN_TRAIL_SPREAD, _trailSpread);
            _trailGravity = Mathf.Max(MIN_TRAIL_GRAVITY, _trailGravity);
            _trailDriftSpeed = Mathf.Max(MIN_TRAIL_DRIFT_SPEED, _trailDriftSpeed);
        }
    }
}
