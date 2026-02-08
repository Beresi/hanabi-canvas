// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime.Firework
{
    public class FireworkLauncher : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("Events")]
        [Tooltip("Listened to — spawns a firework when raised")]
        [SerializeField] private GameEventSO _onLaunchFirework;

        [Header("Configuration")]
        [Tooltip("Firework configuration for spawned instances")]
        [SerializeField] private FireworkConfigSO _fireworkConfig;

        [Tooltip("Pixel data to read the pattern from")]
        [SerializeField] private PixelDataSO _pixelData;

        [Header("Prefab")]
        [Tooltip("Firework prefab to instantiate")]
        [SerializeField] private GameObject _fireworkPrefab;

        [Header("Spawn")]
        [Tooltip("World position where fireworks explode. Uses this transform if null.")]
        [SerializeField] private Transform _spawnPoint;

        // ---- Unity Methods ----
        private void OnEnable()
        {
            if (_onLaunchFirework != null)
            {
                _onLaunchFirework.Register(HandleLaunchFirework);
            }
        }

        private void OnDisable()
        {
            if (_onLaunchFirework != null)
            {
                _onLaunchFirework.Unregister(HandleLaunchFirework);
            }
        }

        // ---- Public Methods ----
        public void Initialize(GameEventSO onLaunchFirework, FireworkConfigSO config,
            PixelDataSO pixelData, GameObject fireworkPrefab, Transform spawnPoint)
        {
            _onLaunchFirework = onLaunchFirework;
            _fireworkConfig = config;
            _pixelData = pixelData;
            _fireworkPrefab = fireworkPrefab;
            _spawnPoint = spawnPoint;
        }

        // ---- Private Methods ----
        private void HandleLaunchFirework()
        {
            if (_fireworkPrefab == null)
            {
                Debug.LogWarning(
                    $"[{nameof(FireworkLauncher)}] Firework prefab is not assigned.", this);
                return;
            }

            if (_fireworkConfig == null)
            {
                Debug.LogWarning(
                    $"[{nameof(FireworkLauncher)}] FireworkConfigSO is not assigned.", this);
                return;
            }

            if (_pixelData == null)
            {
                Debug.LogWarning(
                    $"[{nameof(FireworkLauncher)}] PixelDataSO is not assigned.", this);
                return;
            }

            Vector3 spawnPosition = _spawnPoint != null ? _spawnPoint.position : transform.position;

            GameObject fireworkGO = Instantiate(_fireworkPrefab, spawnPosition, Quaternion.identity);
            FireworkInstance instance = fireworkGO.GetComponent<FireworkInstance>();

            if (instance == null)
            {
                Debug.LogWarning(
                    $"[{nameof(FireworkLauncher)}] Firework prefab is missing FireworkInstance component.", this);
                Destroy(fireworkGO);
                return;
            }

            instance.Initialize(_fireworkConfig, _pixelData, spawnPosition);
        }
    }
}
