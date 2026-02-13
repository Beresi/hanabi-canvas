// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.CameraSystem
{
    /// <summary>
    /// Controls camera transitions between canvas and sky views, and camera shake effects.
    /// Reads all configuration from a <see cref="CameraConfigSO"/>.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("Configuration")]
        [Tooltip("Camera configuration with view positions, transition settings, and shake parameters")]
        [SerializeField] private CameraConfigSO _config;

        [Header("References")]
        [Tooltip("Camera to control. Falls back to Camera.main if null")]
        [SerializeField] private Camera _camera;

        // ---- Private Fields ----

        // Transition state
        private Vector3 _transitionStartPosition;
        private Quaternion _transitionStartRotation;
        private Vector3 _transitionEndPosition;
        private Quaternion _transitionEndRotation;
        private float _transitionElapsed;
        private float _transitionDuration;
        private bool _isTransitioning;

        // Shake state
        private float _shakeElapsed;
        private float _shakeDuration;
        private float _shakeIntensity;
        private bool _isShaking;
        private Vector3 _preShakePosition;

        // ---- Properties ----

        /// <summary>Whether the camera is currently transitioning between views.</summary>
        public bool IsTransitioning => _isTransitioning;

        // ---- Unity Methods ----
        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_config == null)
            {
                Debug.LogWarning("[CameraController] CameraConfigSO is not assigned.", this);
            }
        }

        private void Start()
        {
            if (_camera != null && _config != null)
            {
                _camera.transform.position = _config.CanvasViewPosition;
                _camera.transform.rotation = _config.CanvasViewRotation;
            }
        }

        private void Update()
        {
            if (_isTransitioning)
            {
                UpdateTransition();
            }

            if (_isShaking)
            {
                UpdateShake();
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Transitions the camera from its current position to the sky view defined in config.
        /// </summary>
        public void TransitionToSkyView()
        {
            if (_config == null || _camera == null)
            {
                return;
            }

            StartTransition(
                _camera.transform.position,
                _camera.transform.rotation,
                _config.SkyViewPosition,
                _config.SkyViewRotation);
        }

        /// <summary>
        /// Transitions the camera from its current position to the canvas view defined in config.
        /// </summary>
        public void TransitionToCanvasView()
        {
            if (_config == null || _camera == null)
            {
                return;
            }

            StartTransition(
                _camera.transform.position,
                _camera.transform.rotation,
                _config.CanvasViewPosition,
                _config.CanvasViewRotation);
        }

        /// <summary>
        /// Triggers a decaying camera shake using burst intensity and duration from config.
        /// </summary>
        public void TriggerBurstShake()
        {
            if (_config == null || _camera == null)
            {
                return;
            }

            if (_config.BurstShakeIntensity <= 0f || _config.BurstShakeDuration <= 0f)
            {
                return;
            }

            _shakeElapsed = 0f;
            _shakeDuration = _config.BurstShakeDuration;
            _shakeIntensity = _config.BurstShakeIntensity;
            _isShaking = true;
            _preShakePosition = _camera.transform.position;
        }

        /// <summary>
        /// Initializes the controller for testing, bypassing serialized field wiring.
        /// </summary>
        /// <param name="config">Camera configuration SO.</param>
        /// <param name="camera">Camera to control.</param>
        public void Initialize(CameraConfigSO config, Camera camera)
        {
            _config = config;
            _camera = camera;
        }

        // ---- Private Methods ----
        private void StartTransition(Vector3 startPos, Quaternion startRot,
            Vector3 endPos, Quaternion endRot)
        {
            _transitionStartPosition = startPos;
            _transitionStartRotation = startRot;
            _transitionEndPosition = endPos;
            _transitionEndRotation = endRot;
            _transitionElapsed = 0f;
            _transitionDuration = _config.TransitionDuration;
            _isTransitioning = true;
        }

        private void UpdateTransition()
        {
            _transitionElapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(_transitionElapsed / _transitionDuration);
            float curveValue = _config.TransitionCurve.Evaluate(normalizedTime);

            _camera.transform.position = Vector3.Lerp(
                _transitionStartPosition, _transitionEndPosition, curveValue);
            _camera.transform.rotation = Quaternion.Slerp(
                _transitionStartRotation, _transitionEndRotation, curveValue);

            if (normalizedTime >= 1f)
            {
                _isTransitioning = false;
                _camera.transform.position = _transitionEndPosition;
                _camera.transform.rotation = _transitionEndRotation;
            }
        }

        private void UpdateShake()
        {
            _shakeElapsed += Time.deltaTime;

            if (_shakeElapsed >= _shakeDuration)
            {
                _isShaking = false;
                _camera.transform.position = _preShakePosition;
                return;
            }

            float progress = _shakeElapsed / _shakeDuration;
            float decayingIntensity = _shakeIntensity * (1f - progress);

            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * decayingIntensity,
                Random.Range(-1f, 1f) * decayingIntensity,
                0f);

            _camera.transform.position = _preShakePosition + offset;
        }
    }
}
