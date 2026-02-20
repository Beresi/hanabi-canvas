// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Runtime.Persistence
{
    /// <summary>
    /// In-memory data store for artworks and challenge requests.
    /// Provides CRUD operations and raises events on data changes.
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        // ---- Serialized Fields ----

        [Header("Events")]
        [Tooltip("Raised when artwork or request data changes")]
        [SerializeField] private GameEventSO _onDataChanged;

        [Header("Shared Variables")]
        [Tooltip("Written with current artwork count on every mutation")]
        [SerializeField] private IntVariableSO _artworkCount;

        [Tooltip("Written with current active (uncompleted) request count on every mutation")]
        [SerializeField] private IntVariableSO _activeRequestCount;

        [Header("Config")]
        [Tooltip("Challenge config with predefined requests loaded on Awake")]
        [SerializeField] private ChallengeConfigSO _challengeConfig;

        // ---- Private Fields ----
        private readonly List<ArtworkData> _artworks = new List<ArtworkData>();
        private readonly List<RequestData> _requests = new List<RequestData>();
        private readonly List<RequestData> _cachedActiveRequests = new List<RequestData>();
        private bool _isActiveRequestsCacheDirty = true;

        // ---- Properties ----

        /// <summary>Total number of stored artworks.</summary>
        public int ArtworkCount => _artworks.Count;

        /// <summary>Total number of stored requests.</summary>
        public int RequestCount => _requests.Count;

        // ---- Unity Methods ----

        private void Awake()
        {
            LoadPredefinedRequests();
        }

        // ---- Artwork CRUD ----

        /// <summary>Adds an artwork to the in-memory store.</summary>
        public void AddArtwork(ArtworkData artwork)
        {
            _artworks.Add(artwork);
            RaiseDataChanged();
        }

        /// <summary>Removes an artwork by ID. Returns true if found and removed.</summary>
        public bool RemoveArtwork(string id)
        {
            for (int i = 0; i < _artworks.Count; i++)
            {
                if (_artworks[i].Id == id)
                {
                    _artworks.RemoveAt(i);
                    RaiseDataChanged();
                    return true;
                }
            }

            return false;
        }

        /// <summary>Finds an artwork by ID. Returns null if not found.</summary>
        public ArtworkData? GetArtwork(string id)
        {
            for (int i = 0; i < _artworks.Count; i++)
            {
                if (_artworks[i].Id == id)
                {
                    return _artworks[i];
                }
            }

            return null;
        }

        /// <summary>Returns a read-only view of all artworks.</summary>
        public IReadOnlyList<ArtworkData> GetAllArtworks()
        {
            return _artworks;
        }

        /// <summary>Returns the total number of stored artworks.</summary>
        public int GetArtworkCount()
        {
            return _artworks.Count;
        }

        // ---- Request CRUD ----

        /// <summary>Returns a cached list of uncompleted requests. Rebuilt only on data mutation.</summary>
        public IReadOnlyList<RequestData> GetActiveRequests()
        {
            if (_isActiveRequestsCacheDirty)
            {
                RebuildActiveRequestsCache();
            }

            return _cachedActiveRequests;
        }

        /// <summary>Marks a request as completed by ID. Returns true if found.</summary>
        public bool CompleteRequest(string id)
        {
            for (int i = 0; i < _requests.Count; i++)
            {
                if (_requests[i].Id == id)
                {
                    _requests[i] = _requests[i].WithCompleted();
                    RaiseDataChanged();
                    return true;
                }
            }

            return false;
        }

        /// <summary>Returns a read-only view of all requests.</summary>
        public IReadOnlyList<RequestData> GetAllRequests()
        {
            return _requests;
        }

        // ---- Like ----

        /// <summary>Toggles the IsLiked flag on an artwork by ID.</summary>
        public void ToggleLike(string artworkId)
        {
            for (int i = 0; i < _artworks.Count; i++)
            {
                if (_artworks[i].Id == artworkId)
                {
                    _artworks[i] = _artworks[i].WithLikeToggled();
                    RaiseDataChanged();
                    return;
                }
            }

            Debug.LogWarning($"[DataManager] ToggleLike: artwork '{artworkId}' not found.");
        }

        /// <summary>Returns whether the artwork with the given ID is liked.</summary>
        public bool HasLiked(string artworkId)
        {
            for (int i = 0; i < _artworks.Count; i++)
            {
                if (_artworks[i].Id == artworkId)
                {
                    return _artworks[i].IsLiked;
                }
            }

            return false;
        }

        // ---- Bulk Operations ----

        /// <summary>Replaces all artworks. Used by import functionality.</summary>
        public void SetAllArtworks(List<ArtworkData> artworks)
        {
            _artworks.Clear();
            if (artworks != null)
            {
                for (int i = 0; i < artworks.Count; i++)
                {
                    _artworks.Add(artworks[i]);
                }
            }

            RaiseDataChanged();
        }

        /// <summary>Replaces all requests. Used by import functionality.</summary>
        public void SetAllRequests(List<RequestData> requests)
        {
            _requests.Clear();
            if (requests != null)
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    _requests.Add(requests[i]);
                }
            }

            RaiseDataChanged();
        }

        // ---- Private Methods ----

        private void LoadPredefinedRequests()
        {
            if (_challengeConfig == null || _challengeConfig.PredefinedRequests == null)
            {
                return;
            }

            RequestData[] predefined = _challengeConfig.PredefinedRequests;
            for (int i = 0; i < predefined.Length; i++)
            {
                _requests.Add(predefined[i]);
            }

            _isActiveRequestsCacheDirty = true;
            UpdateVariableSOs();
        }

        private void RaiseDataChanged()
        {
            _isActiveRequestsCacheDirty = true;
            UpdateVariableSOs();

            if (_onDataChanged != null)
            {
                _onDataChanged.Raise();
            }
        }

        private void UpdateVariableSOs()
        {
            if (_artworkCount != null)
            {
                _artworkCount.Value = _artworks.Count;
            }

            if (_activeRequestCount != null)
            {
                int count = 0;
                for (int i = 0; i < _requests.Count; i++)
                {
                    if (!_requests[i].IsCompleted)
                    {
                        count++;
                    }
                }

                _activeRequestCount.Value = count;
            }
        }

        private void RebuildActiveRequestsCache()
        {
            _cachedActiveRequests.Clear();
            for (int i = 0; i < _requests.Count; i++)
            {
                if (!_requests[i].IsCompleted)
                {
                    _cachedActiveRequests.Add(_requests[i]);
                }
            }

            _isActiveRequestsCacheDirty = false;
        }
    }
}
