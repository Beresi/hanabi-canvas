// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class FireworkMeshRenderer : MonoBehaviour
    {
        // ---- Constants ----
        private const int VERTS_PER_PARTICLE = 4;
        private const int TRIS_PER_PARTICLE = 6;

        // ---- Serialized Fields ----
        [Header("References")]
        [Tooltip("The FireworkInstance to render particles from")]
        [SerializeField] private FireworkInstance _fireworkInstance;

        [Tooltip("Material used for particle rendering (additive shader)")]
        [SerializeField] private Material _particleMaterial;

        // ---- Private Fields ----
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Camera _camera;

        private Vector3[] _vertices;
        private Color[] _colors;
        private Vector2[] _uvs;
        private int[] _triangles;
        private bool _isInitialized;

        // ---- Unity Methods ----
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _camera = Camera.main;
            if (_camera == null)
            {
                Debug.LogWarning(
                    $"[{nameof(FireworkMeshRenderer)}] Camera.main is null.", this);
            }
        }

        private void LateUpdate()
        {
            if (_fireworkInstance == null || _camera == null)
            {
                return;
            }

            if (!_isInitialized && _fireworkInstance.Particles != null)
            {
                InitializeMesh(_fireworkInstance.ParticleCount);
            }

            if (!_isInitialized)
            {
                return;
            }

            RebuildMesh();
        }

        // ---- Public Methods ----
        public void Initialize(FireworkInstance instance)
        {
            _fireworkInstance = instance;
        }

        // ---- Private Methods ----
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

            _mesh = new Mesh();
            _mesh.name = "FireworkParticles";
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

            _isInitialized = true;
        }

        private void RebuildMesh()
        {
            ParticleData[] particles = _fireworkInstance.Particles;
            int count = _fireworkInstance.ParticleCount;

            Transform cameraTransform = _camera.transform;
            Vector3 cameraRight = cameraTransform.right;
            Vector3 cameraUp = cameraTransform.up;

            for (int i = 0; i < count; i++)
            {
                int vertBase = i * VERTS_PER_PARTICLE;

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
            }

            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
        }
    }
}
