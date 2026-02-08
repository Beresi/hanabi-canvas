// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using HanabiCanvas.Runtime.Canvas;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime.UI
{
    public class CanvasToolbar : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("Shared Variables")]
        [Tooltip("Shared active tool index — written here, read by PixelCanvas")]
        [SerializeField] private IntVariableSO _activeToolIndex;

        [Header("Events")]
        [Tooltip("Raised when the canvas should be cleared")]
        [SerializeField] private GameEventSO _onCanvasCleared;

        [Tooltip("Raised when a firework should be launched")]
        [SerializeField] private GameEventSO _onLaunchFirework;

        [Header("Tool Buttons")]
        [Tooltip("Button that activates the Draw tool")]
        [SerializeField] private Button _drawButton;

        [Tooltip("Button that activates the Erase tool")]
        [SerializeField] private Button _eraseButton;

        [Tooltip("Button that activates the Fill tool")]
        [SerializeField] private Button _fillButton;

        [Header("Action Buttons")]
        [Tooltip("Button that clears the canvas")]
        [SerializeField] private Button _clearButton;

        [Tooltip("Button that launches the firework")]
        [SerializeField] private Button _launchButton;

        [Header("Selection Visuals")]
        [Tooltip("Color applied to the active tool button")]
        [SerializeField] private Color _activeToolColor = new Color(1f, 1f, 1f, 1f);

        [Tooltip("Color applied to inactive tool buttons")]
        [SerializeField] private Color _inactiveToolColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        // ---- Private Fields ----
        private CanvasTool _currentTool;

        // ---- Unity Methods ----
        private void Awake()
        {
            BindButtons();
            SelectTool(CanvasTool.Draw);
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        // ---- Public Methods ----
        public void SelectTool(CanvasTool tool)
        {
            _currentTool = tool;

            if (_activeToolIndex != null)
            {
                _activeToolIndex.Value = (int)tool;
            }

            UpdateToolVisuals();
        }

        // ---- Private Methods ----
        private void BindButtons()
        {
            if (_drawButton != null)
            {
                _drawButton.onClick.AddListener(OnDrawPressed);
            }

            if (_eraseButton != null)
            {
                _eraseButton.onClick.AddListener(OnErasePressed);
            }

            if (_fillButton != null)
            {
                _fillButton.onClick.AddListener(OnFillPressed);
            }

            if (_clearButton != null)
            {
                _clearButton.onClick.AddListener(OnClearPressed);
            }

            if (_launchButton != null)
            {
                _launchButton.onClick.AddListener(OnLaunchPressed);
            }
        }

        private void UnbindButtons()
        {
            if (_drawButton != null)
            {
                _drawButton.onClick.RemoveListener(OnDrawPressed);
            }

            if (_eraseButton != null)
            {
                _eraseButton.onClick.RemoveListener(OnErasePressed);
            }

            if (_fillButton != null)
            {
                _fillButton.onClick.RemoveListener(OnFillPressed);
            }

            if (_clearButton != null)
            {
                _clearButton.onClick.RemoveListener(OnClearPressed);
            }

            if (_launchButton != null)
            {
                _launchButton.onClick.RemoveListener(OnLaunchPressed);
            }
        }

        private void OnDrawPressed()
        {
            SelectTool(CanvasTool.Draw);
        }

        private void OnErasePressed()
        {
            SelectTool(CanvasTool.Erase);
        }

        private void OnFillPressed()
        {
            SelectTool(CanvasTool.Fill);
        }

        private void OnClearPressed()
        {
            if (_onCanvasCleared != null)
            {
                _onCanvasCleared.Raise();
            }
        }

        private void OnLaunchPressed()
        {
            if (_onLaunchFirework != null)
            {
                _onLaunchFirework.Raise();
            }
        }

        private void UpdateToolVisuals()
        {
            SetButtonColor(_drawButton, _currentTool == CanvasTool.Draw);
            SetButtonColor(_eraseButton, _currentTool == CanvasTool.Erase);
            SetButtonColor(_fillButton, _currentTool == CanvasTool.Fill);
        }

        private void SetButtonColor(Button button, bool isActive)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = isActive ? _activeToolColor : _inactiveToolColor;
            }
        }
    }
}
