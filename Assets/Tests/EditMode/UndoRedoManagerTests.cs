// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Runtime.Canvas;

namespace HanabiCanvas.Tests.EditMode
{
    public class UndoRedoManagerTests
    {
        // ---- Constants ----
        private const int GRID_SIZE = 4;
        private const int MAX_DEPTH = 5;

        // ---- Private Fields ----
        private UndoRedoManager _manager;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _manager = new UndoRedoManager(MAX_DEPTH, GRID_SIZE);
        }

        // ---- Helpers ----

        private Color32[] CreateGrid(byte fillValue)
        {
            Color32[] grid = new Color32[GRID_SIZE];
            for (int i = 0; i < GRID_SIZE; i++)
            {
                grid[i] = new Color32(fillValue, fillValue, fillValue, 255);
            }
            return grid;
        }

        // ---- Tests ----

        [Test]
        public void PushAndUndo_RestoresPreviousState()
        {
            Color32[] original = CreateGrid(100);
            Color32[] modified = CreateGrid(200);

            _manager.PushSnapshot(original);
            Color32[] restored = _manager.Undo(modified);

            Assert.IsNotNull(restored);
            Assert.AreEqual(original[0].r, restored[0].r);
        }

        [Test]
        public void Undo_EmptyStack_ReturnsNull()
        {
            Color32[] current = CreateGrid(100);
            Color32[] result = _manager.Undo(current);

            Assert.IsNull(result);
        }

        [Test]
        public void Redo_AfterUndo_RestoresState()
        {
            Color32[] state1 = CreateGrid(100);
            Color32[] state2 = CreateGrid(200);

            _manager.PushSnapshot(state1);
            _manager.Undo(state2);
            Color32[] restored = _manager.Redo(state1);

            Assert.IsNotNull(restored);
            Assert.AreEqual(state2[0].r, restored[0].r);
        }

        [Test]
        public void NewPaint_ClearsRedoStack()
        {
            Color32[] state1 = CreateGrid(100);
            Color32[] state2 = CreateGrid(200);
            Color32[] state3 = CreateGrid(150);

            _manager.PushSnapshot(state1);
            _manager.Undo(state2);

            Assert.IsTrue(_manager.CanRedo);

            _manager.PushSnapshot(state3);

            Assert.IsFalse(_manager.CanRedo);
        }

        [Test]
        public void MaxDepth_OldestSnapshotDropped()
        {
            for (int i = 0; i < MAX_DEPTH + 3; i++)
            {
                _manager.PushSnapshot(CreateGrid((byte)(i * 10)));
            }

            Assert.AreEqual(MAX_DEPTH, _manager.UndoCount);
        }

        [Test]
        public void Clear_EmptiesBothStacks()
        {
            _manager.PushSnapshot(CreateGrid(100));
            _manager.PushSnapshot(CreateGrid(200));
            _manager.Undo(CreateGrid(200));

            Assert.IsTrue(_manager.CanUndo);
            Assert.IsTrue(_manager.CanRedo);

            _manager.Clear();

            Assert.IsFalse(_manager.CanUndo);
            Assert.IsFalse(_manager.CanRedo);
        }

        [Test]
        public void Redo_EmptyStack_ReturnsNull()
        {
            Color32[] current = CreateGrid(100);
            Color32[] result = _manager.Redo(current);

            Assert.IsNull(result);
        }

        [Test]
        public void PushSnapshot_NullGrid_DoesNothing()
        {
            _manager.PushSnapshot(null);

            Assert.IsFalse(_manager.CanUndo);
        }

        [Test]
        public void MultipleUndos_RestoreInOrder()
        {
            Color32[] state1 = CreateGrid(10);
            Color32[] state2 = CreateGrid(20);
            Color32[] state3 = CreateGrid(30);

            _manager.PushSnapshot(state1);
            _manager.PushSnapshot(state2);

            Color32[] restored1 = _manager.Undo(state3);
            Assert.AreEqual(20, restored1[0].r);

            Color32[] restored2 = _manager.Undo(state2);
            Assert.AreEqual(10, restored2[0].r);
        }
    }
}
