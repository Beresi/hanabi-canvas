// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Manages spark particle bursts: listens for spark requests via event channel,
    /// queues them, plays them sequentially, runs multiple behaviours simultaneously
    /// per request, and renders the combined procedural billboard mesh.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SparkManager : MonoBehaviour
    {
        private const int VERTS_PER_PARTICLE = 4;
        private const int TRIS_PER_PARTICLE = 6;

        [Header("Behaviours")]
        [Tooltip("Spark behaviours to run simultaneously for each request")]
        [SerializeField] private SparkBehaviourSO[] _behaviours;

        [Header("Events")]
        [Tooltip("Event channel to listen for spark requests")]
        [SerializeField] private SparkRequestEventSO _onSparkRequested;

        [Header("Rendering")]
        [Tooltip("Material using HanabiCanvas/FireworkParticle shader (additive)")]
        [SerializeField] private Material _particleMaterial;

        private readonly Queue<SparkRequest> _requestQueue = new Queue<SparkRequest>();

        // Per-behaviour particle storage
        private SparkParticle[][] _behaviourParticles;
        private int[] _behaviourParticleCounts;
        private int _totalParticleCount;
        private bool _isPlaying;

        // Mesh
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Camera _camera;
        private Vector3[] _vertices;
        private Color[] _colors;
        private Vector2[] _uvs;
        private int[] _triangles;
        private bool _isMeshInitialized;

        /// <summary>Whether a spark is currently active.</summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>Total number of particles across all active behaviours.</summary>
        public int TotalParticleCount => _totalParticleCount;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _camera = Camera.main;

            if (_camera == null)
            {
                Debug.LogWarning("[SparkManager] Camera.main is null. Mesh rendering will be skipped.");
            }
        }

        private void OnEnable()
        {
            if (_onSparkRequested != null)
            {
                _onSparkRequested.Register(HandleSparkRequested);
            }
        }

        private void OnDisable()
        {
            if (_onSparkRequested != null)
            {
                _onSparkRequested.Unregister(HandleSparkRequested);
            }
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            float dt = Time.deltaTime;
            for (int b = 0; b < _behaviours.Length; b++)
            {
                if (_behaviours[b] != null && _behaviourParticleCounts[b] > 0)
                {
                    _behaviours[b].UpdateParticles(
                        _behaviourParticles[b],
                        _behaviourParticleCounts[b],
                        dt);
                }
            }

            CheckCompletion();
        }

        private void LateUpdate()
        {
            if (!_isPlaying || _camera == null)
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

        private void HandleSparkRequested(SparkRequest request)
        {
            if (!_isPlaying)
            {
                StartSpark(request);
            }
            else
            {
                _requestQueue.Enqueue(request);
            }
        }

        private void StartSpark(SparkRequest request)
        {
            if (_behaviours == null || _behaviours.Length == 0)
            {
                Debug.LogWarning("[SparkManager] No behaviours assigned. Cannot start spark.");
                return;
            }

            int behaviourCount = _behaviours.Length;

            // Allocate per-behaviour arrays if needed
            if (_behaviourParticles == null || _behaviourParticles.Length != behaviourCount)
            {
                _behaviourParticles = new SparkParticle[behaviourCount][];
                _behaviourParticleCounts = new int[behaviourCount];
            }

            _totalParticleCount = 0;

            for (int b = 0; b < behaviourCount; b++)
            {
                if (_behaviours[b] == null)
                {
                    _behaviourParticleCounts[b] = 0;
                    continue;
                }

                int count = _behaviours[b].GetParticleCount(request);
                _behaviourParticleCounts[b] = count;

                if (_behaviourParticles[b] == null || _behaviourParticles[b].Length < count)
                {
                    _behaviourParticles[b] = new SparkParticle[count];
                }

                _behaviours[b].InitializeParticles(_behaviourParticles[b], count, request);
                _totalParticleCount += count;
            }

            if (_totalParticleCount == 0)
            {
                return;
            }

            if (!_isMeshInitialized || _mesh == null
                || _vertices.Length < _totalParticleCount * VERTS_PER_PARTICLE)
            {
                InitializeMesh(_totalParticleCount);
            }

            _isPlaying = true;
        }

        private void CheckCompletion()
        {
            for (int b = 0; b < _behaviours.Length; b++)
            {
                if (_behaviours[b] == null || _behaviourParticleCounts[b] == 0)
                {
                    continue;
                }

                if (!_behaviours[b].IsComplete(_behaviourParticles[b], _behaviourParticleCounts[b]))
                {
                    return;
                }
            }

            _isPlaying = false;

            if (_requestQueue.Count > 0)
            {
                StartSpark(_requestQueue.Dequeue());
            }
        }

        private void InitializeMesh(int maxParticles)
        {
            int vertexCount = maxParticles * VERTS_PER_PARTICLE;
            int triangleCount = maxParticles * TRIS_PER_PARTICLE;

            _vertices = new Vector3[vertexCount];
            _colors = new Color[vertexCount];
            _uvs = new Vector2[vertexCount];
            _triangles = new int[triangleCount];

            for (int i = 0; i < maxParticles; i++)
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
            _mesh.name = "SparkParticles";
            _mesh.MarkDynamic();

            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;
            _mesh.colors = _colors;
            _mesh.triangles = _triangles;
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 200f);

            _meshFilter.mesh = _mesh;

            if (_particleMaterial != null)
            {
                _meshRenderer.sharedMaterial = _particleMaterial;
            }

            _isMeshInitialized = true;
        }

        private void RebuildMesh()
        {
            Transform cameraTransform = _camera.transform;
            Vector3 cameraRight = cameraTransform.right;
            Vector3 cameraUp = cameraTransform.up;

            int globalIndex = 0;

            for (int b = 0; b < _behaviours.Length; b++)
            {
                int count = _behaviourParticleCounts[b];
                SparkParticle[] particles = _behaviourParticles[b];

                for (int i = 0; i < count; i++)
                {
                    int vertBase = globalIndex * VERTS_PER_PARTICLE;

                    if (particles[i].Life <= 0f || particles[i].Size <= 0f)
                    {
                        _vertices[vertBase + 0] = Vector3.zero;
                        _vertices[vertBase + 1] = Vector3.zero;
                        _vertices[vertBase + 2] = Vector3.zero;
                        _vertices[vertBase + 3] = Vector3.zero;

                        _colors[vertBase + 0] = Color.clear;
                        _colors[vertBase + 1] = Color.clear;
                        _colors[vertBase + 2] = Color.clear;
                        _colors[vertBase + 3] = Color.clear;
                        globalIndex++;
                        continue;
                    }

                    float halfSize = particles[i].Size * 0.5f;
                    Vector3 right = cameraRight * halfSize;
                    Vector3 up = cameraUp * halfSize;
                    Vector3 pos = particles[i].Position;

                    _vertices[vertBase + 0] = pos - right - up;
                    _vertices[vertBase + 1] = pos + right - up;
                    _vertices[vertBase + 2] = pos + right + up;
                    _vertices[vertBase + 3] = pos - right + up;

                    Color color = particles[i].Color;
                    _colors[vertBase + 0] = color;
                    _colors[vertBase + 1] = color;
                    _colors[vertBase + 2] = color;
                    _colors[vertBase + 3] = color;
                    globalIndex++;
                }
            }

            // Clear remaining quads if mesh is larger than current total
            int totalVerts = _totalParticleCount * VERTS_PER_PARTICLE;
            for (int v = globalIndex * VERTS_PER_PARTICLE; v < totalVerts; v++)
            {
                _vertices[v] = Vector3.zero;
                _colors[v] = Color.clear;
            }

            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
        }
    }
}
