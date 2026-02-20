// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HanabiCanvas.Runtime.Persistence
{
    /// <summary>
    /// Static utility for JSON serialization/deserialization of game data.
    /// Uses JsonUtility with wrapper structs (Unity cannot serialize top-level arrays).
    /// </summary>
    public static class JsonPersistence
    {
        // ---- Wrapper Types ----

        [System.Serializable]
        private struct ArtworkListWrapper
        {
            public ArtworkData[] artworks;
        }

        [System.Serializable]
        private struct RequestListWrapper
        {
            public RequestData[] requests;
        }

        // ---- Artwork Serialization ----

        /// <summary>Serializes a list of artworks to a JSON string.</summary>
        public static string ExportAllArtworks(IReadOnlyList<ArtworkData> artworks)
        {
            ArtworkData[] array = new ArtworkData[artworks.Count];
            for (int i = 0; i < artworks.Count; i++)
            {
                array[i] = artworks[i];
            }

            ArtworkListWrapper wrapper = new ArtworkListWrapper { artworks = array };
            return JsonUtility.ToJson(wrapper, true);
        }

        /// <summary>
        /// Deserializes artworks from a JSON string.
        /// Returns an empty list if JSON is null, empty, or malformed.
        /// </summary>
        public static List<ArtworkData> ImportArtworks(string json)
        {
            List<ArtworkData> result = new List<ArtworkData>();
            if (string.IsNullOrEmpty(json))
            {
                return result;
            }

            try
            {
                ArtworkListWrapper wrapper = JsonUtility.FromJson<ArtworkListWrapper>(json);
                if (wrapper.artworks != null)
                {
                    for (int i = 0; i < wrapper.artworks.Length; i++)
                    {
                        result.Add(wrapper.artworks[i]);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[JsonPersistence] Failed to import artworks: {e.Message}");
            }

            return result;
        }

        // ---- Request Serialization ----

        /// <summary>Serializes a list of requests to a JSON string.</summary>
        public static string ExportAllRequests(IReadOnlyList<RequestData> requests)
        {
            RequestData[] array = new RequestData[requests.Count];
            for (int i = 0; i < requests.Count; i++)
            {
                array[i] = requests[i];
            }

            RequestListWrapper wrapper = new RequestListWrapper { requests = array };
            return JsonUtility.ToJson(wrapper, true);
        }

        /// <summary>
        /// Deserializes requests from a JSON string.
        /// Returns an empty list if JSON is null, empty, or malformed.
        /// </summary>
        public static List<RequestData> ImportRequests(string json)
        {
            List<RequestData> result = new List<RequestData>();
            if (string.IsNullOrEmpty(json))
            {
                return result;
            }

            try
            {
                RequestListWrapper wrapper = JsonUtility.FromJson<RequestListWrapper>(json);
                if (wrapper.requests != null)
                {
                    for (int i = 0; i < wrapper.requests.Length; i++)
                    {
                        result.Add(wrapper.requests[i]);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[JsonPersistence] Failed to import requests: {e.Message}");
            }

            return result;
        }

        // ---- File I/O Helpers ----

        /// <summary>Saves a JSON string to a file at the given path.</summary>
        public static void SaveToFile(string json, string path)
        {
            try
            {
                System.IO.File.WriteAllText(path, json);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[JsonPersistence] Failed to save file: {e.Message}");
            }
        }

        /// <summary>
        /// Loads a JSON string from a file at the given path.
        /// Returns null if the file does not exist or read fails.
        /// </summary>
        public static string LoadFromFile(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    return null;
                }

                return System.IO.File.ReadAllText(path);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[JsonPersistence] Failed to load file: {e.Message}");
                return null;
            }
        }
    }
}
