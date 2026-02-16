// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Manages firework particle bursts: listens for firework requests via event channel,
    /// queues them, plays them sequentially, runs multiple behaviours simultaneously
    /// per request, and renders the combined procedural billboard mesh.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class FireworkManager : MonoBehaviour
    {
        private const int VERTS_PER_PARTICLE = 4;
        private const int TRIS_PER_PARTICLE = 6;

        [Header("Behaviours")]
        [Tooltip("Firework behaviours to run simultaneously for each request")]
        [SerializeField] private FireworkBehaviourSO[] _behaviours;

        [Header("Events")]
        [Tooltip("Event channel to listen for firework requests")]
        [SerializeField] private FireworkRequestEventSO _onFireworkRequested;

        [Header("Shared Variables")]
        [Tooltip("Written true when playing, false when complete — read by FireworkSessionManager")]
        [SerializeField] private BoolVariableSO _isFireworkPlaying;

        [Header("Rendering")]
        [Tooltip("Material using HanabiCanvas/FireworkParticle shader (additive)")]
        [SerializeField] private Material _particleMaterial;

        private readonly Queue<FireworkRequest> _requestQueue = new Queue<FireworkRequest>();

        // Per-behaviour particle storage
        private FireworkParticle[][] _behaviourParticles;
        private int[] _behaviourParticleCounts;
        private int _totalParticleCount;
        private bool _isPlaying;

        // Per-behaviour elapsed time for effects
        private float[] _behaviourElapsedTimes;

        // Cached trail effect per behaviour (null if no trail on that behaviour)
        private TrailEffectSO[] _behaviourTrailEffects;

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

        /// <summary>Whether a firework is currently active.</summary>
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
                Debug.LogWarning("[FireworkManager] Camera.main is null. Mesh rendering will be skipped.");
            }
        }

        private void OnEnable()
        {
            if (_onFireworkRequested != null)
            {
                _onFireworkRequested.Register(HandleFireworkRequested);
            }
        }

        private void OnDisable()
        {
            if (_onFireworkRequested != null)
            {
                _onFireworkRequested.Unregister(HandleFireworkRequested);
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
                    // Undo previous gravity displacement before behaviour runs.
                    // This ensures behaviours that set Position absolutely (Pattern) or
                    // accumulate from Velocity (Burst) both start from a clean position.
                    int preCount = _behaviourParticleCounts[b];
                    FireworkParticle[] preParticles = _behaviourParticles[b];
                    for (int i = 0; i < preCount; i++)
                    {
                        if (preParticles[i].Life <= 0f)
                        {
                            continue;
                        }

                        preParticles[i].Position.y += preParticles[i].GravityDisplacementY;
                    }

                    _behaviours[b].UpdateParticles(
                        _behaviourParticles[b],
                        _behaviourParticleCounts[b],
                        dt);

                    // Reset Color.rgb to BaseColor before effects run.
                    // Behaviours only set alpha each frame; without this reset,
                    // multiplicative effects (e.g. emissive glow) would compound RGB.
                    int count = _behaviourParticleCounts[b];
                    FireworkParticle[] particles = _behaviourParticles[b];
                    for (int i = 0; i < count; i++)
                    {
                        if (particles[i].Life <= 0f)
                        {
                            continue;
                        }

                        Color bc = particles[i].BaseColor;
                        particles[i].Color = new Color(
                            bc.r, bc.g, bc.b, particles[i].Color.a);
                    }

                    _behaviourElapsedTimes[b] += dt;

                    FireworkEffectSO[] effects = _behaviours[b].Effects;
                    if (effects != null && effects.Length > 0)
                    {
                        for (int e = 0; e < effects.Length; e++)
                        {
                            effects[e].UpdateEffect(
                                _behaviourParticles[b],
                                _behaviourParticleCounts[b],
                                dt,
                                _behaviourElapsedTimes[b]);
                        }
                    }
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

        private void HandleFireworkRequested(FireworkRequest request)
        {
            if (!_isPlaying)
            {
                StartFirework(request);
            }
            else
            {
                _requestQueue.Enqueue(request);
            }
        }

        private void StartFirework(FireworkRequest request)
        {
            if (_behaviours == null || _behaviours.Length == 0)
            {
                Debug.LogWarning("[FireworkManager] No behaviours assigned. Cannot start firework.");
                return;
            }

            int behaviourCount = _behaviours.Length;

            // Allocate per-behaviour arrays if needed
            if (_behaviourParticles == null || _behaviourParticles.Length != behaviourCount)
            {
                _behaviourParticles = new FireworkParticle[behaviourCount][];
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
                    _behaviourParticles[b] = new FireworkParticle[count];
                }

                _behaviours[b].InitializeParticles(_behaviourParticles[b], count, request);

                // Initialize effects for this behaviour
                FireworkEffectSO[] effects = _behaviours[b].Effects;
                if (effects != null && effects.Length > 0)
                {
                    for (int e = 0; e < effects.Length; e++)
                    {
                        effects[e].InitializeEffect(_behaviourParticles[b], count);
                    }
                }

                _totalParticleCount += count;
            }

            // Allocate elapsed time and trail cache arrays
            int bCount = _behaviours.Length;
            if (_behaviourElapsedTimes == null || _behaviourElapsedTimes.Length != bCount)
            {
                _behaviourElapsedTimes = new float[bCount];
                _behaviourTrailEffects = new TrailEffectSO[bCount];
            }

            for (int b = 0; b < bCount; b++)
            {
                _behaviourElapsedTimes[b] = 0f;
                _behaviourTrailEffects[b] = null;

                FireworkEffectSO[] fx = _behaviours[b].Effects;
                if (fx != null && fx.Length > 0)
                {
                    for (int e = 0; e < fx.Length; e++)
                    {
                        if (fx[e] is TrailEffectSO trail)
                        {
                            _behaviourTrailEffects[b] = trail;
                            break;
                        }
                    }
                }
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

            if (_isFireworkPlaying != null)
            {
                _isFireworkPlaying.Value = true;
            }
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

            if (_isFireworkPlaying != null)
            {
                _isFireworkPlaying.Value = false;
            }

            if (_requestQueue.Count > 0)
            {
                StartFirework(_requestQueue.Dequeue());
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

                if (_particleMaterial.mainTexture != null)
                {
                    _particleMaterial.EnableKeyword("_USE_TEXTURE");
                }
                else
                {
                    _particleMaterial.DisableKeyword("_USE_TEXTURE");
                }
            }

            _isMeshInitialized = true;
        }

        private void RebuildMesh()
        {
            Transform cameraTransform = _camera.transform;
            Vector3 cameraRight = cameraTransform.right;
            Vector3 cameraUp = cameraTransform.up;
            Vector3 cameraForward = cameraTransform.forward;

            int globalIndex = 0;

            for (int b = 0; b < _behaviours.Length; b++)
            {
                int count = _behaviourParticleCounts[b];
                FireworkParticle[] particles = _behaviourParticles[b];
                TrailEffectSO trailEffect = _behaviourTrailEffects != null && b < _behaviourTrailEffects.Length
                    ? _behaviourTrailEffects[b]
                    : null;

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
                    Vector3 pos = particles[i].Position;
                    bool didTrail = false;

                    if (trailEffect != null)
                    {
                        Vector3 velocity = particles[i].Velocity;
                        float speed = velocity.magnitude;

                        if (speed > trailEffect.MinVelocityThreshold)
                        {
                            Vector3 velocityDir = velocity / speed;
                            Vector3 projectedDir = velocityDir
                                - Vector3.Dot(velocityDir, cameraForward) * cameraForward;
                            float projMag = projectedDir.magnitude;

                            if (projMag > 0.001f)
                            {
                                projectedDir /= projMag;
                                Vector3 perpDir = Vector3.Cross(projectedDir, cameraForward);

                                float stretchAmount = speed * trailEffect.StretchMultiplier;
                                if (stretchAmount > trailEffect.MaxStretchLength)
                                {
                                    stretchAmount = trailEffect.MaxStretchLength;
                                }

                                Vector3 right = perpDir * halfSize;
                                Vector3 up = projectedDir * (halfSize + stretchAmount);

                                _vertices[vertBase + 0] = pos - right - up;
                                _vertices[vertBase + 1] = pos + right - up;
                                _vertices[vertBase + 2] = pos + right + up;
                                _vertices[vertBase + 3] = pos - right + up;
                                didTrail = true;
                            }
                        }
                    }

                    if (!didTrail)
                    {
                        BuildBillboardQuad(vertBase, pos, halfSize, cameraRight, cameraUp);
                    }

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
    }
}
