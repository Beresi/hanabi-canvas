// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Configuration SO for Challenge Mode settings including predefined requests and constraint defaults.
    /// </summary>
    [CreateAssetMenu(fileName = "New Challenge Config", menuName = "Hanabi Canvas/Config/Challenge Config")]
    public class ChallengeConfigSO : ScriptableObject
    {
        // ---- Constants ----
        private const int MIN_MAX_ACTIVE_REQUESTS = 1;
        private const int MAX_MAX_ACTIVE_REQUESTS = 10;
        private const float MIN_TIME_LIMIT = 5f;
        private const int MIN_COLOR_LIMIT = 1;

        // ---- Serialized Fields ----
        [Header("Requests")]
        [Tooltip("Array of predefined drawing requests for Challenge Mode")]
        [SerializeField] private RequestData[] _predefinedRequests;

        [Header("Limits")]
        [Tooltip("Maximum number of active requests the player can have at once")]
        [Range(1, 10)]
        [SerializeField] private int _maxActiveRequests = 3;

        [Tooltip("Default time limit in seconds for timed constraints")]
        [Min(5f)]
        [SerializeField] private float _defaultTimeLimit = 60f;

        [Tooltip("Default maximum number of unique colors allowed")]
        [Range(1, 8)]
        [SerializeField] private int _defaultColorLimit = 4;

        // ---- Properties ----

        /// <summary>Array of predefined drawing requests for Challenge Mode.</summary>
        public RequestData[] PredefinedRequests => _predefinedRequests;

        /// <summary>Maximum number of active requests the player can have at once.</summary>
        public int MaxActiveRequests => _maxActiveRequests;

        /// <summary>Default time limit in seconds for timed constraints.</summary>
        public float DefaultTimeLimit => _defaultTimeLimit;

        /// <summary>Default maximum number of unique colors allowed.</summary>
        public int DefaultColorLimit => _defaultColorLimit;

        // ---- Validation ----
        private void OnValidate()
        {
            _maxActiveRequests = Mathf.Clamp(_maxActiveRequests, MIN_MAX_ACTIVE_REQUESTS, MAX_MAX_ACTIVE_REQUESTS);
            _defaultTimeLimit = Mathf.Max(MIN_TIME_LIMIT, _defaultTimeLimit);
            _defaultColorLimit = Mathf.Max(MIN_COLOR_LIMIT, _defaultColorLimit);
        }
    }
}
