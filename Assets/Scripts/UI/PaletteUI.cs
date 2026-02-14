// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HanabiCanvas.Runtime
{
    public class PaletteUI : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [SerializeField] private ColorPaletteSO _palette;
        [SerializeField] private ColorVariableSO _currentColor;
        [SerializeField] private RectTransform _colorButtonContainer;
        [SerializeField] private GameObject _colorButtonPrefab;

        // ---- Private Fields ----
        private readonly List<RectTransform> _buttonTransforms = new List<RectTransform>();
        private int _selectedIndex;

        // ---- Unity Methods ----
        private void Start()
        {
            for (int i = 0; i < _palette.Count; i++)
            {
                GameObject buttonObject = Instantiate(_colorButtonPrefab, _colorButtonContainer);
                Image image = buttonObject.GetComponent<Image>();
                image.color = _palette[i];

                Button button = buttonObject.GetComponent<Button>();
                int capturedIndex = i;
                button.onClick.AddListener(() => SelectColor(capturedIndex));

                _buttonTransforms.Add(buttonObject.GetComponent<RectTransform>());
            }

            SelectColor(0);
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
        private void SelectColor(int index)
        {
            _selectedIndex = index;
            _currentColor.Value = _palette[index];
            UpdateHighlight();
        }

        private void HandleColorChanged(Color newColor)
        {
            for (int i = 0; i < _palette.Count; i++)
            {
                if (_palette[i] == newColor)
                {
                    _selectedIndex = i;
                    UpdateHighlight();
                    return;
                }
            }
        }

        private void UpdateHighlight()
        {
            for (int i = 0; i < _buttonTransforms.Count; i++)
            {
                _buttonTransforms[i].localScale = i == _selectedIndex
                    ? Vector3.one * 1.2f
                    : Vector3.one;
            }
        }
    }
}
