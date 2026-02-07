// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Firework Phase", menuName = "Hanabi Canvas/Config/Firework Phase")]
    public class FireworkPhaseSO : ScriptableObject
    {
        // ---- Constants ----
        private const float MIN_DURATION = 0.01f;

        // ---- Serialized Fields ----
        [Header("Phase Info")]
        [Tooltip("Name of this phase (e.g., Burst, Steer, Hold, Fade)")]
        [SerializeField] private string _phaseName = "Phase";

        [Tooltip("Duration of this phase in seconds")]
        [Min(0.01f)]
        [SerializeField] private float _duration = 1.0f;

        [Tooltip("Progress curve. X = normalized time (0-1), Y = progress output (0-1)")]
        [SerializeField] private AnimationCurve _progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Editor")]
        [Tooltip("Description of what happens during this phase")]
        [TextArea(2, 4)]
        [SerializeField] private string _description = "";

        // ---- Properties ----
        public string PhaseName => _phaseName;
        public float Duration => _duration;
        public AnimationCurve ProgressCurve => _progressCurve;
        public string Description => _description;

        // ---- Validation ----
        private void OnValidate()
        {
            _duration = Mathf.Max(MIN_DURATION, _duration);
        }
    }
}
