// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime.UI
{
    /// <summary>
    /// In-game HUD overlay during Challenge Mode. Displays a countdown timer,
    /// constraint indicators, and flashes warnings on constraint violations.
    /// </summary>
    public class ChallengeHUD : MonoBehaviour
    {
        // ---- Constants ----
        private const float WARNING_THRESHOLD = 10f;
        private const float CRITICAL_THRESHOLD = 5f;
        private const float FLASH_DURATION = 0.5f;

        // ---- Serialized Fields ----

        [Header("Shared Variables")]
        [Tooltip("True when Challenge Mode is active")]
        [SerializeField] private BoolVariableSO _isChallengeMode;

        [Tooltip("Remaining time for time-limited constraints")]
        [SerializeField] private FloatVariableSO _remainingTime;

        [Tooltip("Current unique color count on canvas")]
        [SerializeField] private IntVariableSO _uniqueColorCount;

        [Tooltip("Current filled pixel count on canvas")]
        [SerializeField] private IntVariableSO _filledPixelCount;

        [Header("Events")]
        [Tooltip("Raised when a pixel is painted")]
        [SerializeField] private GameEventSO _onPixelPainted;

        [Tooltip("Raised when a constraint is violated")]
        [SerializeField] private GameEventSO _onConstraintViolated;

        [Header("UI Elements")]
        [Tooltip("Timer countdown text")]
        [SerializeField] private TextMeshProUGUI _timerText;

        [Tooltip("Color count constraint text")]
        [SerializeField] private TextMeshProUGUI _colorCountText;

        [Tooltip("Pixel count constraint text")]
        [SerializeField] private TextMeshProUGUI _pixelCountText;

        [Tooltip("Warning flash overlay image")]
        [SerializeField] private Image _warningFlash;

        [Header("Timer Colors")]
        [Tooltip("Normal timer color")]
        [SerializeField] private Color _normalTimerColor = Color.white;

        [Tooltip("Warning timer color (< 10s)")]
        [SerializeField] private Color _warningTimerColor = Color.yellow;

        [Tooltip("Critical timer color (< 5s)")]
        [SerializeField] private Color _criticalTimerColor = Color.red;

        // ---- Private Fields ----
        private float _flashTimer;
        private bool _isFlashing;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_isChallengeMode != null)
            {
                _isChallengeMode.OnValueChanged += HandleChallengeModeChanged;
            }

            if (_onPixelPainted != null)
            {
                _onPixelPainted.Register(HandlePixelPainted);
            }

            if (_onConstraintViolated != null)
            {
                _onConstraintViolated.Register(HandleConstraintViolated);
            }

            if (_remainingTime != null)
            {
                _remainingTime.OnValueChanged += HandleTimeChanged;
            }

            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (_isChallengeMode != null)
            {
                _isChallengeMode.OnValueChanged -= HandleChallengeModeChanged;
            }

            if (_onPixelPainted != null)
            {
                _onPixelPainted.Unregister(HandlePixelPainted);
            }

            if (_onConstraintViolated != null)
            {
                _onConstraintViolated.Unregister(HandleConstraintViolated);
            }

            if (_remainingTime != null)
            {
                _remainingTime.OnValueChanged -= HandleTimeChanged;
            }
        }

        private void Update()
        {
            if (_isFlashing)
            {
                UpdateFlash();
            }
        }

        // ---- Private Methods ----

        private void HandleChallengeModeChanged(bool isActive)
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            bool isVisible = _isChallengeMode != null && _isChallengeMode.Value;
            gameObject.SetActive(isVisible);
        }

        private void HandlePixelPainted()
        {
            RefreshConstraintIndicators();
        }

        private void HandleTimeChanged(float time)
        {
            RefreshTimerDisplay(time);
        }

        private void HandleConstraintViolated()
        {
            StartWarningFlash();
        }

        private void RefreshTimerDisplay(float time)
        {
            if (_timerText == null)
            {
                return;
            }

            int seconds = Mathf.CeilToInt(time);
            _timerText.text = seconds.ToString();

            if (time <= CRITICAL_THRESHOLD)
            {
                _timerText.color = _criticalTimerColor;
            }
            else if (time <= WARNING_THRESHOLD)
            {
                _timerText.color = _warningTimerColor;
            }
            else
            {
                _timerText.color = _normalTimerColor;
            }
        }

        private void RefreshConstraintIndicators()
        {
            if (_colorCountText != null && _uniqueColorCount != null)
            {
                _colorCountText.text = "Colors: " + _uniqueColorCount.Value;
            }

            if (_pixelCountText != null && _filledPixelCount != null)
            {
                _pixelCountText.text = "Pixels: " + _filledPixelCount.Value;
            }
        }

        private void StartWarningFlash()
        {
            _isFlashing = true;
            _flashTimer = FLASH_DURATION;

            if (_warningFlash != null)
            {
                Color flashColor = _criticalTimerColor;
                flashColor.a = 0.3f;
                _warningFlash.color = flashColor;
                _warningFlash.gameObject.SetActive(true);
            }
        }

        private void UpdateFlash()
        {
            _flashTimer -= Time.deltaTime;

            if (_flashTimer <= 0f)
            {
                _isFlashing = false;

                if (_warningFlash != null)
                {
                    _warningFlash.gameObject.SetActive(false);
                }

                return;
            }

            if (_warningFlash != null)
            {
                float alpha = 0.3f * (_flashTimer / FLASH_DURATION);
                Color color = _warningFlash.color;
                color.a = alpha;
                _warningFlash.color = color;
            }
        }
    }
}
