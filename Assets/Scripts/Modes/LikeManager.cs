// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Persistence;

namespace HanabiCanvas.Runtime.Modes
{
    /// <summary>
    /// Thin static utility for artwork like operations.
    /// Delegates to <see cref="DataManager"/> and raises the like event.
    /// </summary>
    public static class LikeManager
    {
        /// <summary>
        /// Toggles the like state of an artwork and raises the like event.
        /// </summary>
        /// <param name="artworkId">The artwork ID to toggle.</param>
        /// <param name="dataManager">The data manager holding artwork data.</param>
        /// <param name="onArtworkLiked">Optional event to raise after toggling.</param>
        public static void ToggleLike(string artworkId, DataManager dataManager, GameEventSO onArtworkLiked = null)
        {
            if (dataManager == null || string.IsNullOrEmpty(artworkId))
            {
                return;
            }

            dataManager.ToggleLike(artworkId);

            if (onArtworkLiked != null)
            {
                onArtworkLiked.Raise();
            }
        }

        /// <summary>
        /// Returns whether the artwork with the given ID is liked.
        /// </summary>
        /// <param name="artworkId">The artwork ID to check.</param>
        /// <param name="dataManager">The data manager holding artwork data.</param>
        /// <returns>True if the artwork is liked, false otherwise.</returns>
        public static bool HasLiked(string artworkId, DataManager dataManager)
        {
            if (dataManager == null || string.IsNullOrEmpty(artworkId))
            {
                return false;
            }

            return dataManager.HasLiked(artworkId);
        }
    }
}
