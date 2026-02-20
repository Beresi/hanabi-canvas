// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;

namespace HanabiCanvas.Runtime
{
    /// <summary>
    /// Serializable data representing a saved artwork (pixel drawing with metadata).
    /// </summary>
    [System.Serializable]
    public struct ArtworkData
    {
        // ---- Serialized Fields ----
        [SerializeField] private string _id;
        [SerializeField] private string _name;
        [SerializeField] private PixelEntry[] _pixels;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private long _createdTimestamp;
        [SerializeField] private bool _isLiked;

        // ---- Properties ----

        /// <summary>Unique identifier for this artwork.</summary>
        public string Id => _id;

        /// <summary>Display name of this artwork.</summary>
        public string Name => _name;

        /// <summary>Array of non-empty pixel entries that make up the drawing.</summary>
        public PixelEntry[] Pixels => _pixels;

        /// <summary>Width of the canvas in pixels.</summary>
        public int Width => _width;

        /// <summary>Height of the canvas in pixels.</summary>
        public int Height => _height;

        /// <summary>Unix timestamp (ticks) when this artwork was created.</summary>
        public long CreatedTimestamp => _createdTimestamp;

        /// <summary>Whether the player has liked this artwork.</summary>
        public bool IsLiked => _isLiked;

        // ---- Constructor ----

        /// <summary>
        /// Creates a new <see cref="ArtworkData"/> with all fields specified.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        /// <param name="name">Display name.</param>
        /// <param name="pixels">Array of pixel entries.</param>
        /// <param name="width">Canvas width.</param>
        /// <param name="height">Canvas height.</param>
        /// <param name="createdTimestamp">Creation timestamp.</param>
        /// <param name="isLiked">Whether the artwork is liked.</param>
        public ArtworkData(string id, string name, PixelEntry[] pixels, int width, int height, long createdTimestamp, bool isLiked = false)
        {
            _id = id;
            _name = name;
            _pixels = pixels;
            _width = width;
            _height = height;
            _createdTimestamp = createdTimestamp;
            _isLiked = isLiked;
        }

        // ---- Public Methods ----

        /// <summary>
        /// Returns a new <see cref="ArtworkData"/> with the <see cref="IsLiked"/> state toggled.
        /// All other fields are preserved.
        /// </summary>
        public ArtworkData WithLikeToggled()
        {
            return new ArtworkData(_id, _name, _pixels, _width, _height, _createdTimestamp, !_isLiked);
        }
    }
}
