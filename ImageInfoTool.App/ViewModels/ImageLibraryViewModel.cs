using Microsoft.Xna.Framework.Media;
using PhoneKit.Framework.Core.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInfoTool.App.ViewModels
{
    class ImageLibraryViewModel : ViewModelBase
    {
        private MediaLibrary _mediaLibrary;

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static ImageLibraryViewModel _instance;

        ObservableCollection<ImageViewModel> _images = new ObservableCollection<ImageViewModel>();

        public ImageLibraryViewModel()
        {
            Load();
        }

        /// <summary>
        /// Loads all images from the library.
        /// </summary>
        private void Load()
        {
            foreach (var image in MediaLibrary.Pictures)
            {
                _images.Add(new ImageViewModel(image));
            }
        }

        /// <summary>
        /// Gets the image from the given library token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ImageViewModel GetFromToken(string token)
        {
            Picture photo = MediaLibrary.GetPictureFromToken(token);

            if (photo == null)
                return null;

            return new ImageViewModel(photo);
        }

        /// <summary>
        /// Gets the image library view model instance.
        /// </summary>
        public static ImageLibraryViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ImageLibraryViewModel();
                return _instance;
            }
        }

        public IList<ImageViewModel> Images
        {
            get
            {
                return _images;
            }
        }

        public MediaLibrary MediaLibrary
        {
            get
            {
                if (_mediaLibrary == null)
                    _mediaLibrary = new MediaLibrary();
                return _mediaLibrary;
            }
        }
    }
}
