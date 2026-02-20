// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Firework;
using HanabiCanvas.Runtime.CameraSystem;

namespace HanabiCanvas.Runtime.GameFlow
{
    /// <summary>
    /// Manages the firework session cycle: Drawing -> Launching -> Watching -> Resetting -> Drawing.
    /// Listens for the launch event, constructs a <see cref="FireworkRequest"/> from pixel data,
    /// manages camera transitions, and resets the canvas after the firework completes.
    /// All cross-system communication uses SO channels — no direct MonoBehaviour references.
    /// </summary>
    public class FireworkSessionManager : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("Data References")]
        [Tooltip("Shared pixel data — read after PixelCanvas serializes it")]
        [SerializeField] private PixelDataSO _pixelData;

        [Tooltip("Canvas config — reads grid dimensions for FireworkRequest")]
        [SerializeField] private CanvasConfigSO _canvasConfig;

        [Header("Camera Reference")]
        [Tooltip("Called for view transitions")]
        [SerializeField] private CameraController _cameraController;

        [Header("Firework Spawn")]
        [Tooltip("World-space position where fireworks launch from")]
        [SerializeField] private Transform _fireworkSpawnPoint;

        [Header("Shared Variables")]
        [Tooltip("Controls canvas input — false during firework, true during drawing")]
        [SerializeField] private BoolVariableSO _isCanvasInputEnabled;

        [Tooltip("Shared game state — written on every state transition")]
        [SerializeField] private GameStateVariableSO _gameState;

        [Tooltip("Whether a firework is currently playing — written by FireworkManager, read here")]
        [SerializeField] private BoolVariableSO _isFireworkPlaying;

        [Header("Events — Listened")]
        [Tooltip("Raised by toolbar when user clicks Launch")]
        [SerializeField] private GameEventSO _onLaunchFirework;

        [Header("Events — Raised")]
        [Tooltip("Raised to request a firework from FireworkManager")]
        [SerializeField] private FireworkRequestEventSO _onFireworkRequested;

        [Tooltip("Raised to clear the canvas grid on reset")]
        [SerializeField] private GameEventSO _onCanvasCleared;

        // ---- Private Fields ----
        private GameState _currentState;
        private bool _hasRequestedFirework;
        private bool _isResettingStarted;
        private bool _isSessionEnabled = true;

        // ---- Properties ----

        /// <summary>Current game state.</summary>
        public GameState CurrentState => _currentState;

        // ---- Unity Lifecycle ----

        private void Awake()
        {
            _currentState = GameState.Drawing;
            _hasRequestedFirework = false;
            _isResettingStarted = false;
        }

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

        private void Update()
        {
            if (!_isSessionEnabled)
            {
                return;
            }

            switch (_currentState)
            {
                case GameState.Drawing:
                    UpdateDrawingState();
                    break;
                case GameState.Launching:
                    UpdateLaunchingState();
                    break;
                case GameState.Watching:
                    UpdateWatchingState();
                    break;
                case GameState.Resetting:
                    UpdateResettingState();
                    break;
            }
        }

        // ---- Event Handlers ----

        private void HandleLaunchFirework()
        {
            if (_currentState != GameState.Drawing)
            {
                return;
            }

            TransitionTo(GameState.Launching);
        }

        // ---- State Update Methods ----

        private void UpdateDrawingState()
        {
            // Idle — waiting for launch event.
        }

        private void UpdateLaunchingState()
        {
            if (_hasRequestedFirework)
            {
                return;
            }

            // Disable canvas input
            if (_isCanvasInputEnabled != null)
            {
                _isCanvasInputEnabled.Value = false;
            }

            // Check for empty canvas
            if (_pixelData == null || _pixelData.PixelCount == 0)
            {
                Debug.LogWarning("[FireworkSessionManager] No pixels to launch.", this);
                if (_isCanvasInputEnabled != null)
                {
                    _isCanvasInputEnabled.Value = true;
                }

                TransitionTo(GameState.Drawing);
                return;
            }

            // Construct FireworkRequest from PixelDataSO
            int pixelCount = _pixelData.PixelCount;
            PixelEntry[] pattern = new PixelEntry[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                pattern[i] = _pixelData.GetPixelAt(i);
            }

            Vector3 spawnPosition = _fireworkSpawnPoint != null
                ? _fireworkSpawnPoint.position
                : Vector3.up * 10f;

            FireworkRequest request = new FireworkRequest
            {
                Position = spawnPosition,
                Pattern = pattern,
                PatternWidth = _canvasConfig != null ? _canvasConfig.GridWidth : 32,
                PatternHeight = _canvasConfig != null ? _canvasConfig.GridHeight : 32
            };

            if (_onFireworkRequested != null)
            {
                _onFireworkRequested.Raise(request);
            }

            _hasRequestedFirework = true;

            // Start camera transition
            if (_cameraController != null)
            {
                _cameraController.TransitionToSkyView();
            }

            TransitionTo(GameState.Watching);
        }

        private void UpdateWatchingState()
        {
            if (_isFireworkPlaying == null || !_isFireworkPlaying.Value)
            {
                TransitionTo(GameState.Resetting);
            }
        }

        private void UpdateResettingState()
        {
            if (!_isResettingStarted)
            {
                _isResettingStarted = true;
                if (_cameraController != null)
                {
                    _cameraController.TransitionToCanvasView();
                }
            }

            // Wait for camera transition
            if (_cameraController != null && _cameraController.IsTransitioning)
            {
                return;
            }

            // Finalize reset
            if (_onCanvasCleared != null)
            {
                _onCanvasCleared.Raise();
            }

            if (_isCanvasInputEnabled != null)
            {
                _isCanvasInputEnabled.Value = true;
            }

            _hasRequestedFirework = false;
            _isResettingStarted = false;
            TransitionTo(GameState.Drawing);
        }

        // ---- Private Helpers ----

        private void TransitionTo(GameState newState)
        {
            _currentState = newState;
            if (_gameState != null)
            {
                _gameState.Value = newState;
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Enables the session state machine. Called by mode controllers on activation.
        /// </summary>
        public void EnableSession()
        {
            _isSessionEnabled = true;
        }

        /// <summary>
        /// Disables the session state machine. Called by mode controllers on deactivation.
        /// </summary>
        public void DisableSession()
        {
            _isSessionEnabled = false;
        }

        /// <summary>
        /// Resets the session to the Drawing state. Clears internal state,
        /// transitions camera to canvas view, and raises the canvas clear event.
        /// </summary>
        public void ResetSession()
        {
            _hasRequestedFirework = false;
            _isResettingStarted = false;

            if (_onCanvasCleared != null)
            {
                _onCanvasCleared.Raise();
            }

            if (_isCanvasInputEnabled != null)
            {
                _isCanvasInputEnabled.Value = true;
            }

            if (_cameraController != null)
            {
                _cameraController.TransitionToCanvasView();
            }

            TransitionTo(GameState.Drawing);
        }

        // ---- Public Methods (Testing) ----

        /// <summary>
        /// Initializes all fields directly. Used for testing without Unity serialization.
        /// Does NOT call Awake or register events — those happen via Unity lifecycle.
        /// </summary>
        public void Initialize(
            PixelDataSO pixelData,
            CanvasConfigSO canvasConfig,
            CameraController cameraController,
            BoolVariableSO isCanvasInputEnabled,
            GameStateVariableSO gameState,
            BoolVariableSO isFireworkPlaying,
            Transform fireworkSpawnPoint,
            GameEventSO onLaunchFirework,
            FireworkRequestEventSO onFireworkRequested,
            GameEventSO onCanvasCleared)
        {
            _pixelData = pixelData;
            _canvasConfig = canvasConfig;
            _cameraController = cameraController;
            _isCanvasInputEnabled = isCanvasInputEnabled;
            _gameState = gameState;
            _isFireworkPlaying = isFireworkPlaying;
            _fireworkSpawnPoint = fireworkSpawnPoint;
            _onLaunchFirework = onLaunchFirework;
            _onFireworkRequested = onFireworkRequested;
            _onCanvasCleared = onCanvasCleared;
        }
    }
}
