// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime.Firework
{
    /// <summary>
    /// Debug utility that raises a spark request on key press.
    /// Uses a <see cref="PixelDataSO"/> for pattern data if assigned,
    /// otherwise falls back to a hardcoded test pattern.
    /// </summary>
    public class SparkTestTrigger : MonoBehaviour
    {
        // ---- Constants ----
        private const int TEST_PATTERN_SIZE = 32;

        // ---- Serialized Fields ----
        [Header("Events")]
        [Tooltip("Spark request event to raise")]
        [SerializeField] private SparkRequestEventSO _onSparkRequested;

        [Header("Spark Settings")]
        [Tooltip("World position for the burst origin")]
        [SerializeField] private Transform _spawnPoint;

        [Header("Pattern Source")]
        [Tooltip("Optional pixel data SO for pattern. If null, uses a hardcoded test cross.")]
        [SerializeField] private PixelDataSO _pixelData;

        // ---- Unity Methods ----
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_onSparkRequested != null && _spawnPoint != null)
                {
                    PixelEntry[] pattern;
                    int patternWidth;
                    int patternHeight;

                    if (_pixelData != null)
                    {
                        pattern = GetPatternFromPixelData(_pixelData);
                        patternWidth = _pixelData.Width;
                        patternHeight = _pixelData.Height;
                    }
                    else
                    {
                        pattern = CreateTestCrossPattern();
                        patternWidth = TEST_PATTERN_SIZE;
                        patternHeight = TEST_PATTERN_SIZE;
                    }

                    _onSparkRequested.Raise(new SparkRequest
                    {
                        Position = _spawnPoint.position,
                        Pattern = pattern,
                        PatternWidth = patternWidth,
                        PatternHeight = patternHeight
                    });
                }
                else
                {
                    Debug.LogWarning(
                        $"[{nameof(SparkTestTrigger)}] Event or spawn point is not assigned.", this);
                }
            }
        }

        /// <summary>
        /// Extracts pixel entries from a PixelDataSO into an array.
        /// </summary>
        private static PixelEntry[] GetPatternFromPixelData(PixelDataSO pixelData)
        {
            int count = pixelData.PixelCount;
            PixelEntry[] entries = new PixelEntry[count];
            for (int i = 0; i < count; i++)
            {
                entries[i] = pixelData.GetPixelAt(i);
            }

            return entries;
        }

        /// <summary>
        /// Creates a simple cross pattern for testing when no PixelDataSO is assigned.
        /// </summary>
        private static PixelEntry[] CreateTestCrossPattern()
        {
            Color32 red = new Color32(255, 80, 80, 255);
            Color32 yellow = new Color32(255, 200, 60, 255);

            // Simple cross: horizontal and vertical lines through center
            int count = 0;
            PixelEntry[] temp = new PixelEntry[TEST_PATTERN_SIZE * 2];

            // Horizontal line at y=16
            for (int x = 0; x < TEST_PATTERN_SIZE; x++)
            {
                temp[count++] = new PixelEntry((byte)x, 16, red);
            }

            // Vertical line at x=16 (skip center to avoid duplicate)
            for (int y = 0; y < TEST_PATTERN_SIZE; y++)
            {
                if (y == 16)
                {
                    continue;
                }

                temp[count++] = new PixelEntry((byte)16, (byte)y, yellow);
            }

            PixelEntry[] result = new PixelEntry[count];
            System.Array.Copy(temp, result, count);
            return result;
        }
    }
}
