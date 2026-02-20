// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Tests.EditMode
{
    public class DataManagerTests
    {
        // ---- Private Fields ----
        private GameEventSO _onDataChanged;
        private IntVariableSO _artworkCount;
        private IntVariableSO _activeRequestCount;
        private GameObject _gameObject;
        private DataManager _dataManager;

        // ---- Setup / Teardown ----

        [SetUp]
        public void SetUp()
        {
            _onDataChanged = ScriptableObject.CreateInstance<GameEventSO>();
            _artworkCount = ScriptableObject.CreateInstance<IntVariableSO>();
            _activeRequestCount = ScriptableObject.CreateInstance<IntVariableSO>();
            _gameObject = new GameObject("DataManager");
            _dataManager = _gameObject.AddComponent<DataManager>();

            SerializedObject so = new SerializedObject(_dataManager);
            so.FindProperty("_onDataChanged").objectReferenceValue = _onDataChanged;
            so.FindProperty("_artworkCount").objectReferenceValue = _artworkCount;
            so.FindProperty("_activeRequestCount").objectReferenceValue = _activeRequestCount;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
            Object.DestroyImmediate(_onDataChanged);
            Object.DestroyImmediate(_artworkCount);
            Object.DestroyImmediate(_activeRequestCount);
        }

        // ---- Helper Methods ----

        private ArtworkData CreateTestArtwork(string id, bool isLiked = false)
        {
            return new ArtworkData(id, "Test " + id, new PixelEntry[0], 32, 32, 1000L, isLiked);
        }

        private RequestData CreateTestRequest(string id, bool isCompleted = false)
        {
            return new RequestData(id, "Test prompt", new ConstraintData[0], isCompleted);
        }

        // ---- Artwork CRUD Tests ----

        [Test]
        public void AddArtwork_ValidArtwork_IncrementsCount()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));

            Assert.AreEqual(1, _dataManager.ArtworkCount);
        }

        [Test]
        public void AddArtwork_ValidArtwork_RaisesOnDataChanged()
        {
            bool wasRaised = false;
            System.Action listener = () => wasRaised = true;
            _onDataChanged.Register(listener);

            _dataManager.AddArtwork(CreateTestArtwork("art-1"));

            Assert.IsTrue(wasRaised);

            _onDataChanged.Unregister(listener);
        }

        [Test]
        public void RemoveArtwork_ExistingId_DecrementsCount()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));

            bool wasRemoved = _dataManager.RemoveArtwork("art-1");

            Assert.IsTrue(wasRemoved);
            Assert.AreEqual(0, _dataManager.ArtworkCount);
        }

        [Test]
        public void RemoveArtwork_NonexistentId_ReturnsFalse()
        {
            bool wasRemoved = _dataManager.RemoveArtwork("nonexistent");

            Assert.IsFalse(wasRemoved);
        }

        [Test]
        public void GetArtwork_ExistingId_ReturnsArtwork()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));

            ArtworkData? result = _dataManager.GetArtwork("art-1");

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual("art-1", result.Value.Id);
        }

        [Test]
        public void GetArtwork_NonexistentId_ReturnsNull()
        {
            ArtworkData? result = _dataManager.GetArtwork("nonexistent");

            Assert.IsFalse(result.HasValue);
        }

        [Test]
        public void GetAllArtworks_AfterAdds_ReturnsAll()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));
            _dataManager.AddArtwork(CreateTestArtwork("art-2"));
            _dataManager.AddArtwork(CreateTestArtwork("art-3"));

            IReadOnlyList<ArtworkData> all = _dataManager.GetAllArtworks();

            Assert.AreEqual(3, all.Count);
        }

        // ---- Request CRUD Tests ----

        [Test]
        public void CompleteRequest_ExistingId_MarksCompleted()
        {
            List<RequestData> requests = new List<RequestData>();
            requests.Add(CreateTestRequest("req-1"));
            _dataManager.SetAllRequests(requests);

            bool wasCompleted = _dataManager.CompleteRequest("req-1");

            Assert.IsTrue(wasCompleted);
            IReadOnlyList<RequestData> all = _dataManager.GetAllRequests();
            Assert.AreEqual(1, all.Count);
            Assert.IsTrue(all[0].IsCompleted);
        }

        [Test]
        public void CompleteRequest_NonexistentId_ReturnsFalse()
        {
            bool wasCompleted = _dataManager.CompleteRequest("nonexistent");

            Assert.IsFalse(wasCompleted);
        }

        [Test]
        public void GetActiveRequests_MixedCompletion_ReturnsOnlyActive()
        {
            List<RequestData> requests = new List<RequestData>();
            requests.Add(CreateTestRequest("req-1"));
            requests.Add(CreateTestRequest("req-2"));
            requests.Add(CreateTestRequest("req-3"));
            _dataManager.SetAllRequests(requests);

            _dataManager.CompleteRequest("req-2");

            IReadOnlyList<RequestData> active = _dataManager.GetActiveRequests();
            Assert.AreEqual(2, active.Count);
        }

        // ---- Like Tests ----

        [Test]
        public void ToggleLike_ExistingArtwork_TogglesIsLiked()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));

            _dataManager.ToggleLike("art-1");
            Assert.IsTrue(_dataManager.HasLiked("art-1"));

            _dataManager.ToggleLike("art-1");
            Assert.IsFalse(_dataManager.HasLiked("art-1"));
        }

        [Test]
        public void ToggleLike_NonexistentId_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("not found"));

            _dataManager.ToggleLike("nonexistent");
        }

        [Test]
        public void HasLiked_LikedArtwork_ReturnsTrue()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1", true));

            Assert.IsTrue(_dataManager.HasLiked("art-1"));
        }

        [Test]
        public void HasLiked_NonexistentId_ReturnsFalse()
        {
            Assert.IsFalse(_dataManager.HasLiked("nonexistent"));
        }

        // ---- Bulk Operation Tests ----

        [Test]
        public void SetAllArtworks_ReplacesExistingData()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));
            _dataManager.AddArtwork(CreateTestArtwork("art-2"));

            List<ArtworkData> newArtworks = new List<ArtworkData>();
            newArtworks.Add(CreateTestArtwork("art-3"));
            newArtworks.Add(CreateTestArtwork("art-4"));
            newArtworks.Add(CreateTestArtwork("art-5"));
            _dataManager.SetAllArtworks(newArtworks);

            Assert.AreEqual(3, _dataManager.ArtworkCount);
        }

        [Test]
        public void SetAllRequests_ReplacesExistingData()
        {
            List<RequestData> initial = new List<RequestData>();
            initial.Add(CreateTestRequest("req-1"));
            initial.Add(CreateTestRequest("req-2"));
            _dataManager.SetAllRequests(initial);

            List<RequestData> replacement = new List<RequestData>();
            replacement.Add(CreateTestRequest("req-3"));
            replacement.Add(CreateTestRequest("req-4"));
            replacement.Add(CreateTestRequest("req-5"));
            _dataManager.SetAllRequests(replacement);

            Assert.AreEqual(3, _dataManager.RequestCount);
        }

        [Test]
        public void SetAllArtworks_Null_ClearsData()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));
            _dataManager.AddArtwork(CreateTestArtwork("art-2"));

            _dataManager.SetAllArtworks(null);

            Assert.AreEqual(0, _dataManager.ArtworkCount);
        }

        // ---- Variable SO Tests ----

        [Test]
        public void AddArtwork_WritesArtworkCountVariable()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));
            Assert.AreEqual(1, _artworkCount.Value);

            _dataManager.AddArtwork(CreateTestArtwork("art-2"));
            Assert.AreEqual(2, _artworkCount.Value);
        }

        [Test]
        public void RemoveArtwork_WritesArtworkCountVariable()
        {
            _dataManager.AddArtwork(CreateTestArtwork("art-1"));
            _dataManager.RemoveArtwork("art-1");

            Assert.AreEqual(0, _artworkCount.Value);
        }

        [Test]
        public void SetAllRequests_WritesActiveRequestCountVariable()
        {
            List<RequestData> requests = new List<RequestData>();
            requests.Add(CreateTestRequest("req-1"));
            requests.Add(CreateTestRequest("req-2"));
            requests.Add(CreateTestRequest("req-3", true));
            _dataManager.SetAllRequests(requests);

            Assert.AreEqual(2, _activeRequestCount.Value);
        }

        [Test]
        public void CompleteRequest_WritesActiveRequestCountVariable()
        {
            List<RequestData> requests = new List<RequestData>();
            requests.Add(CreateTestRequest("req-1"));
            requests.Add(CreateTestRequest("req-2"));
            _dataManager.SetAllRequests(requests);

            _dataManager.CompleteRequest("req-1");

            Assert.AreEqual(1, _activeRequestCount.Value);
        }

        // ---- Cache Tests ----

        [Test]
        public void GetActiveRequests_CalledTwice_ReturnsSameInstance()
        {
            List<RequestData> requests = new List<RequestData>();
            requests.Add(CreateTestRequest("req-1"));
            _dataManager.SetAllRequests(requests);

            IReadOnlyList<RequestData> first = _dataManager.GetActiveRequests();
            IReadOnlyList<RequestData> second = _dataManager.GetActiveRequests();

            Assert.AreSame(first, second);
        }
    }
}
