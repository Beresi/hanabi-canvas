// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime.UI
{
    /// <summary>
    /// Sound system manager. Listens to game events and plays appropriate audio clips.
    /// Audio sources are created programmatically in Awake — no manual setup required.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // ---- Constants ----
        private const int ONE_SHOT_SOURCE_COUNT = 4;

        // ---- Serialized Fields ----

        [Header("Config")]
        [Tooltip("Audio configuration with clips, volumes, and pitch settings")]
        [SerializeField] private AudioConfigSO _audioConfig;

        [Header("Shared Variables")]
        [Tooltip("Master volume — updated by Settings UI")]
        [SerializeField] private FloatVariableSO _masterVolume;

        [Header("Events — Listened")]
        [Tooltip("Raised when a pixel is painted")]
        [SerializeField] private GameEventSO _onPixelPainted;

        [Tooltip("Raised when user launches a firework")]
        [SerializeField] private GameEventSO _onLaunchFirework;

        [Tooltip("Raised when all firework behaviours complete")]
        [SerializeField] private GameEventSO _onFireworkComplete;

        // ---- Private Fields ----
        private AudioSource[] _oneShotSources;
        private AudioSource _ambienceSource;
        private int _nextOneShotIndex;

        // ---- Unity Methods ----

        private void Awake()
        {
            CreateAudioSources();
        }

        private void OnEnable()
        {
            if (_onPixelPainted != null)
            {
                _onPixelPainted.Register(HandlePixelPainted);
            }

            if (_onLaunchFirework != null)
            {
                _onLaunchFirework.Register(HandleLaunchFirework);
            }

            if (_onFireworkComplete != null)
            {
                _onFireworkComplete.Register(HandleFireworkComplete);
            }

            if (_masterVolume != null)
            {
                _masterVolume.OnValueChanged += HandleVolumeChanged;
            }
        }

        private void OnDisable()
        {
            if (_onPixelPainted != null)
            {
                _onPixelPainted.Unregister(HandlePixelPainted);
            }

            if (_onLaunchFirework != null)
            {
                _onLaunchFirework.Unregister(HandleLaunchFirework);
            }

            if (_onFireworkComplete != null)
            {
                _onFireworkComplete.Unregister(HandleFireworkComplete);
            }

            if (_masterVolume != null)
            {
                _masterVolume.OnValueChanged -= HandleVolumeChanged;
            }
        }

        // ---- Public Methods ----

        /// <summary>
        /// Plays a one-shot audio clip on the next available audio source.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <param name="volumeScale">Volume scale (0-1).</param>
        public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _oneShotSources == null)
            {
                return;
            }

            float masterVol = _masterVolume != null ? _masterVolume.Value : 1f;
            AudioSource source = _oneShotSources[_nextOneShotIndex];
            source.PlayOneShot(clip, volumeScale * masterVol);
            _nextOneShotIndex = (_nextOneShotIndex + 1) % ONE_SHOT_SOURCE_COUNT;
        }

        /// <summary>
        /// Plays the UI click sound from the audio config.
        /// </summary>
        public void PlayUIClick()
        {
            if (_audioConfig != null && _audioConfig.UiClickSound != null)
            {
                PlayOneShot(_audioConfig.UiClickSound);
            }
        }

        /// <summary>
        /// Sets the master volume on all audio sources.
        /// </summary>
        /// <param name="volume">Volume level (0-1).</param>
        public void SetMasterVolume(float volume)
        {
            if (_oneShotSources != null)
            {
                for (int i = 0; i < _oneShotSources.Length; i++)
                {
                    _oneShotSources[i].volume = volume;
                }
            }

            if (_ambienceSource != null)
            {
                _ambienceSource.volume = volume;
            }
        }

        /// <summary>
        /// Starts looping ambience audio.
        /// </summary>
        /// <param name="clip">The ambience clip to loop.</param>
        public void StartAmbience(AudioClip clip)
        {
            if (_ambienceSource == null || clip == null)
            {
                return;
            }

            _ambienceSource.clip = clip;
            _ambienceSource.loop = true;
            float masterVol = _masterVolume != null ? _masterVolume.Value : 1f;
            _ambienceSource.volume = masterVol;
            _ambienceSource.Play();
        }

        /// <summary>
        /// Stops the looping ambience audio.
        /// </summary>
        public void StopAmbience()
        {
            if (_ambienceSource != null)
            {
                _ambienceSource.Stop();
                _ambienceSource.clip = null;
            }
        }

        // ---- Private Methods ----

        private void CreateAudioSources()
        {
            _oneShotSources = new AudioSource[ONE_SHOT_SOURCE_COUNT];

            for (int i = 0; i < ONE_SHOT_SOURCE_COUNT; i++)
            {
                _oneShotSources[i] = gameObject.AddComponent<AudioSource>();
                _oneShotSources[i].playOnAwake = false;
            }

            _ambienceSource = gameObject.AddComponent<AudioSource>();
            _ambienceSource.playOnAwake = false;
            _ambienceSource.loop = true;

            _nextOneShotIndex = 0;
        }

        private void HandlePixelPainted()
        {
            if (_audioConfig == null || _audioConfig.DrawSounds == null || _audioConfig.DrawSounds.Length == 0)
            {
                return;
            }

            int index = Random.Range(0, _audioConfig.DrawSounds.Length);
            AudioClip clip = _audioConfig.DrawSounds[index];

            if (clip == null)
            {
                return;
            }

            float pitch = Random.Range(_audioConfig.PitchRange.x, _audioConfig.PitchRange.y);
            AudioSource source = _oneShotSources[_nextOneShotIndex];
            source.pitch = pitch;
            PlayOneShot(clip);
        }

        private void HandleLaunchFirework()
        {
            if (_audioConfig != null && _audioConfig.LaunchSound != null)
            {
                PlayOneShot(_audioConfig.LaunchSound);
            }

            if (_audioConfig != null && _audioConfig.SparkleLoop != null)
            {
                StartAmbience(_audioConfig.SparkleLoop);
            }
        }

        private void HandleFireworkComplete()
        {
            StopAmbience();

            if (_audioConfig != null && _audioConfig.FadeSound != null)
            {
                PlayOneShot(_audioConfig.FadeSound);
            }
        }

        private void HandleVolumeChanged(float volume)
        {
            SetMasterVolume(volume);
        }
    }
}
