// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Modes;

namespace HanabiCanvas.Runtime.GameFlow
{
    /// <summary>
    /// Top-level app state manager. Sits above <see cref="FireworkSessionManager"/>
    /// and controls which mode (Free, Challenge, Slideshow, Settings) is active.
    /// Reads/writes <see cref="AppStateVariableSO"/> for current app state.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("State")]
        [Tooltip("Shared app state variable — written on every state transition")]
        [SerializeField] private AppStateVariableSO _appState;

        [Header("Mode Controllers")]
        [Tooltip("Free Mode controller — sandbox mode with no constraints")]
        [SerializeField] private FreeModeController _freeModeController;

        [Tooltip("Challenge Mode controller — constraint-driven drawing")]
        [SerializeField] private ChallengeModeController _challengeModeController;

        [Tooltip("Slideshow controller — replays saved artworks")]
        [SerializeField] private SlideshowController _slideshowController;

        [Header("Shared Variables")]
        [Tooltip("True when in Challenge Mode, false when in Free Mode")]
        [SerializeField] private BoolVariableSO _isChallengeMode;

        [Header("Events — Listened")]
        [Tooltip("Raised when slideshow playback completes")]
        [SerializeField] private GameEventSO _onSlideshowComplete;

        [Tooltip("Raised when user requests slideshow exit")]
        [SerializeField] private GameEventSO _onSlideshowExitRequested;

        // ---- Private Fields ----
        private AppState _previousState;
        private AppState _trackedState;
        private bool _isHandlingStateChange;

        // ---- Properties ----

        /// <summary>Current application state.</summary>
        public AppState CurrentAppState => _appState != null ? _appState.Value : AppState.Menu;

        // ---- Unity Methods ----

        private void Awake()
        {
            _previousState = AppState.Menu;
            _trackedState = AppState.Menu;
        }

        private void OnEnable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged += HandleAppStateChanged;
            }

            if (_onSlideshowComplete != null)
            {
                _onSlideshowComplete.Register(HandleSlideshowComplete);
            }

            if (_onSlideshowExitRequested != null)
            {
                _onSlideshowExitRequested.Register(HandleSlideshowExitRequested);
            }
        }

        private void OnDisable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged -= HandleAppStateChanged;
            }

            if (_onSlideshowComplete != null)
            {
                _onSlideshowComplete.Unregister(HandleSlideshowComplete);
            }

            if (_onSlideshowExitRequested != null)
            {
                _onSlideshowExitRequested.Unregister(HandleSlideshowExitRequested);
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Transitions to the specified app state. Writes to the shared
        /// <see cref="AppStateVariableSO"/> — mode activation/deactivation
        /// happens reactively via <see cref="HandleAppStateChanged"/>.
        /// </summary>
        /// <param name="state">The target app state.</param>
        public void SetAppState(AppState state)
        {
            if (_appState == null)
            {
                return;
            }

            _appState.Value = state;
        }

        /// <summary>
        /// Sets which playing mode is active. Must be called before or when
        /// transitioning to <see cref="AppState.Playing"/>.
        /// </summary>
        /// <param name="isChallenge">True for Challenge Mode, false for Free Mode.</param>
        public void SetPlayingMode(bool isChallenge)
        {
            if (_isChallengeMode != null)
            {
                _isChallengeMode.Value = isChallenge;
            }
        }

        /// <summary>
        /// Restores the previous app state. Used when exiting Settings overlay.
        /// Writes to the variable — mode activation happens via the handler.
        /// </summary>
        public void RestorePreviousState()
        {
            if (_appState == null)
            {
                return;
            }

            if (_appState.Value == AppState.Settings)
            {
                _appState.Value = _previousState;
            }
        }

        /// <summary>
        /// Initializes the controller for testing without Unity serialization.
        /// Subscribes to the app state variable for reactive mode activation.
        /// </summary>
        public void Initialize(
            AppStateVariableSO appState,
            FreeModeController freeModeController,
            ChallengeModeController challengeModeController,
            SlideshowController slideshowController,
            BoolVariableSO isChallengeMode,
            GameEventSO onSlideshowComplete,
            GameEventSO onSlideshowExitRequested)
        {
            _appState = appState;
            _freeModeController = freeModeController;
            _challengeModeController = challengeModeController;
            _slideshowController = slideshowController;
            _isChallengeMode = isChallengeMode;
            _onSlideshowComplete = onSlideshowComplete;
            _onSlideshowExitRequested = onSlideshowExitRequested;

            // Subscribe here since OnEnable may have run before fields were set
            if (_appState != null)
            {
                _appState.OnValueChanged -= HandleAppStateChanged;
                _appState.OnValueChanged += HandleAppStateChanged;
            }
        }

        // ---- Private Methods ----

        private void HandleAppStateChanged(AppState newState)
        {
            if (_isHandlingStateChange)
            {
                return;
            }

            _isHandlingStateChange = true;

            AppState oldState = _trackedState;

            if (newState == oldState)
            {
                _isHandlingStateChange = false;
                return;
            }

            // Entering Settings: overlay — don't deactivate current mode
            if (newState == AppState.Settings)
            {
                _previousState = oldState;
                _trackedState = newState;
                _isHandlingStateChange = false;
                return;
            }

            // Leaving Settings: deactivate what was active before Settings
            if (oldState == AppState.Settings)
            {
                DeactivateCurrentMode(_previousState);
                _trackedState = newState;
                ActivateMode(newState);
                _isHandlingStateChange = false;
                return;
            }

            // Normal transition
            DeactivateCurrentMode(oldState);
            _trackedState = newState;
            ActivateMode(newState);
            _isHandlingStateChange = false;
        }

        private void DeactivateCurrentMode(AppState currentState)
        {
            switch (currentState)
            {
                case AppState.Playing:
                    if (_isChallengeMode != null && _isChallengeMode.Value)
                    {
                        if (_challengeModeController != null)
                        {
                            _challengeModeController.Deactivate();
                        }
                    }
                    else
                    {
                        if (_freeModeController != null)
                        {
                            _freeModeController.Deactivate();
                        }
                    }
                    break;

                case AppState.Slideshow:
                    if (_slideshowController != null)
                    {
                        _slideshowController.StopSlideshow();
                    }
                    break;
            }
        }

        private void ActivateMode(AppState state)
        {
            switch (state)
            {
                case AppState.Playing:
                    if (_isChallengeMode != null && _isChallengeMode.Value)
                    {
                        if (_challengeModeController != null)
                        {
                            _challengeModeController.Activate();
                        }
                    }
                    else
                    {
                        if (_freeModeController != null)
                        {
                            _freeModeController.Activate();
                        }
                    }
                    break;

                case AppState.Slideshow:
                    if (_slideshowController != null)
                    {
                        _slideshowController.StartSlideshow();
                    }
                    break;
            }
        }

        private void HandleSlideshowComplete()
        {
            SetAppState(AppState.Menu);
        }

        private void HandleSlideshowExitRequested()
        {
            SetAppState(AppState.Menu);
        }
    }
}
