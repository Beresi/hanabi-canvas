// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Manages rocket flight from spawn to destination and renders the rocket head
    /// plus a trail of spark particles using a procedural billboard mesh.
    /// Listens for launch requests via event channel and raises a firework request
    /// on arrival at the destination.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RocketController : MonoBehaviour
    {
        // ---- Constants ----
        private const int VERTS_PER_PARTICLE = 4;
        private const int TRIS_PER_PARTICLE = 6;

        // ---- Serialized Fields ----
        [Header("Configuration")]
        [Tooltip("Rocket configuration with spawn/destination positions and trail settings")]
        [SerializeField] private RocketConfigSO _rocketConfig;

        [Header("Events — Listened")]
        [Tooltip("Raised by session managers to request a rocket launch")]
        [SerializeField] private FireworkRequestEventSO _onRocketLaunchRequested;

        [Header("Events — Raised")]
        [Tooltip("Raised when rocket arrives at destination to trigger firework explosion")]
        [SerializeField] private FireworkRequestEventSO _onFireworkRequested;

        [Header("Shared Variables")]
        [Tooltip("Written true while rocket is flying, false on arrival")]
        [SerializeField] private BoolVariableSO _isRocketAscending;

        [Header("Rendering")]
        [Tooltip("Material using HanabiCanvas/FireworkParticle shader (additive blend)")]
        [SerializeField] private Material _rocketMaterial;

        // ---- Private Fields: Flight State ----
        private bool _isFlying;
        private Vector3 _spawnPosition;
        private Vector3 _destinationPosition;
        private RocketPathSO _activePath;
        private float _flightDuration;
        private float _flightElapsed;
        private FireworkRequest _pendingRequest;

        // ---- Private Fields: Head Particle ----
        private Vector3 _headPosition;
        private Vector3 _headVelocity;

        // ---- Private Fields: Trail Particles ----
        private FireworkParticle[] _trailParticles;
        private int _trailCount;
        private int _trailSpawnIndex;
        private float _trailSpawnTimer;
        private float _trailSpawnInterval;

        // ---- Private Fields: Mesh ----
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Camera _camera;
        private Vector3[] _vertices;
        private Color[] _colors;
        private Vector2[] _uvs;
        private int[] _triangles;
        private bool _isMeshInitialized;

        // ---- Properties ----

        /// <summary>Whether the rocket is currently in flight.</summary>
        public bool IsFlying => _isFlying;

        /// <summary>Current world position of the rocket head.</summary>
        public Vector3 HeadPosition => _headPosition;

        // ---- Unity Methods ----

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _camera = Camera.main;

            if (_camera == null)
            {
                Debug.LogWarning("[RocketController] Camera.main is null. Mesh rendering will be skipped.");
            }
        }

        private void OnEnable()
        {
            if (_onRocketLaunchRequested != null)
            {
                _onRocketLaunchRequested.Unregister(HandleRocketLaunchRequested);
                _onRocketLaunchRequested.Register(HandleRocketLaunchRequested);
            }
        }

        private void OnDisable()
        {
            if (_onRocketLaunchRequested != null)
            {
                _onRocketLaunchRequested.Unregister(HandleRocketLaunchRequested);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (_isFlying)
            {
                _flightElapsed += deltaTime;
                float t = Mathf.Clamp01(_flightElapsed / _flightDuration);

                // Update head position and velocity from path
                _headPosition = _activePath.Evaluate(_spawnPosition, _destinationPosition, t);
                _headVelocity = _activePath.EvaluateVelocity(_spawnPosition, _destinationPosition, t);

                // Spawn trail particles (round-robin)
                _trailSpawnTimer += deltaTime;
                while (_trailSpawnTimer >= _trailSpawnInterval)
                {
                    _trailSpawnTimer -= _trailSpawnInterval;
                    SpawnTrailParticle();
                }

                // Update trail particles
                UpdateTrailParticles(deltaTime);

                // Check arrival
                if (t >= 1f)
                {
                    OnRocketArrived();
                }
            }
            else if (HasAliveTrailParticles())
            {
                // Continue updating trail particles after rocket arrives so they fade out
                UpdateTrailParticles(deltaTime);
            }
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                return;
            }

            if (!_isFlying && !HasAliveTrailParticles())
            {
                return;
            }

            if (!_isMeshInitialized)
            {
                return;
            }

            RebuildMesh();
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Initializes the controller with external references. Used by tests to bypass
        /// serialized field wiring and Awake lifecycle. Also registers the event listener
        /// on the provided launch event channel.
        /// </summary>
        /// <param name="rocketConfig">Rocket configuration SO.</param>
        /// <param name="onRocketLaunchRequested">Event to listen for launch requests.</param>
        /// <param name="onFireworkRequested">Event to raise when rocket arrives.</param>
        /// <param name="isRocketAscending">Shared bool written during flight.</param>
        public void Initialize(
            RocketConfigSO rocketConfig,
            FireworkRequestEventSO onRocketLaunchRequested,
            FireworkRequestEventSO onFireworkRequested,
            BoolVariableSO isRocketAscending)
        {
            // Unregister from previous event if switching references
            if (_onRocketLaunchRequested != null)
            {
                _onRocketLaunchRequested.Unregister(HandleRocketLaunchRequested);
            }

            _rocketConfig = rocketConfig;
            _onRocketLaunchRequested = onRocketLaunchRequested;
            _onFireworkRequested = onFireworkRequested;
            _isRocketAscending = isRocketAscending;

            // Register on the new event channel
            if (_onRocketLaunchRequested != null)
            {
                _onRocketLaunchRequested.Register(HandleRocketLaunchRequested);
            }
        }

        // ---- Private Methods ----

        private void HandleRocketLaunchRequested(FireworkRequest request)
        {
            if (_rocketConfig == null)
            {
                Debug.LogWarning("[RocketController] No RocketConfigSO assigned. Passing request through immediately.");
                if (_onFireworkRequested != null)
                {
                    _onFireworkRequested.Raise(request);
                }
                return;
            }

            _activePath = _rocketConfig.GetRandomPath();
            if (_activePath == null)
            {
                Debug.LogWarning("[RocketController] RocketConfigSO returned no path. Passing request through immediately.");
                if (_onFireworkRequested != null)
                {
                    _onFireworkRequested.Raise(request);
                }
                return;
            }

            _pendingRequest = request;
            _spawnPosition = _rocketConfig.GetRandomSpawnPosition();
            _destinationPosition = _rocketConfig.GetRandomDestinationPosition();
            _flightDuration = _activePath.GetFlightDuration(_spawnPosition, _destinationPosition);
            _flightElapsed = 0f;

            // Initialize trail
            _trailCount = _rocketConfig.TrailParticleCount;

            if (_trailParticles == null || _trailParticles.Length < _trailCount)
            {
                _trailParticles = new FireworkParticle[_trailCount];
            }

            // Zero out all trail particles
            for (int i = 0; i < _trailCount; i++)
            {
                _trailParticles[i].Life = 0f;
                _trailParticles[i].Size = 0f;
            }

            _trailSpawnIndex = 0;
            _trailSpawnTimer = 0f;
            _trailSpawnInterval = _trailCount > 0
                ? _rocketConfig.TrailLifetime / _trailCount
                : 1f;

            // Initialize mesh if needed (1 head + _trailCount trail particles)
            int totalParticles = 1 + _trailCount;
            if (!_isMeshInitialized || _mesh == null
                || _vertices.Length < totalParticles * VERTS_PER_PARTICLE)
            {
                InitializeMesh(totalParticles);
            }

            _headPosition = _spawnPosition;
            _headVelocity = Vector3.zero;
            _isFlying = true;

            if (_isRocketAscending != null)
            {
                _isRocketAscending.Value = true;
            }
        }

        private void SpawnTrailParticle()
        {
            if (_trailCount <= 0)
            {
                return;
            }

            int index = _trailSpawnIndex;
            _trailSpawnIndex = (_trailSpawnIndex + 1) % _trailCount;

            Vector3 offset = Random.insideUnitSphere * _rocketConfig.TrailSpread;
            Vector3 driftDir = _headVelocity.sqrMagnitude > 0.001f
                ? -_headVelocity.normalized
                : Vector3.down;

            _trailParticles[index].Position = _headPosition + offset;
            _trailParticles[index].Velocity = driftDir * _rocketConfig.TrailDriftSpeed;
            _trailParticles[index].Color = _rocketConfig.TrailColor;
            _trailParticles[index].Size = _rocketConfig.TrailParticleSize;
            _trailParticles[index].Life = _rocketConfig.TrailLifetime;
            _trailParticles[index].MaxLife = _rocketConfig.TrailLifetime;
            _trailParticles[index].RandomSeed = Random.value;
            _trailParticles[index].BaseColor = _rocketConfig.TrailColor;
        }

        private void UpdateTrailParticles(float deltaTime)
        {
            for (int i = 0; i < _trailCount; i++)
            {
                if (_trailParticles[i].Life <= 0f)
                {
                    continue;
                }

                _trailParticles[i].Life -= deltaTime;
                if (_trailParticles[i].Life < 0f)
                {
                    _trailParticles[i].Life = 0f;
                }

                // Apply gravity
                _trailParticles[i].Velocity += Vector3.down * _rocketConfig.TrailGravity * deltaTime;

                // Integrate position
                _trailParticles[i].Position += _trailParticles[i].Velocity * deltaTime;

                // Compute normalized progress (0 = birth, 1 = death)
                float progress = _trailParticles[i].MaxLife > 0f
                    ? (_trailParticles[i].MaxLife - _trailParticles[i].Life) / _trailParticles[i].MaxLife
                    : 1f;

                // Update size and alpha from curves
                _trailParticles[i].Size = _rocketConfig.TrailParticleSize
                    * _rocketConfig.TrailSizeOverLife.Evaluate(progress);

                Color c = _trailParticles[i].Color;
                _trailParticles[i].Color = new Color(
                    c.r, c.g, c.b,
                    _rocketConfig.TrailAlphaOverLife.Evaluate(progress));
            }
        }

        private void OnRocketArrived()
        {
            _isFlying = false;

            if (_isRocketAscending != null)
            {
                _isRocketAscending.Value = false;
            }

            // Modify the pending request to use the rocket's destination position
            _pendingRequest.Position = _destinationPosition;

            if (_onFireworkRequested != null)
            {
                _onFireworkRequested.Raise(_pendingRequest);
            }
        }

        private void InitializeMesh(int totalParticles)
        {
            int vertexCount = totalParticles * VERTS_PER_PARTICLE;
            int triangleCount = totalParticles * TRIS_PER_PARTICLE;

            _vertices = new Vector3[vertexCount];
            _colors = new Color[vertexCount];
            _uvs = new Vector2[vertexCount];
            _triangles = new int[triangleCount];

            for (int i = 0; i < totalParticles; i++)
            {
                int vertBase = i * VERTS_PER_PARTICLE;
                int triBase = i * TRIS_PER_PARTICLE;

                _uvs[vertBase + 0] = new Vector2(0f, 0f);
                _uvs[vertBase + 1] = new Vector2(1f, 0f);
                _uvs[vertBase + 2] = new Vector2(1f, 1f);
                _uvs[vertBase + 3] = new Vector2(0f, 1f);

                _triangles[triBase + 0] = vertBase + 0;
                _triangles[triBase + 1] = vertBase + 1;
                _triangles[triBase + 2] = vertBase + 2;
                _triangles[triBase + 3] = vertBase + 0;
                _triangles[triBase + 4] = vertBase + 2;
                _triangles[triBase + 5] = vertBase + 3;
            }

            // Destroy old mesh if it exists to prevent leak
            if (_mesh != null)
            {
                Destroy(_mesh);
            }

            _mesh = new Mesh();
            _mesh.name = "RocketParticles";
            _mesh.MarkDynamic();

            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;
            _mesh.colors = _colors;
            _mesh.triangles = _triangles;
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 200f);

            _meshFilter.mesh = _mesh;

            if (_rocketMaterial != null)
            {
                _meshRenderer.sharedMaterial = _rocketMaterial;

                if (_rocketMaterial.mainTexture != null)
                {
                    _rocketMaterial.EnableKeyword("_USE_TEXTURE");
                }
                else
                {
                    _rocketMaterial.DisableKeyword("_USE_TEXTURE");
                }
            }

            _isMeshInitialized = true;
        }

        private void RebuildMesh()
        {
            Transform cameraTransform = _camera.transform;
            Vector3 cameraRight = cameraTransform.right;
            Vector3 cameraUp = cameraTransform.up;

            // Convert world positions to local space so mesh renders correctly
            // regardless of where this GameObject is placed in the scene
            Vector3 localOffset = transform.position;

            int globalIndex = 0;

            // Head particle (index 0)
            int vertBase = 0;
            if (_isFlying)
            {
                float halfSize = _rocketConfig.HeadSize * 0.5f;
                BuildBillboardQuad(vertBase, _headPosition - localOffset, halfSize, cameraRight, cameraUp);

                Color headColor = _rocketConfig.HeadColor * _rocketConfig.HeadEmissive;
                headColor.a = 1f;
                _colors[vertBase + 0] = headColor;
                _colors[vertBase + 1] = headColor;
                _colors[vertBase + 2] = headColor;
                _colors[vertBase + 3] = headColor;
            }
            else
            {
                // Degenerate quad
                _vertices[vertBase + 0] = Vector3.zero;
                _vertices[vertBase + 1] = Vector3.zero;
                _vertices[vertBase + 2] = Vector3.zero;
                _vertices[vertBase + 3] = Vector3.zero;

                _colors[vertBase + 0] = Color.clear;
                _colors[vertBase + 1] = Color.clear;
                _colors[vertBase + 2] = Color.clear;
                _colors[vertBase + 3] = Color.clear;
            }
            globalIndex = 1;

            // Trail particles
            for (int i = 0; i < _trailCount; i++)
            {
                vertBase = globalIndex * VERTS_PER_PARTICLE;

                if (_trailParticles[i].Life <= 0f || _trailParticles[i].Size <= 0f)
                {
                    // Degenerate quad
                    _vertices[vertBase + 0] = Vector3.zero;
                    _vertices[vertBase + 1] = Vector3.zero;
                    _vertices[vertBase + 2] = Vector3.zero;
                    _vertices[vertBase + 3] = Vector3.zero;

                    _colors[vertBase + 0] = Color.clear;
                    _colors[vertBase + 1] = Color.clear;
                    _colors[vertBase + 2] = Color.clear;
                    _colors[vertBase + 3] = Color.clear;
                }
                else
                {
                    float halfSize = _trailParticles[i].Size * 0.5f;
                    BuildBillboardQuad(
                        vertBase,
                        _trailParticles[i].Position - localOffset,
                        halfSize,
                        cameraRight,
                        cameraUp);

                    Color color = _trailParticles[i].Color;
                    _colors[vertBase + 0] = color;
                    _colors[vertBase + 1] = color;
                    _colors[vertBase + 2] = color;
                    _colors[vertBase + 3] = color;
                }

                globalIndex++;
            }

            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
        }

        private void BuildBillboardQuad(int vertBase, Vector3 pos, float halfSize,
            Vector3 cameraRight, Vector3 cameraUp)
        {
            Vector3 right = cameraRight * halfSize;
            Vector3 up = cameraUp * halfSize;

            _vertices[vertBase + 0] = pos - right - up;
            _vertices[vertBase + 1] = pos + right - up;
            _vertices[vertBase + 2] = pos + right + up;
            _vertices[vertBase + 3] = pos - right + up;
        }

        private bool HasAliveTrailParticles()
        {
            if (_trailParticles == null)
            {
                return false;
            }

            for (int i = 0; i < _trailCount; i++)
            {
                if (_trailParticles[i].Life > 0f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
