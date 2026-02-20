// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Serializable data describing a single drawing constraint for Challenge Mode.
    /// </summary>
    [System.Serializable]
    public struct ConstraintData
    {
        // ---- Serialized Fields ----
        [SerializeField] private ConstraintType _type;
        [SerializeField] private int _intValue;
        [SerializeField] private float _floatValue;
        [SerializeField] private bool _boolValue;

        // ---- Properties ----

        /// <summary>The type of constraint.</summary>
        public ConstraintType Type => _type;

        /// <summary>Integer parameter for the constraint (e.g., color count, pixel count).</summary>
        public int IntValue => _intValue;

        /// <summary>Float parameter for the constraint (e.g., time limit in seconds).</summary>
        public float FloatValue => _floatValue;

        /// <summary>Boolean parameter for the constraint (e.g., symmetry required).</summary>
        public bool BoolValue => _boolValue;

        // ---- Constructor ----

        /// <summary>
        /// Creates a new <see cref="ConstraintData"/> with the specified type and values.
        /// </summary>
        /// <param name="type">The type of constraint.</param>
        /// <param name="intValue">Integer parameter value.</param>
        /// <param name="floatValue">Float parameter value.</param>
        /// <param name="boolValue">Boolean parameter value.</param>
        public ConstraintData(ConstraintType type, int intValue = 0, float floatValue = 0f, bool boolValue = false)
        {
            _type = type;
            _intValue = intValue;
            _floatValue = floatValue;
            _boolValue = boolValue;
        }
    }
}
