// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime
{
    public class PatternGalleryUI : MonoBehaviour
    {
        // ---- Serialized Fields ----
        [SerializeField] private PatternListSO _patternLibrary;
        [SerializeField] private IntVariableSO _selectedPatternIndex;
        [SerializeField] private GameEventSO _onLaunchPattern;
        [SerializeField] private RectTransform _thumbnailContainer;
        [SerializeField] private GameObject _thumbnailPrefab;

        // ---- Private Fields ----
        private readonly List<GameObject> _thumbnails = new List<GameObject>();

        // ---- Unity Methods ----
        private void OnEnable()
        {
            if (_patternLibrary != null)
            {
                _patternLibrary.OnItemAdded += HandlePatternAdded;
            }
        }

        private void OnDisable()
        {
            if (_patternLibrary != null)
            {
                _patternLibrary.OnItemAdded -= HandlePatternAdded;
            }
        }

        private void Start()
        {
            for (int i = 0; i < _patternLibrary.Count; i++)
            {
                CreateThumbnail(_patternLibrary.GetAt(i), i);
            }
        }

        // ---- Private Methods ----
        private void HandlePatternAdded(FireworkPattern pattern)
        {
            CreateThumbnail(pattern, _patternLibrary.Count - 1);
        }

        private void CreateThumbnail(FireworkPattern pattern, int index)
        {
            Texture2D tex = new Texture2D(pattern.Width, pattern.Height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color[] clearPixels = new Color[pattern.Width * pattern.Height];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = Color.clear;
            }
            tex.SetPixels(clearPixels);

            if (pattern.Pixels != null)
            {
                for (int i = 0; i < pattern.Pixels.Length; i++)
                {
                    PixelEntry entry = pattern.Pixels[i];
                    tex.SetPixel(entry.X, entry.Y, entry.Color);
                }
            }
            tex.Apply();

            GameObject thumbnailObj = Instantiate(_thumbnailPrefab, _thumbnailContainer);
            thumbnailObj.SetActive(true);

            RawImage rawImage = thumbnailObj.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = tex;
            }

            Button button = thumbnailObj.GetComponent<Button>();
            if (button != null)
            {
                int capturedIndex = index;
                button.onClick.AddListener(() =>
                {
                    _selectedPatternIndex.Value = capturedIndex;
                    _onLaunchPattern.Raise();
                });
            }

            _thumbnails.Add(thumbnailObj);
        }
    }
}
