// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.CameraSystem
{
    /// <summary>
    /// Sets the camera background to a solid dark sky color.
    /// </summary>
    public class SkyBackground : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("Sky")]
        [Tooltip("Background color for the night sky")]
        [SerializeField] private Color _skyColor = new Color(0.02f, 0.02f, 0.08f, 1f);

        [Header("References")]
        [Tooltip("Camera whose background color to set. Falls back to Camera.main")]
        [SerializeField] private Camera _camera;

        // ---- Unity Methods ----
        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera != null)
            {
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.backgroundColor = _skyColor;
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Initializes the component for testing.
        /// </summary>
        /// <param name="camera">Camera whose background to set.</param>
        /// <param name="skyColor">Background color for the night sky.</param>
        public void Initialize(Camera camera, Color skyColor)
        {
            _camera = camera;
            _skyColor = skyColor;
        }
    }
}
