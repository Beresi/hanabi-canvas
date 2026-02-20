// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Individual color button in the palette. Manages three visual states
    /// (disabled, default, selected) via frame sprite swapping and scale.
    /// </summary>
    public class ColorButton : MonoBehaviour
    {
        // ---- Constants ----
        private const float SELECTED_SCALE = 1.1f;

        // ---- Serialized Fields ----

        [Header("References")]
        [Tooltip("Image component displaying the frame border")]
        [SerializeField] private Image _frameImage;

        [Tooltip("Image component displaying the color fill")]
        [SerializeField] private Image _colorFillImage;

        [Tooltip("Button component for click detection")]
        [SerializeField] private Button _button;

        [Header("Frame Sprites")]
        [Tooltip("Sprite shown when button is disabled")]
        [SerializeField] private Sprite _disabledSprite;

        [Tooltip("Sprite shown in normal/unselected state")]
        [SerializeField] private Sprite _defaultSprite;

        [Tooltip("Sprite shown when button is selected")]
        [SerializeField] private Sprite _selectedSprite;

        // ---- Private Fields ----
        private Action<ColorButton> _onClicked;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Initializes the color button with its display color and click callback.
        /// </summary>
        /// <param name="color">The color to display in the fill area.</param>
        /// <param name="onClicked">Callback invoked when this button is clicked.</param>
        public void Initialize(Color color, Action<ColorButton> onClicked)
        {
            if (_colorFillImage != null)
            {
                _colorFillImage.color = color;
            }

            _onClicked = onClicked;
        }

        /// <summary>
        /// Sets the visual state of this color button by swapping the frame sprite
        /// and adjusting scale. Selected buttons scale to 1.1x, others revert to 1.0x.
        /// </summary>
        /// <param name="state">The target visual state.</param>
        public void SetState(ColorButtonState state)
        {
            if (_frameImage != null)
            {
                switch (state)
                {
                    case ColorButtonState.Disabled:
                        _frameImage.sprite = _disabledSprite;
                        break;
                    case ColorButtonState.Selected:
                        _frameImage.sprite = _selectedSprite;
                        break;
                    case ColorButtonState.Default:
                    default:
                        _frameImage.sprite = _defaultSprite;
                        break;
                }
            }

            transform.localScale = state == ColorButtonState.Selected
                ? Vector3.one * SELECTED_SCALE
                : Vector3.one;
        }

        // ---- Private Methods ----

        private void HandleClick()
        {
            _onClicked?.Invoke(this);
        }
    }

    /// <summary>
    /// Visual states for a <see cref="ColorButton"/>.
    /// </summary>
    public enum ColorButtonState
    {
        Disabled,
        Default,
        Selected
    }
}
