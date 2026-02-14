// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class PatternListSOTests
    {
        // ---- Private Fields ----
        private PatternListSO _patternList;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _patternList = ScriptableObject.CreateInstance<PatternListSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_patternList);
        }

        // ---- Helpers ----
        private FireworkPattern CreateTestPattern()
        {
            return new FireworkPattern
            {
                Pixels = new PixelEntry[]
                {
                    new PixelEntry(0, 0, new Color32(255, 0, 0, 255)),
                    new PixelEntry(1, 0, new Color32(0, 255, 0, 255)),
                },
                Width = 2,
                Height = 1,
            };
        }

        // ---- Tests ----
        [Test]
        public void Add_SinglePattern_IncrementsCount()
        {
            FireworkPattern pattern = CreateTestPattern();

            _patternList.Add(pattern);

            Assert.AreEqual(1, _patternList.Count);
        }

        [Test]
        public void Add_Pattern_FiresOnItemAdded()
        {
            bool isFired = false;
            _patternList.OnItemAdded += (_) => isFired = true;

            _patternList.Add(CreateTestPattern());

            Assert.IsTrue(isFired);
        }

        [Test]
        public void GetAt_ValidIndex_ReturnsPattern()
        {
            FireworkPattern pattern = CreateTestPattern();
            _patternList.Add(pattern);

            FireworkPattern retrieved = _patternList.GetAt(0);

            Assert.AreEqual(pattern.Width, retrieved.Width);
            Assert.AreEqual(pattern.Height, retrieved.Height);
            Assert.AreEqual(pattern.Pixels.Length, retrieved.Pixels.Length);
        }

        [Test]
        public void Clear_AfterAdd_ResetsCount()
        {
            _patternList.Add(CreateTestPattern());
            _patternList.Add(CreateTestPattern());

            _patternList.Clear();

            Assert.AreEqual(0, _patternList.Count);
        }

        [Test]
        public void Clear_FiresOnCleared()
        {
            bool isFired = false;
            _patternList.OnCleared += () => isFired = true;
            _patternList.Add(CreateTestPattern());

            _patternList.Clear();

            Assert.IsTrue(isFired);
        }
    }
}
