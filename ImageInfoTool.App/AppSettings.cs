using Microsoft.Phone.Maps.Controls;
using PhoneKit.Framework.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInfoTool.App
{
    public static class AppSettings
    {
        /// <summary>
        /// Setting for whether the file name should be hidden.
        /// </summary>
        public static StoredObject<bool> HideFileName = new StoredObject<bool>("_hideFileName_", true);

        /// <summary>
        /// Setting for whether the file name should be hidden.
        /// </summary>
        public static StoredObject<MapCartographicMode> MapType = new StoredObject<MapCartographicMode>("_mapType_", MapCartographicMode.Road);

        /// <summary>
        /// Indicates whether the user has reviewed the app.
        /// </summary>
        public static StoredObject<bool> HasReviewed = new StoredObject<bool>("_hasReviewed_", false);

        /// <summary>
        /// Indicates the deadline for the removed add when the user has reviewed the app.
        /// </summary>
        public static StoredObject<DateTime> AddFreeDateDeadline = new StoredObject<DateTime>("_hasReviewedAddFreeDeadline_", DateTime.MinValue);
    }
}
