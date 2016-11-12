using System;
using System.IO;
using System.Net;
using System.Windows.Navigation;
using Windows.Phone.Storage.SharedAccess;

namespace ImageInfoTool.App
{
    /// <summary>
    /// The custom URI mapper.
    /// </summary>
    public class CustomUriMapper : UriMapperBase
    {
        /// <summary>
        /// Maps the given URI.
        /// </summary>
        /// <param name="uri">The URI to map.</param>
        /// <returns>THe mapped URI.</returns>
        public override Uri MapUri(Uri uri)
        {
            string tempUri = HttpUtility.UrlDecode(uri.ToString());
            string mappedUri;

            // Launch from the photo edit picker.
            // This is only for Windows Phone 8 apps.
            // Incoming URI example: /MainPage.xaml?Action=EditPhotoContent&FileId=%7Bea74a960-3829-4007-8859-cd065654fbbc%7D
            if ((tempUri.Contains("EditPhotoContent")) && (tempUri.Contains("FileId")))
            {
                // Redirect to PhotoEdit.xaml.
                mappedUri = tempUri.Replace("MainPage", "ImageInfoPage");
                mappedUri = mappedUri.Replace("FileId", AppConstants.PARAM_FILE_TOKEN);
                return new Uri(mappedUri, UriKind.Relative);
            }

            // Launch from the rich media "Open in" link.
            // This is only for Windows Phone 8 apps.
            // Incoming URI example: /MainPage.xaml?Action=RichMediaEdit&token=%7Bed8b7de8-6cf9-454e-afe4-abb60ef75160%7D
            if ((tempUri.Contains("RichMediaEdit")) && (tempUri.Contains(AppConstants.PARAM_FILE_TOKEN)))
            {
                // Redirect to RichMediaPage.xaml.
                mappedUri = tempUri.Replace("MainPage", "ImageInfoPage");
                return new Uri(mappedUri, UriKind.Relative);
            }

            // Launch from the photo apps picker.
            // This is for only Windows Phone OS 7.1 apps.
            // Incoming URI example: /MainPage.xaml?token=%7B273fea8d-134c-4764-870d-42224d13eb1a%7D
            if ((tempUri.Contains(AppConstants.PARAM_FILE_TOKEN)) && !(tempUri.Contains("RichMediaEdit")))
            {
                // Redirect to PhotoPage.xaml.
                mappedUri = tempUri.Replace("MainPage", "ImageInfoPage");
                return new Uri(mappedUri, UriKind.Relative);
            }

            // URI association launch for contoso.
            if (tempUri.Contains("/Protocol"))
            {

                int mediaLibIndexPosition = tempUri.IndexOf(string.Format("{0}=", AppConstants.PARAM_MEDIA_LIB_INDEX));
                
                if (mediaLibIndexPosition == -1)
                {
                    return new Uri(string.Format("/Pages/MainPage.xaml"), UriKind.Relative);
                }
                mediaLibIndexPosition += AppConstants.PARAM_MEDIA_LIB_INDEX.Length + 1;
                string mediaLibIndex = tempUri.Substring(mediaLibIndexPosition);

                return new Uri(string.Format("/Pages/ImageInfoPage.xaml?{0}={1}", AppConstants.PARAM_MEDIA_LIB_INDEX, mediaLibIndex), UriKind.Relative);
            }

            // File association launch
            if (tempUri.Contains("/FileTypeAssociation"))
            {
                // Get the file ID (after "fileToken=").
                int fileIDIndex = tempUri.IndexOf("fileToken=") + 10;
                string fileID = tempUri.Substring(fileIDIndex);

                // Get the file name.
                string incomingFileName = SharedStorageAccessManager.GetSharedFileName(fileID);

                // Get the file extension.
                string incomingFileType = Path.GetExtension(incomingFileName);

                // Map the .sdkTest1 and .sdkTest2 files to different pages.
                switch (incomingFileType)
                {
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                        return new Uri(string.Format("/Pages/ImageInfoPage.xaml?{0}={1}", AppConstants.PARAM_FILE_TOKEN, fileID), UriKind.Relative);
                }

                // ensure to go to the main page when the file type association failed
                return new Uri("/Pages/MainPage.xaml", UriKind.Relative);
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }
}
