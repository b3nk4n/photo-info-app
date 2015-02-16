using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInfoTool.App.GeoLocation
{
    public static class GeoLocationHelper
    {
        public const double NO_COORDINATE = -9999;

        /// <summary>
        /// Converts a exif geo data to a double value.
        /// </summary>
        /// <param name="geoData">The geo data.</param>
        /// <param name="posRef">The positin reference.</param>
        /// <returns>The numeric position.</returns>
        public static double ToDouble(double[] geoData, string posRef)
        {
            if (geoData == null || geoData.Length != 3)
                return NO_COORDINATE;

            int factor = (posRef == "S" || posRef == "W") ? -1 : 1;

            return (geoData[0] + geoData[1] / 60.0 + geoData[2] / 3600.0) * factor;
        }

        /// <summary>
        /// Converts a exif geo data to a double value.
        /// </summary>
        /// <param name="geoData">The geo data.</param>
        /// <param name="posRef">The positin reference.</param>
        /// <returns>The position text.</returns>
        public static string ToDegreeString(double[] geoData, string posRef)
        {
            if (geoData == null || geoData.Length != 3)
                return AppConstants.PLACEHOLDER_STRING;

            return string.Format(CultureInfo.InvariantCulture, "{0}°{1}\'{2}\"{3}", geoData[0], geoData[1], geoData[2], posRef);
        }
    }
}
