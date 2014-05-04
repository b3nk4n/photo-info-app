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

        private bool _isAllDataLoaded;

        public ImageLibraryViewModel()
        {
            //LoadAll();
            _isAllDataLoaded = false;
        }

        /// <summary>
        /// Loads all images from the library.
        /// </summary>
        public void LoadAll()
        {
            if (_isAllDataLoaded)
                return;

            foreach (var image in MediaLibrary.Pictures)
            {
                _images.Insert(0, new ImageViewModel(image));
            }

            _isAllDataLoaded = true;
        }

        public void InserImage(ImageViewModel image)
        {
            _images.Insert(0, image);
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

        public ImageViewModel GetRandomFromLibrary()
        {
            Picture photo;
            Random rand = new Random();
            var imagesCount = MediaLibrary.Pictures.Count;

            if (imagesCount == 0)
                return null;

            var index = imagesCount / 2;
            int retryCounter = 20;
            do
            {
                // get one of the "new half"
                index = rand.Next(imagesCount / 2, imagesCount - 1);
                photo = MediaLibrary.Pictures[index];

                retryCounter--;

                if (photo.Width > 480 && photo.Height > 800 && photo.Album.Name.ToUpper() != "WHATSAPP" && photo.Album.Name.ToUpper() != "SCREENSHOTS")
                    break;

                photo = null;
            } while (retryCounter > 0);

            if (photo == null)
                return null;

            return new ImageViewModel(photo);
        }

        /// <summary>
        /// Gets an image by its instance id.
        /// </summary>
        /// <param name="instanceId">The instance id</param>
        /// <returns>The image viwe model or NULL.</returns>
        public ImageViewModel GetByInstanceId(int instanceId)
        {
            foreach (var image in _images)
            {
                if (image.InstanceId == instanceId)
                    return image;
            }

            return null;
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
