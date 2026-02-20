// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.GameFlow;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Runtime.Modes
{
    /// <summary>
    /// Challenge Mode controller. Enforces constraints during drawing (color limit,
    /// time limit, symmetry required, pixel limit) and validates before launch.
    /// On successful completion, marks the request complete and saves the artwork.
    /// </summary>
    public class ChallengeModeController : MonoBehaviour
    {
        // ---- Constants ----
        private const float WARNING_THRESHOLD_SECONDS = 10f;
        private const float CRITICAL_THRESHOLD_SECONDS = 5f;

        // ---- Serialized Fields ----

        [Header("Session")]
        [Tooltip("The session manager that handles the Drawing->Launch->Watch->Reset cycle")]
        [SerializeField] private FireworkSessionManager _sessionManager;

        [Header("Data")]
        [Tooltip("Data manager for saving artworks and completing requests")]
        [SerializeField] private DataManager _dataManager;

        [Tooltip("Shared pixel data — read to build ArtworkData on save")]
        [SerializeField] private PixelDataSO _pixelData;

        [Tooltip("Canvas config for reading grid dimensions")]
        [SerializeField] private CanvasConfigSO _canvasConfig;

        [Tooltip("Challenge config with default constraint values")]
        [SerializeField] private ChallengeConfigSO _challengeConfig;

        [Header("Shared Variables")]
        [Tooltip("Controls canvas input")]
        [SerializeField] private BoolVariableSO _isCanvasInputEnabled;

        [Tooltip("Written true when symmetry is required by constraint")]
        [SerializeField] private BoolVariableSO _isSymmetryEnabled;

        [Tooltip("Remaining time for time-limited constraints")]
        [SerializeField] private FloatVariableSO _remainingTime;

        [Tooltip("Current unique color count on canvas")]
        [SerializeField] private IntVariableSO _uniqueColorCount;

        [Tooltip("Current filled pixel count on canvas")]
        [SerializeField] private IntVariableSO _filledPixelCount;

        [Header("Events — Listened")]
        [Tooltip("Raised when all firework behaviours complete")]
        [SerializeField] private GameEventSO _onFireworkComplete;

        [Tooltip("Raised when a pixel is painted on the canvas")]
        [SerializeField] private GameEventSO _onPixelPainted;

        [Header("Events — Raised")]
        [Tooltip("Raised when a constraint is violated")]
        [SerializeField] private GameEventSO _onConstraintViolated;

        [Tooltip("Raised to clear the canvas on reset")]
        [SerializeField] private GameEventSO _onCanvasCleared;

        // ---- Private Fields ----
        private bool _isActive;
        private RequestData _activeRequest;
        private bool _hasTimeLimit;
        private float _timeLimit;
        private bool _hasColorLimit;
        private int _colorLimit;
        private bool _hasPixelLimit;
        private int _pixelLimit;
        private bool _hasSymmetryRequired;

        // ---- Properties ----

        /// <summary>Whether Challenge Mode is currently active.</summary>
        public bool IsActive => _isActive;

        /// <summary>The currently active request, if any.</summary>
        public RequestData ActiveRequest => _activeRequest;

        // ---- Unity Methods ----

        private void OnEnable()
        {
            if (_onFireworkComplete != null)
            {
                _onFireworkComplete.Register(HandleFireworkComplete);
            }

            if (_onPixelPainted != null)
            {
                _onPixelPainted.Register(HandlePixelPainted);
            }
        }

        private void OnDisable()
        {
            if (_onFireworkComplete != null)
            {
                _onFireworkComplete.Unregister(HandleFireworkComplete);
            }

            if (_onPixelPainted != null)
            {
                _onPixelPainted.Unregister(HandlePixelPainted);
            }
        }

        private void Update()
        {
            if (!_isActive || !_hasTimeLimit)
            {
                return;
            }

            UpdateTimer();
        }

        // ---- Public Methods ----

        /// <summary>
        /// Activates Challenge Mode with no specific request. Uses default constraints.
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            ResetConstraintState();

            if (_isCanvasInputEnabled != null)
            {
                _isCanvasInputEnabled.Value = true;
            }

            if (_sessionManager != null)
            {
                _sessionManager.EnableSession();
            }
        }

        /// <summary>
        /// Activates Challenge Mode with the specified request and its constraints.
        /// </summary>
        /// <param name="request">The request to enforce.</param>
        public void Activate(RequestData request)
        {
            _isActive = true;
            _activeRequest = request;
            ParseConstraints(request);

            if (_isCanvasInputEnabled != null)
            {
                _isCanvasInputEnabled.Value = true;
            }

            if (_hasSymmetryRequired && _isSymmetryEnabled != null)
            {
                _isSymmetryEnabled.Value = true;
            }

            if (_hasTimeLimit && _remainingTime != null)
            {
                _remainingTime.Value = _timeLimit;
            }

            if (_sessionManager != null)
            {
                _sessionManager.EnableSession();
            }
        }

        /// <summary>
        /// Deactivates Challenge Mode and clears constraint state.
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            ResetConstraintState();

            if (_isCanvasInputEnabled != null)
            {
                _isCanvasInputEnabled.Value = false;
            }

            if (_sessionManager != null)
            {
                _sessionManager.DisableSession();
            }
        }

        /// <summary>
        /// Validates all active constraints. Returns true if all constraints are satisfied.
        /// </summary>
        public bool ValidateConstraints()
        {
            if (!_isActive)
            {
                return true;
            }

            if (_hasColorLimit && _uniqueColorCount != null)
            {
                if (_uniqueColorCount.Value > _colorLimit)
                {
                    RaiseConstraintViolated();
                    return false;
                }
            }

            if (_hasPixelLimit && _filledPixelCount != null)
            {
                if (_filledPixelCount.Value > _pixelLimit)
                {
                    RaiseConstraintViolated();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes the controller for testing without Unity serialization.
        /// </summary>
        public void Initialize(
            FireworkSessionManager sessionManager,
            DataManager dataManager,
            PixelDataSO pixelData,
            CanvasConfigSO canvasConfig,
            ChallengeConfigSO challengeConfig,
            BoolVariableSO isCanvasInputEnabled,
            BoolVariableSO isSymmetryEnabled,
            FloatVariableSO remainingTime,
            IntVariableSO uniqueColorCount,
            IntVariableSO filledPixelCount,
            GameEventSO onFireworkComplete,
            GameEventSO onPixelPainted,
            GameEventSO onConstraintViolated,
            GameEventSO onCanvasCleared)
        {
            _sessionManager = sessionManager;
            _dataManager = dataManager;
            _pixelData = pixelData;
            _canvasConfig = canvasConfig;
            _challengeConfig = challengeConfig;
            _isCanvasInputEnabled = isCanvasInputEnabled;
            _isSymmetryEnabled = isSymmetryEnabled;
            _remainingTime = remainingTime;
            _uniqueColorCount = uniqueColorCount;
            _filledPixelCount = filledPixelCount;
            _onFireworkComplete = onFireworkComplete;
            _onPixelPainted = onPixelPainted;
            _onConstraintViolated = onConstraintViolated;
            _onCanvasCleared = onCanvasCleared;
        }

        // ---- Private Methods ----

        private void ParseConstraints(RequestData request)
        {
            ResetConstraintState();

            if (request.Constraints == null)
            {
                return;
            }

            for (int i = 0; i < request.Constraints.Length; i++)
            {
                ConstraintData constraint = request.Constraints[i];

                switch (constraint.Type)
                {
                    case ConstraintType.ColorLimit:
                        _hasColorLimit = true;
                        _colorLimit = constraint.IntValue;
                        break;

                    case ConstraintType.TimeLimit:
                        _hasTimeLimit = true;
                        _timeLimit = constraint.FloatValue;
                        break;

                    case ConstraintType.SymmetryRequired:
                        _hasSymmetryRequired = constraint.BoolValue;
                        break;

                    case ConstraintType.PixelLimit:
                        _hasPixelLimit = true;
                        _pixelLimit = constraint.IntValue;
                        break;
                }
            }
        }

        private void ResetConstraintState()
        {
            _hasTimeLimit = false;
            _timeLimit = 0f;
            _hasColorLimit = false;
            _colorLimit = 0;
            _hasPixelLimit = false;
            _pixelLimit = 0;
            _hasSymmetryRequired = false;

            if (_isSymmetryEnabled != null)
            {
                _isSymmetryEnabled.Value = false;
            }
        }

        private void UpdateTimer()
        {
            if (_remainingTime == null)
            {
                return;
            }

            float newTime = _remainingTime.Value - Time.deltaTime;

            if (newTime <= 0f)
            {
                _remainingTime.Value = 0f;
                RaiseConstraintViolated();
                return;
            }

            _remainingTime.Value = newTime;
        }

        private void HandlePixelPainted()
        {
            if (!_isActive)
            {
                return;
            }

            if (_hasColorLimit && _uniqueColorCount != null)
            {
                if (_uniqueColorCount.Value > _colorLimit)
                {
                    RaiseConstraintViolated();
                }
            }

            if (_hasPixelLimit && _filledPixelCount != null)
            {
                if (_filledPixelCount.Value > _pixelLimit)
                {
                    RaiseConstraintViolated();
                }
            }
        }

        private void HandleFireworkComplete()
        {
            if (!_isActive)
            {
                return;
            }

            SaveArtworkAndCompleteRequest();
        }

        private void SaveArtworkAndCompleteRequest()
        {
            if (_dataManager == null || _pixelData == null)
            {
                return;
            }

            if (_pixelData.PixelCount == 0)
            {
                return;
            }

            // Save artwork
            int pixelCount = _pixelData.PixelCount;
            PixelEntry[] pixels = new PixelEntry[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                pixels[i] = _pixelData.GetPixelAt(i);
            }

            int width = _canvasConfig != null ? _canvasConfig.GridWidth : _pixelData.Width;
            int height = _canvasConfig != null ? _canvasConfig.GridHeight : _pixelData.Height;

            ArtworkData artwork = new ArtworkData(
                System.Guid.NewGuid().ToString(),
                _activeRequest.Prompt ?? "Challenge Artwork",
                pixels,
                width,
                height,
                System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _dataManager.AddArtwork(artwork);

            // Mark request complete
            if (_activeRequest.Id != null)
            {
                _dataManager.CompleteRequest(_activeRequest.Id);
            }
        }

        private void RaiseConstraintViolated()
        {
            if (_onConstraintViolated != null)
            {
                _onConstraintViolated.Raise();
            }
        }
    }
}
