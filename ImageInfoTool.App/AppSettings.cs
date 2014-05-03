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
    }
}
