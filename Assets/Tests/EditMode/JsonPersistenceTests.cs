// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Tests.EditMode
{
    public class JsonPersistenceTests
    {
        // ---- Artwork Serialization Tests ----

        [Test]
        public void ExportAllArtworks_EmptyList_ReturnsValidJson()
        {
            List<ArtworkData> artworks = new List<ArtworkData>();

            string json = JsonPersistence.ExportAllArtworks(artworks);
            List<ArtworkData> imported = JsonPersistence.ImportArtworks(json);

            Assert.IsNotNull(json);
            Assert.AreEqual(0, imported.Count);
        }

        [Test]
        public void ExportAllArtworks_WithData_RoundTrips()
        {
            List<ArtworkData> artworks = new List<ArtworkData>();
            artworks.Add(new ArtworkData("test-id-1", "Test Art 1", new PixelEntry[0], 32, 32, 1000L));
            artworks.Add(new ArtworkData("test-id-2", "Test Art 2", new PixelEntry[0], 32, 32, 2000L, true));

            string json = JsonPersistence.ExportAllArtworks(artworks);
            List<ArtworkData> imported = JsonPersistence.ImportArtworks(json);

            Assert.AreEqual(2, imported.Count);
            Assert.AreEqual("test-id-1", imported[0].Id);
            Assert.AreEqual("Test Art 1", imported[0].Name);
            Assert.AreEqual(32, imported[0].Width);
            Assert.AreEqual(32, imported[0].Height);
            Assert.AreEqual(1000L, imported[0].CreatedTimestamp);
            Assert.IsFalse(imported[0].IsLiked);
            Assert.AreEqual("test-id-2", imported[1].Id);
            Assert.AreEqual("Test Art 2", imported[1].Name);
            Assert.AreEqual(2000L, imported[1].CreatedTimestamp);
            Assert.IsTrue(imported[1].IsLiked);
        }

        [Test]
        public void ImportArtworks_NullJson_ReturnsEmptyList()
        {
            List<ArtworkData> result = JsonPersistence.ImportArtworks(null);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ImportArtworks_EmptyJson_ReturnsEmptyList()
        {
            List<ArtworkData> result = JsonPersistence.ImportArtworks("");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ImportArtworks_MalformedJson_ReturnsEmptyList()
        {
            LogAssert.Expect(LogType.Warning, new Regex("Failed to import artworks"));

            List<ArtworkData> result = JsonPersistence.ImportArtworks("not json");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        // ---- Request Serialization Tests ----

        [Test]
        public void ExportAllRequests_EmptyList_ReturnsValidJson()
        {
            List<RequestData> requests = new List<RequestData>();

            string json = JsonPersistence.ExportAllRequests(requests);
            List<RequestData> imported = JsonPersistence.ImportRequests(json);

            Assert.IsNotNull(json);
            Assert.AreEqual(0, imported.Count);
        }

        [Test]
        public void ExportAllRequests_WithData_RoundTrips()
        {
            List<RequestData> requests = new List<RequestData>();
            requests.Add(new RequestData("req-1", "Draw a heart", new ConstraintData[0]));
            requests.Add(new RequestData("req-2", "Draw a star", new ConstraintData[0], true));

            string json = JsonPersistence.ExportAllRequests(requests);
            List<RequestData> imported = JsonPersistence.ImportRequests(json);

            Assert.AreEqual(2, imported.Count);
            Assert.AreEqual("req-1", imported[0].Id);
            Assert.AreEqual("Draw a heart", imported[0].Prompt);
            Assert.IsFalse(imported[0].IsCompleted);
            Assert.AreEqual("req-2", imported[1].Id);
            Assert.AreEqual("Draw a star", imported[1].Prompt);
            Assert.IsTrue(imported[1].IsCompleted);
        }

        [Test]
        public void ImportRequests_NullJson_ReturnsEmptyList()
        {
            List<RequestData> result = JsonPersistence.ImportRequests(null);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ImportRequests_MalformedJson_ReturnsEmptyList()
        {
            LogAssert.Expect(LogType.Warning, new Regex("Failed to import requests"));

            List<RequestData> result = JsonPersistence.ImportRequests("not json");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}
