using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInfoTool.App
{
    /// <summary>
    /// Global app constants.
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// The media library navigation key.
        /// </summary>
        public const string PARAM_MEDIA_LIB_INDEX = "mediaLibIndex";

        /// <summary>
        /// The placeholder string.
        /// </summary>
        public const string PARAM_FILE_TOKEN = "token";

        /// <summary>
        /// The placeholder string.
        /// </summary>
        public const string PLACEHOLDER_STRING = "-";

        /// <summary>
        /// The light theme base path.
        /// </summary>
        public const string THEME_LIGHT_BASEPATH = "/Assets/Images/light/";

        /// <summary>
        /// The dark theme base path.
        /// </summary>
        public const string THEME_DARK_BASEPATH = "/Assets/Images/dark/";

        /// <summary>
        /// The in-app key "no adverts".
        /// </summary>
        public const string PRO_VERSION_IN_APP_KEY = "imageInfoPro";

        /// <summary>
        /// The time interval of the ad free version in days.
        /// </summary>
        public const int AD_FREE_TRIAL_TIME_IN_DAYS = 14;
    }
}
