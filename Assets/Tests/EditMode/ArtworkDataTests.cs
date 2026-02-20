// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class ArtworkDataTests
    {
        // ---- Tests ----
        [Test]
        public void Constructor_AllFields_SetsProperties()
        {
            PixelEntry[] pixels = new PixelEntry[]
            {
                new PixelEntry(0, 0, new Color32(255, 0, 0, 255)),
                new PixelEntry(1, 1, new Color32(0, 255, 0, 255))
            };

            ArtworkData artwork = new ArtworkData("art-001", "Test Art", pixels, 32, 32, 1234567890L, true);

            Assert.AreEqual("art-001", artwork.Id);
            Assert.AreEqual("Test Art", artwork.Name);
            Assert.AreEqual(pixels, artwork.Pixels);
            Assert.AreEqual(32, artwork.Width);
            Assert.AreEqual(32, artwork.Height);
            Assert.AreEqual(1234567890L, artwork.CreatedTimestamp);
            Assert.IsTrue(artwork.IsLiked);
        }

        [Test]
        public void WithLikeToggled_NotLiked_ReturnsLiked()
        {
            ArtworkData artwork = new ArtworkData("art-001", "Test", new PixelEntry[0], 32, 32, 0L, false);

            ArtworkData toggled = artwork.WithLikeToggled();

            Assert.IsTrue(toggled.IsLiked);
        }

        [Test]
        public void WithLikeToggled_Liked_ReturnsNotLiked()
        {
            ArtworkData artwork = new ArtworkData("art-001", "Test", new PixelEntry[0], 32, 32, 0L, true);

            ArtworkData toggled = artwork.WithLikeToggled();

            Assert.IsFalse(toggled.IsLiked);
        }

        [Test]
        public void WithLikeToggled_PreservesOtherFields()
        {
            PixelEntry[] pixels = new PixelEntry[]
            {
                new PixelEntry(5, 10, new Color32(128, 128, 128, 255))
            };
            ArtworkData artwork = new ArtworkData("art-002", "My Drawing", pixels, 32, 32, 9999L, false);

            ArtworkData toggled = artwork.WithLikeToggled();

            Assert.AreEqual("art-002", toggled.Id);
            Assert.AreEqual("My Drawing", toggled.Name);
            Assert.AreEqual(pixels, toggled.Pixels);
            Assert.AreEqual(32, toggled.Width);
            Assert.AreEqual(32, toggled.Height);
            Assert.AreEqual(9999L, toggled.CreatedTimestamp);
        }

        [Test]
        public void Pixels_NullArray_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                ArtworkData artwork = new ArtworkData("art-003", "Empty", null, 32, 32, 0L);
                _ = artwork.Pixels;
            });
        }
    }
}
