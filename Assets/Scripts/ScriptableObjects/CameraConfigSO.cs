// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Configuration SO for camera positions, transitions, and screen-shake settings.
    /// </summary>
    [CreateAssetMenu(fileName = "New Camera Config", menuName = "Hanabi Canvas/Config/Camera Config")]
    public class CameraConfigSO : ScriptableObject
    {
        // ---- Constants ----
        private const float MIN_TRANSITION_DURATION = 0.1f;

        // ---- Serialized Fields ----
        [Header("Canvas View")]
        [Tooltip("World-space position of the camera when viewing the canvas")]
        [SerializeField] private Vector3 _canvasViewPosition = new Vector3(0f, 0f, -10f);

        [Tooltip("Euler rotation of the camera when viewing the canvas")]
        [SerializeField] private Vector3 _canvasViewRotationEuler = Vector3.zero;

        [Header("Sky View")]
        [Tooltip("World-space position of the camera when viewing the sky for fireworks")]
        [SerializeField] private Vector3 _skyViewPosition = new Vector3(0f, 10f, -15f);

        [Tooltip("Euler rotation of the camera when viewing the sky for fireworks")]
        [SerializeField] private Vector3 _skyViewRotationEuler = new Vector3(10f, 0f, 0f);

        [Header("Transition")]
        [Tooltip("Duration in seconds for the camera to transition between views")]
        [Min(0.1f)]
        [SerializeField] private float _transitionDuration = 1.0f;

        [Tooltip("Animation curve controlling the ease of camera transitions")]
        [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Camera Shake")]
        [Tooltip("Intensity of camera shake when a firework burst occurs")]
        [Min(0f)]
        [SerializeField] private float _burstShakeIntensity = 0.1f;

        [Tooltip("Duration in seconds of the camera shake effect on burst")]
        [Min(0f)]
        [SerializeField] private float _burstShakeDuration = 0.3f;

        // ---- Properties ----

        /// <summary>World-space position of the camera when viewing the canvas.</summary>
        public Vector3 CanvasViewPosition => _canvasViewPosition;

        /// <summary>Rotation of the camera when viewing the canvas.</summary>
        public Quaternion CanvasViewRotation => Quaternion.Euler(_canvasViewRotationEuler);

        /// <summary>World-space position of the camera when viewing the sky.</summary>
        public Vector3 SkyViewPosition => _skyViewPosition;

        /// <summary>Rotation of the camera when viewing the sky.</summary>
        public Quaternion SkyViewRotation => Quaternion.Euler(_skyViewRotationEuler);

        /// <summary>Duration in seconds for camera transitions between views.</summary>
        public float TransitionDuration => _transitionDuration;

        /// <summary>Animation curve controlling the ease of camera transitions.</summary>
        public AnimationCurve TransitionCurve => _transitionCurve;

        /// <summary>Intensity of camera shake when a firework burst occurs.</summary>
        public float BurstShakeIntensity => _burstShakeIntensity;

        /// <summary>Duration in seconds of the camera shake effect on burst.</summary>
        public float BurstShakeDuration => _burstShakeDuration;

        // ---- Validation ----
        private void OnValidate()
        {
            _transitionDuration = Mathf.Max(MIN_TRANSITION_DURATION, _transitionDuration);
            _burstShakeIntensity = Mathf.Max(0f, _burstShakeIntensity);
            _burstShakeDuration = Mathf.Max(0f, _burstShakeDuration);
        }
    }
}
