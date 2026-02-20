// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HanabiCanvas.Runtime.Canvas
{
    /// <summary>
    /// Plain C# class managing undo/redo stacks for the pixel canvas.
    /// Stores grid snapshots as flat <see cref="Color32"/> arrays.
    /// Owned and driven by <see cref="DrawingCanvas"/>.
    /// </summary>
    public class UndoRedoManager
    {
        // ---- Private Fields ----
        private readonly List<Color32[]> _undoStack;
        private readonly List<Color32[]> _redoStack;
        private readonly int _maxDepth;
        private readonly int _gridSize;

        // ---- Properties ----

        /// <summary>Whether there are snapshots available to undo.</summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>Whether there are snapshots available to redo.</summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>Current undo stack depth.</summary>
        public int UndoCount => _undoStack.Count;

        /// <summary>Current redo stack depth.</summary>
        public int RedoCount => _redoStack.Count;

        // ---- Constructor ----

        /// <summary>
        /// Creates a new undo/redo manager with the specified max depth and grid size.
        /// </summary>
        /// <param name="maxDepth">Maximum number of undo snapshots to retain.</param>
        /// <param name="gridSize">Total number of pixels in the grid (width * height).</param>
        public UndoRedoManager(int maxDepth, int gridSize)
        {
            _maxDepth = Mathf.Max(1, maxDepth);
            _gridSize = gridSize;
            _undoStack = new List<Color32[]>(_maxDepth);
            _redoStack = new List<Color32[]>(_maxDepth);
        }

        // ---- Public Methods ----

        /// <summary>
        /// Saves the current grid state before a modification.
        /// Clears the redo stack (standard undo/redo behaviour).
        /// </summary>
        /// <param name="currentGrid">The current grid pixel data to snapshot.</param>
        public void PushSnapshot(Color32[] currentGrid)
        {
            if (currentGrid == null)
            {
                return;
            }

            Color32[] snapshot = new Color32[_gridSize];
            System.Array.Copy(currentGrid, snapshot, _gridSize);

            _undoStack.Add(snapshot);

            // Enforce max depth — drop oldest
            if (_undoStack.Count > _maxDepth)
            {
                _undoStack.RemoveAt(0);
            }

            // New action clears redo
            _redoStack.Clear();
        }

        /// <summary>
        /// Undoes the last action. Pushes the current state to the redo stack
        /// and returns the previous state.
        /// </summary>
        /// <param name="currentGrid">The current grid state to push to redo.</param>
        /// <returns>The restored grid state, or null if nothing to undo.</returns>
        public Color32[] Undo(Color32[] currentGrid)
        {
            if (_undoStack.Count == 0)
            {
                return null;
            }

            // Push current to redo
            if (currentGrid != null)
            {
                Color32[] redoSnapshot = new Color32[_gridSize];
                System.Array.Copy(currentGrid, redoSnapshot, _gridSize);
                _redoStack.Add(redoSnapshot);
            }

            // Pop from undo
            int lastIndex = _undoStack.Count - 1;
            Color32[] restored = _undoStack[lastIndex];
            _undoStack.RemoveAt(lastIndex);

            return restored;
        }

        /// <summary>
        /// Redoes the last undone action. Pushes the current state to the undo stack
        /// and returns the redo state.
        /// </summary>
        /// <param name="currentGrid">The current grid state to push to undo.</param>
        /// <returns>The restored grid state, or null if nothing to redo.</returns>
        public Color32[] Redo(Color32[] currentGrid)
        {
            if (_redoStack.Count == 0)
            {
                return null;
            }

            // Push current to undo
            if (currentGrid != null)
            {
                Color32[] undoSnapshot = new Color32[_gridSize];
                System.Array.Copy(currentGrid, undoSnapshot, _gridSize);
                _undoStack.Add(undoSnapshot);
            }

            // Pop from redo
            int lastIndex = _redoStack.Count - 1;
            Color32[] restored = _redoStack[lastIndex];
            _redoStack.RemoveAt(lastIndex);

            return restored;
        }

        /// <summary>
        /// Clears both undo and redo stacks.
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
