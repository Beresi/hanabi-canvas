// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Serializable data representing a Challenge Mode drawing request with constraints.
    /// </summary>
    [System.Serializable]
    public struct RequestData
    {
        // ---- Serialized Fields ----
        [SerializeField] private string _id;
        [SerializeField] private string _prompt;
        [SerializeField] private ConstraintData[] _constraints;
        [SerializeField] private bool _isCompleted;

        // ---- Properties ----

        /// <summary>Unique identifier for this request.</summary>
        public string Id => _id;

        /// <summary>Text prompt describing what the player should draw.</summary>
        public string Prompt => _prompt;

        /// <summary>Array of constraints the player must satisfy.</summary>
        public ConstraintData[] Constraints => _constraints;

        /// <summary>Whether this request has been completed by the player.</summary>
        public bool IsCompleted => _isCompleted;

        // ---- Constructor ----

        /// <summary>
        /// Creates a new <see cref="RequestData"/> with all fields specified.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        /// <param name="prompt">Drawing prompt text.</param>
        /// <param name="constraints">Array of constraints.</param>
        /// <param name="isCompleted">Whether the request is completed.</param>
        public RequestData(string id, string prompt, ConstraintData[] constraints, bool isCompleted = false)
        {
            _id = id;
            _prompt = prompt;
            _constraints = constraints;
            _isCompleted = isCompleted;
        }

        // ---- Public Methods ----

        /// <summary>
        /// Returns a new <see cref="RequestData"/> with <see cref="IsCompleted"/> set to true.
        /// All other fields are preserved.
        /// </summary>
        public RequestData WithCompleted()
        {
            return new RequestData(_id, _prompt, _constraints, true);
        }
    }
}
