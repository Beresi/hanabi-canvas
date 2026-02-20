// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Configuration SO for slideshow playback timing and behaviour settings.
    /// </summary>
    [CreateAssetMenu(fileName = "New Slideshow Config", menuName = "Hanabi Canvas/Config/Slideshow Config")]
    public class SlideshowConfigSO : ScriptableObject
    {
        // ---- Constants ----
        private const float MIN_DISPLAY_DURATION = 1f;
        private const float MIN_TRANSITION_DURATION = 0.1f;

        // ---- Serialized Fields ----
        [Header("Timing")]
        [Tooltip("Duration in seconds each artwork is displayed before transitioning")]
        [Min(1f)]
        [SerializeField] private float _artworkDisplayDuration = 8f;

        [Tooltip("Duration in seconds for the transition between artworks")]
        [Min(0.1f)]
        [SerializeField] private float _transitionDuration = 2f;

        [Header("Playback")]
        [Tooltip("Whether the slideshow loops back to the first artwork after the last")]
        [SerializeField] private bool _isLooping = true;

        [Tooltip("Whether the slideshow order is randomized")]
        [SerializeField] private bool _isShuffled = false;

        // ---- Properties ----

        /// <summary>Duration in seconds each artwork is displayed before transitioning.</summary>
        public float ArtworkDisplayDuration => _artworkDisplayDuration;

        /// <summary>Duration in seconds for the transition between artworks.</summary>
        public float TransitionDuration => _transitionDuration;

        /// <summary>Whether the slideshow loops back to the first artwork after the last.</summary>
        public bool IsLooping => _isLooping;

        /// <summary>Whether the slideshow order is randomized.</summary>
        public bool IsShuffled => _isShuffled;

        // ---- Validation ----
        private void OnValidate()
        {
            _artworkDisplayDuration = Mathf.Max(MIN_DISPLAY_DURATION, _artworkDisplayDuration);
            _transitionDuration = Mathf.Max(MIN_TRANSITION_DURATION, _transitionDuration);
        }
    }
}
