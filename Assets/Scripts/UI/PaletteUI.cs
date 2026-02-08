// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

namespace HanabiCanvas.Runtime.UI
{
    public class PaletteUI : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [Header("References")]
        [Tooltip("The color palette to display")]
        [SerializeField] private ColorPaletteSO _palette;

        [Tooltip("Shared active color index — written here, read by PixelCanvas")]
        [SerializeField] private IntVariableSO _activeColorIndex;

        [Header("UI")]
        [Tooltip("Parent transform for color buttons (should have a HorizontalLayoutGroup)")]
        [SerializeField] private RectTransform _buttonContainer;

        [Tooltip("Prefab for each color button")]
        [SerializeField] private Button _colorButtonPrefab;

        [Header("Selection")]
        [Tooltip("Color tint applied to the selected button")]
        [SerializeField] private Color _selectedTint = Color.white;

        [Tooltip("Color tint applied to unselected buttons")]
        [SerializeField] private Color _unselectedTint = new Color(0.6f, 0.6f, 0.6f, 1f);

        // ---- Private Fields ----
        private Button[] _colorButtons;
        private int _selectedIndex;

        // ---- Unity Methods ----
        private void Awake()
        {
            if (_palette == null || _activeColorIndex == null)
            {
                Debug.LogWarning($"[{nameof(PaletteUI)}] Palette or ActiveColorIndex SO is not assigned.", this);
                enabled = false;
                return;
            }

            CreateColorButtons();
            SelectColor(0);
        }

        // ---- Public Methods ----
        public void Initialize(ColorPaletteSO palette, IntVariableSO activeColorIndex)
        {
            _palette = palette;
            _activeColorIndex = activeColorIndex;
            CreateColorButtons();
            SelectColor(0);
        }

        public void SelectColor(int index)
        {
            if (_colorButtons == null || index < 0 || index >= _colorButtons.Length)
            {
                return;
            }

            _selectedIndex = index;

            if (_activeColorIndex != null)
            {
                _activeColorIndex.Value = index;
            }

            UpdateSelectionVisuals();
        }

        // ---- Private Methods ----
        private void CreateColorButtons()
        {
            if (_buttonContainer == null || _colorButtonPrefab == null)
            {
                return;
            }

            ClearExistingButtons();

            Color32[] colors = _palette.Colors;
            _colorButtons = new Button[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                Button button = Instantiate(_colorButtonPrefab, _buttonContainer);
                button.gameObject.name = $"ColorButton_{i}";

                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = colors[i];
                }

                int colorIndex = i;
                button.onClick.AddListener(() => SelectColor(colorIndex));

                _colorButtons[i] = button;
            }
        }

        private void ClearExistingButtons()
        {
            if (_colorButtons != null)
            {
                for (int i = 0; i < _colorButtons.Length; i++)
                {
                    if (_colorButtons[i] != null)
                    {
                        Destroy(_colorButtons[i].gameObject);
                    }
                }
            }

            _colorButtons = null;
        }

        private void UpdateSelectionVisuals()
        {
            for (int i = 0; i < _colorButtons.Length; i++)
            {
                RectTransform rectTransform = _colorButtons[i].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float scale = (i == _selectedIndex) ? 1.2f : 1.0f;
                    rectTransform.localScale = new Vector3(scale, scale, 1f);
                }

                Image image = _colorButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    Color32 paletteColor = _palette.Colors[i];
                    Color tint = (i == _selectedIndex) ? _selectedTint : _unselectedTint;
                    image.color = new Color(
                        paletteColor.r / 255f * tint.r,
                        paletteColor.g / 255f * tint.g,
                        paletteColor.b / 255f * tint.b,
                        1f);
                }
            }
        }
    }
}
