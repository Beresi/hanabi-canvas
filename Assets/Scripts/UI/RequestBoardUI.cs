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
    /// Panel showing available challenge requests. Each request card displays
    /// prompt text, constraint info, and a completion checkmark. The user selects
    /// a request to begin Challenge Mode.
    /// </summary>
    public class RequestBoardUI : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("State")]
        [Tooltip("Shared app state — shows/hides based on this")]
        [SerializeField] private AppStateVariableSO _appState;

        [Header("Data")]
        [Tooltip("Data manager for reading request list (read-only exception)")]
        [SerializeField] private DataManager _dataManager;

        [Header("Shared Variables")]
        [Tooltip("Written when user selects a request")]
        [SerializeField] private IntVariableSO _selectedRequestIndex;

        [Tooltip("True when in Challenge Mode")]
        [SerializeField] private BoolVariableSO _isChallengeMode;

        [Header("Events")]
        [Tooltip("Raised when data changes (request list refresh)")]
        [SerializeField] private GameEventSO _onDataChanged;

        [Header("UI Elements")]
        [Tooltip("Container for request card instances")]
        [SerializeField] private Transform _cardContainer;

        [Tooltip("Prefab for request cards")]
        [SerializeField] private GameObject _requestCardPrefab;

        [Tooltip("Back button to return to menu")]
        [SerializeField] private Button _backButton;

        // ---- Private Fields ----
        private readonly List<GameObject> _spawnedCards = new List<GameObject>();

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged += HandleAppStateChanged;
            }

            if (_onDataChanged != null)
            {
                _onDataChanged.Register(RefreshRequestList);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            RefreshVisibility();
            RefreshRequestList();
        }

        private void OnDisable()
        {
            if (_appState != null)
            {
                _appState.OnValueChanged -= HandleAppStateChanged;
            }

            if (_onDataChanged != null)
            {
                _onDataChanged.Unregister(RefreshRequestList);
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        // ---- Private Methods ----

        private void HandleAppStateChanged(AppState state)
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            bool isVisible = _appState != null
                && _appState.Value == AppState.Playing
                && _isChallengeMode != null
                && _isChallengeMode.Value;

            gameObject.SetActive(isVisible);

            if (isVisible)
            {
                RefreshRequestList();
            }
        }

        private void RefreshRequestList()
        {
            ClearCards();

            if (_dataManager == null || _cardContainer == null || _requestCardPrefab == null)
            {
                return;
            }

            IReadOnlyList<RequestData> requests = _dataManager.GetActiveRequests();

            for (int i = 0; i < requests.Count; i++)
            {
                GameObject card = Instantiate(_requestCardPrefab, _cardContainer);
                _spawnedCards.Add(card);

                RequestData request = requests[i];
                int requestIndex = i;

                // Set prompt text
                TextMeshProUGUI promptText = card.GetComponentInChildren<TextMeshProUGUI>();
                if (promptText != null)
                {
                    promptText.text = request.Prompt;
                }

                // Wire select button
                Button selectButton = card.GetComponentInChildren<Button>();
                if (selectButton != null)
                {
                    selectButton.onClick.AddListener(() => OnRequestSelected(requestIndex));
                }
            }
        }

        private void ClearCards()
        {
            for (int i = 0; i < _spawnedCards.Count; i++)
            {
                if (_spawnedCards[i] != null)
                {
                    Destroy(_spawnedCards[i]);
                }
            }

            _spawnedCards.Clear();
        }

        private void OnRequestSelected(int index)
        {
            if (_selectedRequestIndex != null)
            {
                _selectedRequestIndex.Value = index;
            }
        }

        private void OnBackClicked()
        {
            if (_appState != null)
            {
                _appState.Value = AppState.Menu;
            }
        }
    }
}
