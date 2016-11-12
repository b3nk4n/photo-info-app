using Microsoft.Phone.Maps.Controls;
using PhoneKit.Framework.Core.Storage;
using System;

namespace ImageInfoTool.App
{
    public static class AppSettings
    {
        /// <summary>
        /// Setting for whether the file name should be hidden.
        /// </summary>
        public static StoredObject<bool> HideFileName = new StoredObject<bool>("_hideFileName_", false);

        /// <summary>
        /// Setting for whether the file name should be hidden.
        /// </summary>
        public static StoredObject<MapCartographicMode> MapType = new StoredObject<MapCartographicMode>("_mapType_", MapCartographicMode.Aerial);

        /// <summary>
        /// Indicates whether the user has reviewed the app.
        /// </summary>
        public static StoredObject<bool> HasReviewed = new StoredObject<bool>("_hasReviewed_", false);

        /// <summary>
        /// Indicates the deadline for the removed add when the user has reviewed the app.
        /// </summary>
        public static StoredObject<DateTime> AddFreeDateDeadline = new StoredObject<DateTime>("_hasReviewedAddFreeDeadline_", DateTime.MinValue);

        /// <summary>
        /// Setting for whether the screenshots album should be hidden.
        /// </summary>
        public static StoredObject<bool> HideScreenshotsAlbum = new StoredObject<bool>("_hideScreenshots_", true);

        /// <summary>
        /// Settings for whether the user has done the swipe tutorial, to show it only once.
        /// </summary>
        public static StoredObject<bool> HasDoneSwipeTutorial = new StoredObject<bool>("_swipeTutorial_", false);
    }
}
