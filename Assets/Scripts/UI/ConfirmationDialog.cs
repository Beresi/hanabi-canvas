// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HanabiCanvas.Runtime.UI
{
    /// <summary>
    /// Reusable modal confirmation dialog. Shows a title, message, and confirm/cancel buttons.
    /// Uses a static instance reference for convenient access from any UI component.
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("UI Elements")]
        [Tooltip("The dialog panel to show/hide")]
        [SerializeField] private GameObject _panel;

        [Tooltip("Title text of the dialog")]
        [SerializeField] private TextMeshProUGUI _titleText;

        [Tooltip("Message text of the dialog")]
        [SerializeField] private TextMeshProUGUI _messageText;

        [Tooltip("Confirm action button")]
        [SerializeField] private Button _confirmButton;

        [Tooltip("Cancel action button")]
        [SerializeField] private Button _cancelButton;

        // ---- Private Fields ----
        private System.Action _onConfirmCallback;
        private System.Action _onCancelCallback;

        // ---- Static Access ----
        private static ConfirmationDialog _instance;

        // ---- Properties ----

        /// <summary>Whether the dialog is currently visible.</summary>
        public bool IsVisible => _panel != null && _panel.activeSelf;

        // ---- Unity Methods ----

        private void Awake()
        {
            _instance = this;

            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        private void OnDisable()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelClicked);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Shows the dialog with the specified title, message, and callbacks.
        /// </summary>
        /// <param name="title">Dialog title.</param>
        /// <param name="message">Dialog message body.</param>
        /// <param name="onConfirm">Callback invoked when confirm is clicked.</param>
        /// <param name="onCancel">Optional callback invoked when cancel is clicked.</param>
        public void Show(string title, string message, System.Action onConfirm, System.Action onCancel = null)
        {
            _onConfirmCallback = onConfirm;
            _onCancelCallback = onCancel;

            if (_titleText != null)
            {
                _titleText.text = title;
            }

            if (_messageText != null)
            {
                _messageText.text = message;
            }

            if (_panel != null)
            {
                _panel.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the dialog and clears callbacks.
        /// </summary>
        public void Hide()
        {
            _onConfirmCallback = null;
            _onCancelCallback = null;

            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        /// <summary>
        /// Static convenience method to show the dialog via the singleton instance.
        /// </summary>
        public static void ShowDialog(string title, string message, System.Action onConfirm, System.Action onCancel = null)
        {
            if (_instance != null)
            {
                _instance.Show(title, message, onConfirm, onCancel);
            }
            else
            {
                Debug.LogWarning("[ConfirmationDialog] No instance found in scene.");
            }
        }

        // ---- Private Methods ----

        private void OnConfirmClicked()
        {
            System.Action callback = _onConfirmCallback;
            Hide();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            System.Action callback = _onCancelCallback;
            Hide();
            callback?.Invoke();
        }
    }
}
