// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Modes;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Tests.EditMode
{
    public class LikeManagerTests
    {
        // ---- Private Fields ----
        private GameObject _dataManagerGO;
        private DataManager _dataManager;
        private GameEventSO _onArtworkLiked;
        private string _testArtworkId;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _dataManagerGO = new GameObject("TestDataManager");
            _dataManager = _dataManagerGO.AddComponent<DataManager>();
            _onArtworkLiked = ScriptableObject.CreateInstance<GameEventSO>();

            _testArtworkId = "test-artwork-1";
            PixelEntry[] pixels = new PixelEntry[]
            {
                new PixelEntry(0, 0, new Color32(255, 0, 0, 255))
            };

            ArtworkData artwork = new ArtworkData(
                _testArtworkId, "Test Artwork", pixels, 32, 32, 0);
            _dataManager.AddArtwork(artwork);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_dataManagerGO);
            Object.DestroyImmediate(_onArtworkLiked);
        }

        // ---- Tests ----

        [Test]
        public void ToggleLike_DelegatesToDataManager()
        {
            LikeManager.ToggleLike(_testArtworkId, _dataManager, _onArtworkLiked);

            Assert.IsTrue(_dataManager.HasLiked(_testArtworkId));
        }

        [Test]
        public void ToggleLike_RaisesEvent()
        {
            bool wasRaised = false;
            _onArtworkLiked.Register(() => wasRaised = true);

            LikeManager.ToggleLike(_testArtworkId, _dataManager, _onArtworkLiked);

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void HasLiked_DelegatesToDataManager()
        {
            Assert.IsFalse(LikeManager.HasLiked(_testArtworkId, _dataManager));

            LikeManager.ToggleLike(_testArtworkId, _dataManager);

            Assert.IsTrue(LikeManager.HasLiked(_testArtworkId, _dataManager));
        }

        [Test]
        public void ToggleLike_NullDataManager_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => LikeManager.ToggleLike("id", null));
        }

        [Test]
        public void HasLiked_NullDataManager_ReturnsFalse()
        {
            Assert.IsFalse(LikeManager.HasLiked("id", null));
        }
    }
}
