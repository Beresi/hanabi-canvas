// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using HanabiCanvas.Runtime.GameFlow;

namespace HanabiCanvas.Runtime.UI
{
    /// <summary>
    /// Settings overlay panel. Provides volume control and a back button.
    /// Visible only when <see cref="AppState.Settings"/> is active.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("State")]
        [Tooltip("Shared app state — shows/hides based on this")]
        [SerializeField] private AppStateVariableSO _appState;

        [Header("Shared Variables")]
        [Tooltip("Master volume control")]
        [SerializeField] private FloatVariableSO _masterVolume;

        [Header("UI Elements")]
        [Tooltip("Button to close settings and return")]
        [SerializeField] private Button _backButton;

        [Tooltip("Volume control slider")]
        [SerializeField] private Slider _volumeSlider;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged += HandleAppStateChanged;
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            if (_volumeSlider != null)
            {
                if (_masterVolume != null)
                {
                    _volumeSlider.value = _masterVolume.Value;
                }

                _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged -= HandleAppStateChanged;
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackClicked);
            }

            if (_volumeSlider != null)
            {
                _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            }
        }

        // ---- Private Methods ----

        private void HandleAppStateChanged(AppState state)
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            bool isVisible = _appState != null && _appState.Value == AppState.Settings;
            gameObject.SetActive(isVisible);
        }

        private void OnBackClicked()
        {
            if (_appState != null)
            {
                _appState.Value = AppState.Menu;
            }
        }

        private void OnVolumeChanged(float value)
        {
            if (_masterVolume != null)
            {
                _masterVolume.Value = value;
            }
        }
    }
}
