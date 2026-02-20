// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.GameFlow;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Runtime.UI
{
    /// <summary>
    /// Settings overlay panel. Provides export/import functionality, volume control,
    /// and credits. Visible only when <see cref="AppState.Settings"/> is active.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("State")]
        [Tooltip("Shared app state — shows/hides based on this")]
        [SerializeField] private AppStateVariableSO _appState;

        [Header("Data")]
        [Tooltip("Data manager for export/import operations")]
        [SerializeField] private DataManager _dataManager;

        [Header("Shared Variables")]
        [Tooltip("Master volume control")]
        [SerializeField] private FloatVariableSO _masterVolume;

        [Header("Events")]
        [Tooltip("Raised when data changes")]
        [SerializeField] private GameEventSO _onDataChanged;

        [Header("UI Elements")]
        [Tooltip("Button to export artwork data to clipboard")]
        [SerializeField] private Button _exportButton;

        [Tooltip("Button to import artwork data from text field")]
        [SerializeField] private Button _importButton;

        [Tooltip("Button to close settings and return")]
        [SerializeField] private Button _backButton;

        [Tooltip("Volume control slider")]
        [SerializeField] private Slider _volumeSlider;

        [Tooltip("Input field for pasting JSON data")]
        [SerializeField] private TMP_InputField _importJsonField;

        [Tooltip("Status text showing export/import result")]
        [SerializeField] private TextMeshProUGUI _statusText;

        // ---- Private Fields ----
        private AppState _previousState;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged += HandleAppStateChanged;
            }

            if (_exportButton != null)
            {
                _exportButton.onClick.AddListener(OnExportClicked);
            }

            if (_importButton != null)
            {
                _importButton.onClick.AddListener(OnImportClicked);
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

            if (_exportButton != null)
            {
                _exportButton.onClick.RemoveListener(OnExportClicked);
            }

            if (_importButton != null)
            {
                _importButton.onClick.RemoveListener(OnImportClicked);
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

        private void OnExportClicked()
        {
            if (_dataManager == null)
            {
                SetStatus("No data manager assigned.");
                return;
            }

            string json = JsonPersistence.ExportAllArtworks(_dataManager.GetAllArtworks());
            GUIUtility.systemCopyBuffer = json;
            SetStatus("Exported to clipboard! (" + _dataManager.ArtworkCount + " artworks)");
        }

        private void OnImportClicked()
        {
            if (_dataManager == null)
            {
                SetStatus("No data manager assigned.");
                return;
            }

            if (_importJsonField == null || string.IsNullOrEmpty(_importJsonField.text))
            {
                SetStatus("Paste JSON data into the text field first.");
                return;
            }

            List<ArtworkData> imported = JsonPersistence.ImportArtworks(_importJsonField.text);

            if (imported == null || imported.Count == 0)
            {
                SetStatus("Import failed — no valid artwork data found.");
                return;
            }

            _dataManager.SetAllArtworks(imported);
            SetStatus("Imported " + imported.Count + " artworks.");

            if (_importJsonField != null)
            {
                _importJsonField.text = "";
            }
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

        private void SetStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }
    }
}
