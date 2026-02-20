// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Configuration SO for audio settings including sound clips, volume, and pitch ranges.
    /// </summary>
    [CreateAssetMenu(fileName = "New Audio Config", menuName = "Hanabi Canvas/Config/Audio Config")]
    public class AudioConfigSO : ScriptableObject
    {
        // ---- Constants ----
        private const float MIN_VOLUME = 0f;
        private const float MAX_VOLUME = 1f;
        private const float MIN_PITCH = 0.1f;
        private const float MAX_PITCH = 3f;

        // ---- Serialized Fields ----
        [Header("Drawing")]
        [Tooltip("Array of sound clips played when the player draws a pixel")]
        [SerializeField] private AudioClip[] _drawSounds;

        [Header("Firework")]
        [Tooltip("Sound clip played when a firework is launched")]
        [SerializeField] private AudioClip _launchSound;

        [Tooltip("Sound clip played when a firework bursts")]
        [SerializeField] private AudioClip _burstSound;

        [Tooltip("Looping sound clip for the sparkle phase of a firework")]
        [SerializeField] private AudioClip _sparkleLoop;

        [Tooltip("Sound clip played as the firework fades out")]
        [SerializeField] private AudioClip _fadeSound;

        [Header("UI")]
        [Tooltip("Sound clip played for UI button clicks")]
        [SerializeField] private AudioClip _uiClickSound;

        [Tooltip("Background music played on the main menu")]
        [SerializeField] private AudioClip _menuMusic;

        [Header("Settings")]
        [Tooltip("Master volume for all sounds (0 = muted, 1 = full volume)")]
        [Range(0f, 1f)]
        [SerializeField] private float _masterVolume = 1f;

        [Tooltip("Min/max pitch range for sound randomization (x = min, y = max)")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(0.9f, 1.1f);

        // ---- Properties ----

        /// <summary>Array of sound clips played when the player draws a pixel.</summary>
        public AudioClip[] DrawSounds => _drawSounds;

        /// <summary>Sound clip played when a firework is launched.</summary>
        public AudioClip LaunchSound => _launchSound;

        /// <summary>Sound clip played when a firework bursts.</summary>
        public AudioClip BurstSound => _burstSound;

        /// <summary>Looping sound clip for the sparkle phase of a firework.</summary>
        public AudioClip SparkleLoop => _sparkleLoop;

        /// <summary>Sound clip played as the firework fades out.</summary>
        public AudioClip FadeSound => _fadeSound;

        /// <summary>Sound clip played for UI button clicks.</summary>
        public AudioClip UiClickSound => _uiClickSound;

        /// <summary>Background music played on the main menu.</summary>
        public AudioClip MenuMusic => _menuMusic;

        /// <summary>Master volume for all sounds (0 = muted, 1 = full volume).</summary>
        public float MasterVolume => _masterVolume;

        /// <summary>Min/max pitch range for sound randomization (x = min, y = max).</summary>
        public Vector2 PitchRange => _pitchRange;

        // ---- Validation ----
        private void OnValidate()
        {
            _masterVolume = Mathf.Clamp(_masterVolume, MIN_VOLUME, MAX_VOLUME);
            _pitchRange.x = Mathf.Clamp(_pitchRange.x, MIN_PITCH, MAX_PITCH);
            _pitchRange.y = Mathf.Clamp(_pitchRange.y, MIN_PITCH, MAX_PITCH);

            if (_pitchRange.y < _pitchRange.x)
            {
                _pitchRange.y = _pitchRange.x;
            }
        }
    }
}
