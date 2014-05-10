using Microsoft.Xna.Framework.Media;
using PhoneKit.Framework.Core.Collections;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Themeing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageInfoTool.App.ViewModels
{
    class ImageLibraryViewModel : ViewModelBase
    {
        private MediaLibrary _mediaLibrary;

        private const string SCREENSHOTS_ALBUM_NAME = "Screenshots";

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static ImageLibraryViewModel _instance;

        IList<ImageViewModel> _images = new List<ImageViewModel>();

        public ImageLibraryViewModel()
        {
        }

        /// <summary>
        /// Loads all images from the library.
        /// </summary>
        /// <returns>The async task.</returns>
        public async Task LoadAll()
        {
            _images.Clear();
            List<ImageViewModel> tempList = new List<ImageViewModel>();
            var totalCount = MediaLibrary.Pictures.Count;

            bool hideScreenshots = AppSettings.HideScreenshotsAlbum.Value;

            await Task.Run(() =>
            {
                int i = 0;
                foreach (var picture in MediaLibrary.Pictures)
                {
                    // skip screenshots
                    if (hideScreenshots && picture.Album.Name == SCREENSHOTS_ALBUM_NAME)
                    {
                        ++i;
                        continue;
                    }

                    tempList.Add(new ImageViewModel(i++, picture));
                }
            });

            // add all afterwards to get no cross-thread exception
            foreach (var tempImage in tempList)
            {
                _images.Add(tempImage);
            }

            NotifyPropertyChanged("Images");
            NotifyPropertyChanged("GroupedImages");
        }

        /// <summary>
        /// Gets whether there are more images to load.
        /// </summary>
        public bool HasLoadedAllImages
        {
            get
            {
                return _images.Count >= GetFilteredImageCount();
            }
        }

        /// <summary>
        /// Gets the number of visible images
        /// </summary>
        /// <returns></returns>
        public int GetFilteredImageCount()
        {
            if (AppSettings.HideScreenshotsAlbum.Value)
                return MediaLibrary.Pictures.Count(p => p.Album.Name != SCREENSHOTS_ALBUM_NAME);
            else
                return MediaLibrary.Pictures.Count;
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

            return new ImageViewModel(0 /* id is not from interest here */, photo);
        }

        /// <summary>
        /// Gets a random "good" image from the library.
        /// </summary>
        /// <returns>The image view model.</returns>
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

                if (photo.Width > 480 && photo.Height > 800 &&
                    photo.Album.Name != "WhatsApp" &&
                    photo.Album.Name != SCREENSHOTS_ALBUM_NAME && 
                    photo.Album.Name != "Pictures")
                    break;

                photo = null;
            } while (retryCounter > 0);

            if (photo == null)
                return null;

            return new ImageViewModel(index, photo);
        }

        /// <summary>
        /// Gets an image by the given library index.
        /// </summary>
        /// <param name="index">The library index.</param>
        /// <returns>The image view model.</returns>
        public ImageViewModel GetByLibIndex(int index)
        {
            if (index >= MediaLibrary.Pictures.Count)
                return null;

            return new ImageViewModel(index, MediaLibrary.Pictures[index]);
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

        /// <summary>
        /// Gets the grouped images
        /// </summary>
        public IList<KeyedList<string, ImageViewModel>> GroupedImages
        {
            get
            {
                var groupedImages = from image in _images
                                    orderby image.CreationDateTime
                                    group image by image.CreationDateTime.ToString("y") into imagesByMonth
                                    select new KeyedList<string, ImageViewModel>(imagesByMonth);
                
                var groupList = new List<KeyedList<string, ImageViewModel>>(groupedImages);

                // add empty groupe to allow scrolling to the asolute bottom
                groupList.Add(new KeyedList<string, ImageViewModel>("", new List<ImageViewModel>()));

                return groupList;
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

        /// <summary>
        /// Gets the show image path.
        /// </summary>
        public string LogoImagePath
        {
            get
            {
                const string imageName = "logo200.png";
                if (PhoneThemeHelper.IsLightThemeActive)
                    return AppConstants.THEME_LIGHT_BASEPATH + imageName;
                else
                    return AppConstants.THEME_DARK_BASEPATH + imageName;
            }
        }
    }
}
