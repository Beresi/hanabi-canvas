// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.GameFlow;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Runtime.Modes
{
    /// <summary>
    /// Sandbox mode controller — no constraints, no timer, no pressure.
    /// Wraps the existing <see cref="FireworkSessionManager"/> session cycle.
    /// On firework completion, auto-saves the artwork to <see cref="DataManager"/>.
    /// </summary>
    public class FreeModeController : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("Session")]
        [Tooltip("The session manager that handles the Drawing->Launch->Watch->Reset cycle")]
        [SerializeField] private FireworkSessionManager _sessionManager;

        [Header("Data")]
        [Tooltip("Data manager for saving artworks")]
        [SerializeField] private DataManager _dataManager;

        [Tooltip("Shared pixel data — read to build ArtworkData on save")]
        [SerializeField] private PixelDataSO _pixelData;

        [Tooltip("Canvas config for reading grid dimensions")]
        [SerializeField] private CanvasConfigSO _canvasConfig;

        [Header("Shared Variables")]
        [Tooltip("Controls canvas input — set true on activate, false on deactivate")]
        [SerializeField] private BoolVariableSO _isCanvasInputEnabled;

        [Header("Events — Listened")]
        [Tooltip("Raised when all firework behaviours complete")]
        [SerializeField] private GameEventSO _onFireworkComplete;

        [Header("Events — Raised")]
        [Tooltip("Raised to clear the canvas on reset")]
        [SerializeField] private GameEventSO _onCanvasCleared;

        // ---- Private Fields ----
        private bool _isActive;
        private PixelEntry[] _savedPixels;
        private int _savedWidth;
        private int _savedHeight;
        private bool _hasPendingSave;

        // ---- Properties ----

        /// <summary>Whether Free Mode is currently active.</summary>
        public bool IsActive => _isActive;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_onFireworkComplete != null)
            {
                _onFireworkComplete.Register(HandleFireworkComplete);
            }
        }

        private void OnDisable()
        {
            if (_onFireworkComplete != null)
            {
                _onFireworkComplete.Unregister(HandleFireworkComplete);
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Activates Free Mode. Enables canvas input and the session manager.
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            _hasPendingSave = false;

            if (_isCanvasInputEnabled != null)
            {
                _isCanvasInputEnabled.Value = true;
            }

            if (_sessionManager != null)
            {
                _sessionManager.EnableSession();
            }
        }

        /// <summary>
        /// Deactivates Free Mode. Disables canvas input and the session manager.
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            _hasPendingSave = false;

            if (_isCanvasInputEnabled != null)
            {
                _isCanvasInputEnabled.Value = false;
            }

            if (_sessionManager != null)
            {
                _sessionManager.DisableSession();
            }
        }

        /// <summary>
        /// Initializes the controller for testing without Unity serialization.
        /// </summary>
        public void Initialize(
            FireworkSessionManager sessionManager,
            DataManager dataManager,
            PixelDataSO pixelData,
            CanvasConfigSO canvasConfig,
            BoolVariableSO isCanvasInputEnabled,
            GameEventSO onFireworkComplete,
            GameEventSO onCanvasCleared)
        {
            _sessionManager = sessionManager;
            _dataManager = dataManager;
            _pixelData = pixelData;
            _canvasConfig = canvasConfig;
            _isCanvasInputEnabled = isCanvasInputEnabled;
            _onFireworkComplete = onFireworkComplete;
            _onCanvasCleared = onCanvasCleared;
        }

        // ---- Private Methods ----

        private void HandleFireworkComplete()
        {
            if (!_isActive)
            {
                return;
            }

            SaveArtwork();
        }

        private void SaveArtwork()
        {
            if (_dataManager == null || _pixelData == null)
            {
                return;
            }

            if (_pixelData.PixelCount == 0)
            {
                return;
            }

            int pixelCount = _pixelData.PixelCount;
            PixelEntry[] pixels = new PixelEntry[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                pixels[i] = _pixelData.GetPixelAt(i);
            }

            int width = _canvasConfig != null ? _canvasConfig.GridWidth : _pixelData.Width;
            int height = _canvasConfig != null ? _canvasConfig.GridHeight : _pixelData.Height;

            ArtworkData artwork = new ArtworkData(
                System.Guid.NewGuid().ToString(),
                "Artwork " + (_dataManager.ArtworkCount + 1),
                pixels,
                width,
                height,
                System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _dataManager.AddArtwork(artwork);
        }
    }
}
