// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Firework;
using HanabiCanvas.Runtime.Modes;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Tests.EditMode
{
    public class SlideshowControllerTests
    {
        // ---- Private Fields ----
        private GameObject _controllerGO;
        private SlideshowController _controller;
        private GameObject _dataManagerGO;
        private DataManager _dataManager;
        private SlideshowConfigSO _slideshowConfig;
        private GameEventSO _onFireworkComplete;
        private GameEventSO _onSlideshowExitRequested;
        private FireworkRequestEventSO _onFireworkRequested;
        private GameEventSO _onSlideshowStarted;
        private GameEventSO _onSlideshowArtworkChanged;
        private GameEventSO _onSlideshowComplete;
        private ArtworkEventSO _onSlideshowArtworkStarted;
        private IntVariableSO _slideshowCurrentIndex;
        private IntVariableSO _slideshowTotalCount;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _controllerGO = new GameObject("TestSlideshowController");
            _controller = _controllerGO.AddComponent<SlideshowController>();

            _dataManagerGO = new GameObject("TestDataManager");
            _dataManager = _dataManagerGO.AddComponent<DataManager>();

            _slideshowConfig = ScriptableObject.CreateInstance<SlideshowConfigSO>();
            _onFireworkComplete = ScriptableObject.CreateInstance<GameEventSO>();
            _onSlideshowExitRequested = ScriptableObject.CreateInstance<GameEventSO>();
            _onFireworkRequested = ScriptableObject.CreateInstance<FireworkRequestEventSO>();
            _onSlideshowStarted = ScriptableObject.CreateInstance<GameEventSO>();
            _onSlideshowArtworkChanged = ScriptableObject.CreateInstance<GameEventSO>();
            _onSlideshowComplete = ScriptableObject.CreateInstance<GameEventSO>();
            _onSlideshowArtworkStarted = ScriptableObject.CreateInstance<ArtworkEventSO>();
            _slideshowCurrentIndex = ScriptableObject.CreateInstance<IntVariableSO>();
            _slideshowTotalCount = ScriptableObject.CreateInstance<IntVariableSO>();

            _controller.Initialize(
                _slideshowConfig, _dataManager, null,
                _onFireworkComplete, _onSlideshowExitRequested,
                _onFireworkRequested,
                _onSlideshowStarted, _onSlideshowArtworkChanged,
                _onSlideshowComplete, _onSlideshowArtworkStarted,
                _slideshowCurrentIndex, _slideshowTotalCount);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_controllerGO);
            Object.DestroyImmediate(_dataManagerGO);
            Object.DestroyImmediate(_slideshowConfig);
            Object.DestroyImmediate(_onFireworkComplete);
            Object.DestroyImmediate(_onSlideshowExitRequested);
            Object.DestroyImmediate(_onFireworkRequested);
            Object.DestroyImmediate(_onSlideshowStarted);
            Object.DestroyImmediate(_onSlideshowArtworkChanged);
            Object.DestroyImmediate(_onSlideshowComplete);
            Object.DestroyImmediate(_onSlideshowArtworkStarted);
            Object.DestroyImmediate(_slideshowCurrentIndex);
            Object.DestroyImmediate(_slideshowTotalCount);
        }

        // ---- Helpers ----

        private void AddTestArtwork(string name)
        {
            PixelEntry[] pixels = new PixelEntry[]
            {
                new PixelEntry(0, 0, new Color32(255, 0, 0, 255))
            };

            ArtworkData artwork = new ArtworkData(
                System.Guid.NewGuid().ToString(), name,
                pixels, 32, 32, 0);

            _dataManager.AddArtwork(artwork);
        }

        // ---- Tests ----

        [Test]
        public void StartSlideshow_WithArtworks_RaisesStartEvent()
        {
            AddTestArtwork("Test1");
            bool wasRaised = false;
            _onSlideshowStarted.Register(() => wasRaised = true);

            _controller.StartSlideshow();

            Assert.IsTrue(wasRaised);
            Assert.IsTrue(_controller.IsPlaying);
        }

        [Test]
        public void StartSlideshow_EmptyList_DoesNotStart()
        {
            _controller.StartSlideshow();

            Assert.IsFalse(_controller.IsPlaying);
        }

        [Test]
        public void StartSlideshow_SetsCurrentIndexToZero()
        {
            AddTestArtwork("Test1");
            AddTestArtwork("Test2");

            _controller.StartSlideshow();

            Assert.AreEqual(0, _controller.CurrentIndex);
            Assert.AreEqual(2, _controller.TotalCount);
        }

        [Test]
        public void StopSlideshow_ResetsState()
        {
            AddTestArtwork("Test1");
            _controller.StartSlideshow();

            _controller.StopSlideshow();

            Assert.IsFalse(_controller.IsPlaying);
            Assert.AreEqual(0, _controller.CurrentIndex);
        }

        [Test]
        public void StartSlideshow_RaisesFireworkRequest()
        {
            AddTestArtwork("Test1");
            bool wasRequested = false;
            _onFireworkRequested.Register((request) => wasRequested = true);

            _controller.StartSlideshow();

            Assert.IsTrue(wasRequested);
        }

        [Test]
        public void StartSlideshow_UpdatesSharedVariables()
        {
            AddTestArtwork("Test1");
            AddTestArtwork("Test2");

            _controller.StartSlideshow();

            Assert.AreEqual(0, _slideshowCurrentIndex.Value);
            Assert.AreEqual(2, _slideshowTotalCount.Value);
        }
    }
}
