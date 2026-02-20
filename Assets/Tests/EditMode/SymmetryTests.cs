// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;

namespace HanabiCanvas.Tests.EditMode
{
    /// <summary>
    /// Tests for symmetry mirror position calculation logic.
    /// Tests the arithmetic directly without requiring DrawingCanvas MonoBehaviour.
    /// </summary>
    public class SymmetryTests
    {
        // ---- Constants ----
        private const int GRID_SIZE = 32;

        // ---- Tests ----

        [Test]
        public void Horizontal_MirrorsXAxis()
        {
            int px = 5;
            int py = 10;
            int mirrorX = GRID_SIZE - 1 - px;

            Assert.AreEqual(26, mirrorX);
            Assert.AreEqual(10, py);
        }

        [Test]
        public void Vertical_MirrorsYAxis()
        {
            int px = 5;
            int py = 10;
            int mirrorY = GRID_SIZE - 1 - py;

            Assert.AreEqual(5, px);
            Assert.AreEqual(21, mirrorY);
        }

        [Test]
        public void Both_MirrorsAllFourPositions()
        {
            int px = 5;
            int py = 10;
            int mirrorX = GRID_SIZE - 1 - px;
            int mirrorY = GRID_SIZE - 1 - py;

            // Original
            Assert.AreEqual(5, px);
            Assert.AreEqual(10, py);

            // Horizontal mirror
            Assert.AreEqual(26, mirrorX);
            Assert.AreEqual(10, py);

            // Vertical mirror
            Assert.AreEqual(5, px);
            Assert.AreEqual(21, mirrorY);

            // Diagonal mirror
            Assert.AreEqual(26, mirrorX);
            Assert.AreEqual(21, mirrorY);
        }

        [Test]
        public void Center_MirrorsSamePosition()
        {
            int px = GRID_SIZE / 2;
            int py = GRID_SIZE / 2;
            int mirrorX = GRID_SIZE - 1 - px;
            int mirrorY = GRID_SIZE - 1 - py;

            // With even grid, center mirrors to grid_size/2 - 1
            Assert.AreEqual(15, mirrorX);
            Assert.AreEqual(15, mirrorY);
        }

        [Test]
        public void Edge_MirrorsToOppositeEdge()
        {
            int px = 0;
            int py = 0;
            int mirrorX = GRID_SIZE - 1 - px;
            int mirrorY = GRID_SIZE - 1 - py;

            Assert.AreEqual(31, mirrorX);
            Assert.AreEqual(31, mirrorY);
        }
    }
}
