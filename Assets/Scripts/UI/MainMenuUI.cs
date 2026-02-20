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
    /// Main menu panel. Displays mode selection buttons and saved artwork count.
    /// Visible only when <see cref="AppState.Menu"/> is active.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("State")]
        [Tooltip("Shared app state — shows/hides based on this")]
        [SerializeField] private AppStateVariableSO _appState;

        [Header("Shared Variables")]
        [Tooltip("True when Challenge Mode selected, false for Free Mode")]
        [SerializeField] private BoolVariableSO _isChallengeMode;

        [Tooltip("Read-only artwork count for display")]
        [SerializeField] private IntVariableSO _artworkCount;

        [Header("Events")]
        [Tooltip("Raised when data changes (artwork count refresh)")]
        [SerializeField] private GameEventSO _onDataChanged;

        [Header("UI Elements")]
        [Tooltip("Button to start Free Mode")]
        [SerializeField] private Button _freeModeButton;

        [Tooltip("Button to start Challenge Mode")]
        [SerializeField] private Button _challengeModeButton;

        [Tooltip("Button to start Slideshow")]
        [SerializeField] private Button _slideshowButton;

        [Tooltip("Button to open Settings")]
        [SerializeField] private Button _settingsButton;

        [Tooltip("Text displaying saved artwork count")]
        [SerializeField] private TextMeshProUGUI _artworkCountText;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged += HandleAppStateChanged;
            }

            if (_onDataChanged != null)
            {
                _onDataChanged.Register(RefreshArtworkCount);
            }

            if (_freeModeButton != null)
            {
                _freeModeButton.onClick.AddListener(OnFreeModeClicked);
            }

            if (_challengeModeButton != null)
            {
                _challengeModeButton.onClick.AddListener(OnChallengeModeClicked);
            }

            if (_slideshowButton != null)
            {
                _slideshowButton.onClick.AddListener(OnSlideshowClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            RefreshVisibility();
            RefreshArtworkCount();
        }

        private void OnDisable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged -= HandleAppStateChanged;
            }

            if (_onDataChanged != null)
            {
                _onDataChanged.Unregister(RefreshArtworkCount);
            }

            if (_freeModeButton != null)
            {
                _freeModeButton.onClick.RemoveListener(OnFreeModeClicked);
            }

            if (_challengeModeButton != null)
            {
                _challengeModeButton.onClick.RemoveListener(OnChallengeModeClicked);
            }

            if (_slideshowButton != null)
            {
                _slideshowButton.onClick.RemoveListener(OnSlideshowClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            }
        }

        // ---- Private Methods ----

        private void HandleAppStateChanged(AppState state)
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            bool isVisible = _appState != null && _appState.Value == AppState.Menu;
            gameObject.SetActive(isVisible);
        }

        private void RefreshArtworkCount()
        {
            if (_artworkCountText != null && _artworkCount != null)
            {
                _artworkCountText.text = _artworkCount.Value + " Artworks Saved";
            }
        }

        private void OnFreeModeClicked()
        {
            if (_isChallengeMode != null)
            {
                _isChallengeMode.Value = false;
            }

            if (_appState != null)
            {
                _appState.Value = AppState.Playing;
            }
        }

        private void OnChallengeModeClicked()
        {
            if (_isChallengeMode != null)
            {
                _isChallengeMode.Value = true;
            }

            if (_appState != null)
            {
                _appState.Value = AppState.Playing;
            }
        }

        private void OnSlideshowClicked()
        {
            if (_appState != null)
            {
                _appState.Value = AppState.Slideshow;
            }
        }

        private void OnSettingsClicked()
        {
            if (_appState != null)
            {
                _appState.Value = AppState.Settings;
            }
        }
    }
}
