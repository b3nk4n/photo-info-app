using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

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
            string tempUri = uri.ToString();
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

            // Otherwise perform normal launch.
            return uri;
        }
    }
}
