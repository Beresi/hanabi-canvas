// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Firework;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Runtime.Modes
{
    /// <summary>
    /// Plays back saved artworks as sequential firework displays.
    /// Communicates with <see cref="FireworkManager"/> exclusively through
    /// <see cref="FireworkRequestEventSO"/> and listens for completion events.
    /// </summary>
    public class SlideshowController : MonoBehaviour
    {
        // ---- Enums ----
        private enum SlideshowState
        {
            Idle,
            Playing,
            Transitioning,
            Complete
        }

        // ---- Serialized Fields ----

        [Header("Config")]
        [Tooltip("Slideshow timing and playback settings")]
        [SerializeField] private SlideshowConfigSO _slideshowConfig;

        [Header("Data")]
        [Tooltip("Data manager to read artwork list from")]
        [SerializeField] private DataManager _dataManager;

        [Header("Firework Spawn")]
        [Tooltip("World-space position where slideshow fireworks launch from")]
        [SerializeField] private Transform _fireworkSpawnPoint;

        [Header("Events — Listened")]
        [Tooltip("Raised when all firework behaviours complete")]
        [SerializeField] private GameEventSO _onFireworkComplete;

        [Tooltip("Raised when user requests slideshow exit")]
        [SerializeField] private GameEventSO _onSlideshowExitRequested;

        [Header("Events — Raised")]
        [Tooltip("Raised to request a firework burst")]
        [SerializeField] private FireworkRequestEventSO _onFireworkRequested;

        [Tooltip("Raised when slideshow playback begins")]
        [SerializeField] private GameEventSO _onSlideshowStarted;

        [Tooltip("Raised when slideshow advances to next artwork")]
        [SerializeField] private GameEventSO _onSlideshowArtworkChanged;

        [Tooltip("Raised when slideshow finishes all artworks")]
        [SerializeField] private GameEventSO _onSlideshowComplete;

        [Tooltip("Typed event with the current artwork data")]
        [SerializeField] private ArtworkEventSO _onSlideshowArtworkStarted;

        [Header("Shared Variables")]
        [Tooltip("Current artwork index in the slideshow")]
        [SerializeField] private IntVariableSO _slideshowCurrentIndex;

        [Tooltip("Total number of artworks in the slideshow")]
        [SerializeField] private IntVariableSO _slideshowTotalCount;

        [Header("Rocket System")]
        [Tooltip("Whether the rocket launch system is active")]
        [SerializeField] private BoolVariableSO _isRocketEnabled;

        [Tooltip("Raised to request a rocket launch")]
        [SerializeField] private FireworkRequestEventSO _onRocketLaunchRequested;

        // ---- Private Fields ----
        private SlideshowState _currentState;
        private List<ArtworkData> _playlist;
        private int _currentIndex;
        private float _transitionTimer;
        private bool _isActive;

        // ---- Properties ----

        /// <summary>Whether the slideshow is currently playing.</summary>
        public bool IsPlaying => _currentState != SlideshowState.Idle;

        /// <summary>Current artwork index (0-based).</summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>Total number of artworks in the playlist.</summary>
        public int TotalCount => _playlist != null ? _playlist.Count : 0;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_onFireworkComplete != null)
            {
                _onFireworkComplete.Register(HandleFireworkComplete);
            }

            if (_onSlideshowExitRequested != null)
            {
                _onSlideshowExitRequested.Register(HandleSlideshowExitRequested);
            }
        }

        private void OnDisable()
        {
            if (_onFireworkComplete != null)
            {
                _onFireworkComplete.Unregister(HandleFireworkComplete);
            }

            if (_onSlideshowExitRequested != null)
            {
                _onSlideshowExitRequested.Unregister(HandleSlideshowExitRequested);
            }
        }

        private void Update()
        {
            if (_currentState == SlideshowState.Transitioning)
            {
                UpdateTransition();
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Begins slideshow playback from the first (or random) artwork.
        /// Reads the artwork list from <see cref="DataManager"/>.
        /// </summary>
        public void StartSlideshow()
        {
            if (_dataManager == null)
            {
                Debug.LogWarning("[SlideshowController] DataManager is not assigned.", this);
                return;
            }

            IReadOnlyList<ArtworkData> allArtworks = _dataManager.GetAllArtworks();

            if (allArtworks == null || allArtworks.Count == 0)
            {
                Debug.LogWarning("[SlideshowController] No artworks available for slideshow.", this);
                return;
            }

            // Build playlist
            _playlist = new List<ArtworkData>(allArtworks.Count);
            for (int i = 0; i < allArtworks.Count; i++)
            {
                _playlist.Add(allArtworks[i]);
            }

            // Shuffle if configured
            if (_slideshowConfig != null && _slideshowConfig.IsShuffled)
            {
                ShufflePlaylist();
            }

            _currentIndex = 0;
            _isActive = true;

            UpdateSharedVariables();

            if (_onSlideshowStarted != null)
            {
                _onSlideshowStarted.Raise();
            }

            PlayCurrentArtwork();
        }

        /// <summary>
        /// Stops slideshow playback and resets to idle state.
        /// </summary>
        public void StopSlideshow()
        {
            _currentState = SlideshowState.Idle;
            _isActive = false;
            _playlist = null;
            _currentIndex = 0;
        }

        /// <summary>
        /// Skips to the next artwork immediately.
        /// </summary>
        public void SkipToNext()
        {
            if (!_isActive || _playlist == null)
            {
                return;
            }

            AdvanceToNext();
        }

        /// <summary>
        /// Initializes the controller for testing without Unity serialization.
        /// </summary>
        public void Initialize(
            SlideshowConfigSO slideshowConfig,
            DataManager dataManager,
            Transform fireworkSpawnPoint,
            GameEventSO onFireworkComplete,
            GameEventSO onSlideshowExitRequested,
            FireworkRequestEventSO onFireworkRequested,
            GameEventSO onSlideshowStarted,
            GameEventSO onSlideshowArtworkChanged,
            GameEventSO onSlideshowComplete,
            ArtworkEventSO onSlideshowArtworkStarted,
            IntVariableSO slideshowCurrentIndex,
            IntVariableSO slideshowTotalCount,
            BoolVariableSO isRocketEnabled = null,
            FireworkRequestEventSO onRocketLaunchRequested = null)
        {
            _slideshowConfig = slideshowConfig;
            _dataManager = dataManager;
            _fireworkSpawnPoint = fireworkSpawnPoint;
            _onFireworkComplete = onFireworkComplete;
            _onSlideshowExitRequested = onSlideshowExitRequested;
            _onFireworkRequested = onFireworkRequested;
            _onSlideshowStarted = onSlideshowStarted;
            _onSlideshowArtworkChanged = onSlideshowArtworkChanged;
            _onSlideshowComplete = onSlideshowComplete;
            _onSlideshowArtworkStarted = onSlideshowArtworkStarted;
            _slideshowCurrentIndex = slideshowCurrentIndex;
            _slideshowTotalCount = slideshowTotalCount;
            _isRocketEnabled = isRocketEnabled;
            _onRocketLaunchRequested = onRocketLaunchRequested;
        }

        // ---- Private Methods ----

        private void PlayCurrentArtwork()
        {
            if (_playlist == null || _currentIndex >= _playlist.Count)
            {
                return;
            }

            ArtworkData artwork = _playlist[_currentIndex];
            _currentState = SlideshowState.Playing;

            UpdateSharedVariables();

            if (_onSlideshowArtworkChanged != null)
            {
                _onSlideshowArtworkChanged.Raise();
            }

            if (_onSlideshowArtworkStarted != null)
            {
                _onSlideshowArtworkStarted.Raise(artwork);
            }

            // Construct and raise firework request
            Vector3 spawnPosition = _fireworkSpawnPoint != null
                ? _fireworkSpawnPoint.position
                : Vector3.up * 10f;

            FireworkRequest request = new FireworkRequest
            {
                Position = spawnPosition,
                Pattern = artwork.Pixels,
                PatternWidth = artwork.Width,
                PatternHeight = artwork.Height
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
        }

        private void AdvanceToNext()
        {
            _currentIndex++;

            if (_currentIndex >= _playlist.Count)
            {
                bool shouldLoop = _slideshowConfig != null && _slideshowConfig.IsLooping;

                if (shouldLoop)
                {
                    _currentIndex = 0;

                    if (_slideshowConfig.IsShuffled)
                    {
                        ShufflePlaylist();
                    }

                    PlayCurrentArtwork();
                }
                else
                {
                    _currentState = SlideshowState.Complete;
                    _isActive = false;

                    if (_onSlideshowComplete != null)
                    {
                        _onSlideshowComplete.Raise();
                    }
                }

                return;
            }

            float transitionDuration = _slideshowConfig != null
                ? _slideshowConfig.TransitionDuration
                : 2f;

            _transitionTimer = transitionDuration;
            _currentState = SlideshowState.Transitioning;
        }

        private void UpdateTransition()
        {
            _transitionTimer -= Time.deltaTime;

            if (_transitionTimer <= 0f)
            {
                PlayCurrentArtwork();
            }
        }

        private void HandleFireworkComplete()
        {
            if (!_isActive || _currentState != SlideshowState.Playing)
            {
                return;
            }

            AdvanceToNext();
        }

        private void HandleSlideshowExitRequested()
        {
            StopSlideshow();
        }

        private void UpdateSharedVariables()
        {
            if (_slideshowCurrentIndex != null)
            {
                _slideshowCurrentIndex.Value = _currentIndex;
            }

            if (_slideshowTotalCount != null && _playlist != null)
            {
                _slideshowTotalCount.Value = _playlist.Count;
            }
        }

        private void ShufflePlaylist()
        {
            if (_playlist == null || _playlist.Count <= 1)
            {
                return;
            }

            for (int i = _playlist.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                ArtworkData temp = _playlist[i];
                _playlist[i] = _playlist[j];
                _playlist[j] = temp;
            }
        }
    }
}
