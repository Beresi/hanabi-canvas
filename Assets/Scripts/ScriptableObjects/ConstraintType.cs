// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Types of drawing constraints for Challenge Mode requests.
    /// </summary>
    public enum ConstraintType
    {
        /// <summary>Maximum number of unique colors allowed.</summary>
        ColorLimit,

        /// <summary>Drawing time limit in seconds.</summary>
        TimeLimit,

        /// <summary>Must use symmetry mode.</summary>
        SymmetryRequired,

        /// <summary>Maximum or minimum number of filled pixels.</summary>
        PixelLimit,

        /// <summary>Only specific palette indices allowed.</summary>
        PaletteRestriction
    }
}
