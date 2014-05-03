using System;
using System.Collections.Generic;
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
        /// <param name="geoData"></param>
        /// <returns></returns>
        public static double ToDouble(double[] geoData)
        {
            if (geoData == null || geoData.Length != 3)
                return NO_COORDINATE;

            return geoData[0] + geoData[1] / 60.0 + geoData[2] / 3600.0;
        }

        /// <summary>
        /// Converts a exif geo data to a double value.
        /// </summary>
        /// <param name="geoData"></param>
        /// <returns></returns>
        public static string ToDegreeString(double[] geoData)
        {
            if (geoData == null || geoData.Length != 3)
                return AppConstants.PLACEHOLDER_STRING;

            return string.Format("{0}°{1}\'{2}\"", geoData[0], geoData[1], geoData[2]);
        }
    }
}
