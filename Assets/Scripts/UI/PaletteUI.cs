// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using System.Collections.Generic;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Manages a palette of <see cref="ColorButton"/> instances in a grid layout.
    /// Acts as a radio group — only one button is selected at a time.
    /// Writes the selected color to a <see cref="ColorVariableSO"/>.
    /// </summary>
    public class PaletteUI : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("Data")]
        [Tooltip("Color palette defining available colors")]
        [SerializeField] private ColorPaletteSO _palette;

        [Tooltip("Shared variable for the currently selected color")]
        [SerializeField] private ColorVariableSO _currentColor;

        [Header("UI")]
        [Tooltip("Container with GridLayoutGroup for color buttons")]
        [SerializeField] private RectTransform _colorButtonContainer;

        [Tooltip("Prefab with a ColorButton component")]
        [SerializeField] private ColorButton _colorButtonPrefab;

        // ---- Private Fields ----
        private readonly List<ColorButton> _colorButtons = new List<ColorButton>();
        private int _selectedIndex = -1;

        // ---- Unity Methods ----

        private void Start()
        {
            CreateButtons();
            SelectButton(0);
        }

        private void OnEnable()
        {
            if (_currentColor != null)
            {
                _currentColor.OnValueChanged += HandleColorChanged;
            }
        }

        private void OnDisable()
        {
            if (_currentColor != null)
            {
                _currentColor.OnValueChanged -= HandleColorChanged;
            }
        }

        // ---- Private Methods ----

        [ContextMenu("Create Buttons")]
        private void CreateButtons()
        {
            for (int i = 0; i < _palette.Count; i++)
            {
                var colorButton = Instantiate(_colorButtonPrefab, _colorButtonContainer);

                colorButton.Initialize(_palette[i], HandleButtonClicked);
                colorButton.SetState(ColorButtonState.Default);
                _colorButtons.Add(colorButton);
            }
        }

        private void HandleButtonClicked(ColorButton clickedButton)
        {
            int index = _colorButtons.IndexOf(clickedButton);

            SelectButton(index);
        }

        private void SelectButton(int index)
        {
            if (index < 0 || index >= _colorButtons.Count) return;

            // Deselect previous
            if (_selectedIndex >= 0 && _selectedIndex < _colorButtons.Count)
            {
                _colorButtons[_selectedIndex].SetState(ColorButtonState.Default);
            }

            // Select new
            _selectedIndex = index;
            _colorButtons[_selectedIndex].SetState(ColorButtonState.Selected);

            // Write shared color variable
            if (_currentColor != null && _palette != null)
            {
                _currentColor.Value = _palette[index];
            }
        }

        private void HandleColorChanged(Color newColor)
        {
            for (int i = 0; i < _palette.Count; i++)
            {
                if (_palette[i] == newColor)
                {
                    if (i != _selectedIndex)
                    {
                        // Deselect previous
                        if (_selectedIndex >= 0 && _selectedIndex < _colorButtons.Count)
                        {
                            _colorButtons[_selectedIndex].SetState(ColorButtonState.Default);
                        }

                        _selectedIndex = i;

                        // Select new
                        if (_selectedIndex < _colorButtons.Count)
                        {
                            _colorButtons[_selectedIndex].SetState(ColorButtonState.Selected);
                        }
                    }
                    return;
                }
            }
        }
    }
}
