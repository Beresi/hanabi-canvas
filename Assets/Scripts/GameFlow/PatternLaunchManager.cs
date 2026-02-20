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
    /// Manages launching a saved pattern from the pattern library as a firework.
    /// Runs a parallel state machine (Idle -> Launching -> Watching -> Resetting -> Idle)
    /// independent of the main drawing session flow.
    /// </summary>
    public class PatternLaunchManager : MonoBehaviour
    {
        private enum LaunchState
        {
            Idle,
            Launching,
            Ascending,
            Watching,
            Resetting
        }

        // ---- Serialized Fields ----

        [Header("Pattern Data")]
        [SerializeField] private PatternListSO _patternLibrary;
        [SerializeField] private IntVariableSO _selectedPatternIndex;

        [Header("Events — Listened")]
        [SerializeField] private GameEventSO _onLaunchPattern;

        [Header("Events — Raised")]
        [SerializeField] private FireworkRequestEventSO _onFireworkRequested;

        [Header("Camera Reference")]
        [SerializeField] private CameraController _cameraController;

        [Header("Shared Variables")]
        [SerializeField] private BoolVariableSO _isFireworkPlaying;

        [Header("Firework Spawn")]
        [SerializeField] private Transform _fireworkSpawnPoint;

        [Header("Rocket System")]
        [Tooltip("Whether the rocket launch system is active")]
        [SerializeField] private BoolVariableSO _isRocketEnabled;

        [Tooltip("Whether a rocket is currently ascending")]
        [SerializeField] private BoolVariableSO _isRocketAscending;

        [Tooltip("Raised to request a rocket launch")]
        [SerializeField] private FireworkRequestEventSO _onRocketLaunchRequested;

        [Header("UI")]
        [SerializeField] private GameObject _drawingScreen;

        // ---- Private Fields ----
        private LaunchState _currentState;
        private bool _hasRequested;
        private bool _isResettingStarted;

        // ---- Unity Lifecycle ----

        private void OnEnable()
        {
            if (_onLaunchPattern != null)
            {
                _onLaunchPattern.Register(HandleLaunchPattern);
            }
        }

        private void OnDisable()
        {
            if (_onLaunchPattern != null)
            {
                _onLaunchPattern.Unregister(HandleLaunchPattern);
            }
        }

        private void Update()
        {
            switch (_currentState)
            {
                case LaunchState.Idle:
                    break;
                case LaunchState.Launching:
                    UpdateLaunching();
                    break;
                case LaunchState.Ascending:
                    UpdateAscending();
                    break;
                case LaunchState.Watching:
                    UpdateWatching();
                    break;
                case LaunchState.Resetting:
                    UpdateResetting();
                    break;
            }
        }

        // ---- Event Handlers ----

        private void HandleLaunchPattern()
        {
            if (_currentState != LaunchState.Idle)
            {
                return;
            }

            if (_patternLibrary == null || _patternLibrary.Count == 0)
            {
                return;
            }

            _currentState = LaunchState.Launching;
        }

        // ---- State Update Methods ----

        private void UpdateLaunching()
        {
            if (_hasRequested)
            {
                return;
            }

            int index = _selectedPatternIndex != null ? _selectedPatternIndex.Value : 0;
            index = Mathf.Clamp(index, 0, _patternLibrary.Count - 1);

            FireworkPattern pattern = _patternLibrary.GetAt(index);

            if (pattern.Pixels == null || pattern.Pixels.Length == 0)
            {
                Debug.LogWarning("[PatternLaunchManager] Selected pattern has no pixels.", this);
                _currentState = LaunchState.Idle;
                return;
            }

            Vector3 spawnPosition = _fireworkSpawnPoint != null
                ? _fireworkSpawnPoint.position
                : Vector3.up * 10f;

            FireworkRequest request = new FireworkRequest
            {
                Position = spawnPosition,
                Pattern = pattern.Pixels,
                PatternWidth = pattern.Width,
                PatternHeight = pattern.Height
            };

            // Check if rocket system is enabled
            bool isRocketEnabled = _isRocketEnabled != null && _isRocketEnabled.Value;

            if (isRocketEnabled && _onRocketLaunchRequested != null)
            {
                _onRocketLaunchRequested.Raise(request);
            }
            else if (_onFireworkRequested != null)
            {
                _onFireworkRequested.Raise(request);
            }

            _hasRequested = true;

            if (_drawingScreen != null)
            {
                _drawingScreen.SetActive(false);
            }

            if (_cameraController != null)
            {
                _cameraController.TransitionToSkyView();
            }

            // Transition to Ascending if rocket enabled, otherwise Watching
            if (isRocketEnabled)
            {
                _currentState = LaunchState.Ascending;
            }
            else
            {
                _currentState = LaunchState.Watching;
            }
        }

        private void UpdateAscending()
        {
            // Wait for rocket to finish ascending
            if (_isRocketAscending != null && _isRocketAscending.Value)
            {
                return;
            }

            // Rocket has arrived — transition to watching firework
            _currentState = LaunchState.Watching;
        }

        private void UpdateWatching()
        {
            if (_isFireworkPlaying == null || !_isFireworkPlaying.Value)
            {
                _currentState = LaunchState.Resetting;
            }
        }

        private void UpdateResetting()
        {
            if (!_isResettingStarted)
            {
                _isResettingStarted = true;

                if (_cameraController != null)
                {
                    _cameraController.TransitionToCanvasView();
                }
            }

            if (_cameraController != null && _cameraController.IsTransitioning)
            {
                return;
            }

            if (_drawingScreen != null)
            {
                _drawingScreen.SetActive(true);
            }

            _hasRequested = false;
            _isResettingStarted = false;
            _currentState = LaunchState.Idle;
        }
    }
}
