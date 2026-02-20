// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.GameFlow;

namespace HanabiCanvas.Runtime.UI
{
    /// <summary>
    /// Minimal HUD overlay during slideshow playback. Shows progress,
    /// artwork name, like button, skip, and exit controls.
    /// Visible only when <see cref="AppState.Slideshow"/> is active.
    /// </summary>
    public class SlideshowUI : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("State")]
        [Tooltip("Shared app state — shows/hides based on this")]
        [SerializeField] private AppStateVariableSO _appState;

        [Header("Shared Variables")]
        [Tooltip("Current artwork index in slideshow")]
        [SerializeField] private IntVariableSO _slideshowCurrentIndex;

        [Tooltip("Total artwork count in slideshow")]
        [SerializeField] private IntVariableSO _slideshowTotalCount;

        [Header("Events — Listened")]
        [Tooltip("Raised when slideshow advances to next artwork")]
        [SerializeField] private GameEventSO _onSlideshowArtworkChanged;

        [Tooltip("Typed event with the current artwork data")]
        [SerializeField] private ArtworkEventSO _onSlideshowArtworkStarted;

        [Tooltip("Raised when slideshow completes")]
        [SerializeField] private GameEventSO _onSlideshowComplete;

        [Header("Events — Raised")]
        [Tooltip("Raised when user wants to exit slideshow")]
        [SerializeField] private GameEventSO _onSlideshowExitRequested;

        [Tooltip("Raised when user toggles like on an artwork")]
        [SerializeField] private GameEventSO _onArtworkLiked;

        [Header("UI Elements")]
        [Tooltip("Progress text (e.g. 'Artwork 3 of 12')")]
        [SerializeField] private TextMeshProUGUI _progressText;

        [Tooltip("Current artwork name")]
        [SerializeField] private TextMeshProUGUI _artworkNameText;

        [Tooltip("Skip to next artwork button")]
        [SerializeField] private Button _skipButton;

        [Tooltip("Exit slideshow button")]
        [SerializeField] private Button _exitButton;

        [Tooltip("Like/heart toggle button")]
        [SerializeField] private Button _likeButton;

        [Tooltip("Like icon image (for toggling appearance)")]
        [SerializeField] private Image _likeIcon;

        [Tooltip("Color when liked")]
        [SerializeField] private Color _likedColor = Color.red;

        [Tooltip("Color when not liked")]
        [SerializeField] private Color _unlikedColor = Color.white;

        // ---- Private Fields ----
        private ArtworkData _currentArtwork;
        private bool _hasCurrentArtwork;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged += HandleAppStateChanged;
            }

            if (_onSlideshowArtworkChanged != null)
            {
                _onSlideshowArtworkChanged.Register(HandleArtworkChanged);
            }

            if (_onSlideshowArtworkStarted != null)
            {
                _onSlideshowArtworkStarted.Register(HandleArtworkStarted);
            }

            if (_onSlideshowComplete != null)
            {
                _onSlideshowComplete.Register(HandleSlideshowComplete);
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(OnSkipClicked);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(OnExitClicked);
            }

            if (_likeButton != null)
            {
                _likeButton.onClick.AddListener(OnLikeClicked);
            }

            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged -= HandleAppStateChanged;
            }

            if (_onSlideshowArtworkChanged != null)
            {
                _onSlideshowArtworkChanged.Unregister(HandleArtworkChanged);
            }

            if (_onSlideshowArtworkStarted != null)
            {
                _onSlideshowArtworkStarted.Unregister(HandleArtworkStarted);
            }

            if (_onSlideshowComplete != null)
            {
                _onSlideshowComplete.Unregister(HandleSlideshowComplete);
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.RemoveListener(OnSkipClicked);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(OnExitClicked);
            }

            if (_likeButton != null)
            {
                _likeButton.onClick.RemoveListener(OnLikeClicked);
            }
        }

        // ---- Private Methods ----

        private void HandleAppStateChanged(AppState state)
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            bool isVisible = _appState != null && _appState.Value == AppState.Slideshow;
            gameObject.SetActive(isVisible);
        }

        private void HandleArtworkChanged()
        {
            RefreshProgress();
        }

        private void HandleArtworkStarted(ArtworkData artwork)
        {
            _currentArtwork = artwork;
            _hasCurrentArtwork = true;

            if (_artworkNameText != null)
            {
                _artworkNameText.text = artwork.Name ?? "";
            }

            RefreshLikeIcon();
            RefreshProgress();
        }

        private void HandleSlideshowComplete()
        {
            _hasCurrentArtwork = false;
        }

        private void RefreshProgress()
        {
            if (_progressText == null)
            {
                return;
            }

            int current = _slideshowCurrentIndex != null ? _slideshowCurrentIndex.Value + 1 : 0;
            int total = _slideshowTotalCount != null ? _slideshowTotalCount.Value : 0;
            _progressText.text = "Artwork " + current + " of " + total;
        }

        private void RefreshLikeIcon()
        {
            if (_likeIcon == null || !_hasCurrentArtwork)
            {
                return;
            }

            _likeIcon.color = _currentArtwork.IsLiked ? _likedColor : _unlikedColor;
        }

        private void OnSkipClicked()
        {
            // Skip handled via the same exit requested event — SlideshowController will advance
            if (_onSlideshowArtworkChanged != null)
            {
                // The skip action is implicit — the controller listens for firework complete.
                // For an explicit skip, we'd need a dedicated event. For now, the exit button
                // is the primary control. Skip can be wired later.
            }
        }

        private void OnExitClicked()
        {
            if (_onSlideshowExitRequested != null)
            {
                _onSlideshowExitRequested.Raise();
            }
        }

        private void OnLikeClicked()
        {
            if (_onArtworkLiked != null)
            {
                _onArtworkLiked.Raise();
            }
        }
    }
}
