using Microsoft.Xna.Framework.Media;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.OS;
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

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static ImageLibraryViewModel _instance;

        ObservableCollection<ImageViewModel> _images = new ObservableCollection<ImageViewModel>();

        public ImageLibraryViewModel()
        {
        }

        public async Task<int> LoadNext(int count)
        {
            if (!CanLoadNext)
                return 0;

            List<ImageViewModel> tempList = new List<ImageViewModel>();
            var totalCount = MediaLibrary.Pictures.Count;
            var currentCount = _images.Count;
            var start = (totalCount - currentCount) - 1;

            await Task.Run(() =>
            {
                var end = Math.Max(start - count + 1, 0);

                for (int i = start; i >= end; --i)
                {
                    var picture = MediaLibrary.Pictures[i];
                    tempList.Add(new ImageViewModel(i, picture));
                }
            });

            // add all afterwards to get no cross-thread exception
            foreach (var tempImage in tempList)
            {
                _images.Insert(0, tempImage);
            }

            return tempList.Count;
        }

        /// <summary>
        /// Clears all images.
        /// </summary>
        public void Clear()
        {
            _images.Clear();
        }

        /// <summary>
        /// Gets whether there are more images to load.
        /// </summary>
        public bool CanLoadNext
        {
            get
            {
                return _images.Count < MediaLibrary.Pictures.Count;
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

            var index = -1;

            for (int i = 0; i < MediaLibrary.Pictures.Count; ++i)
            {
                if (photo == MediaLibrary.Pictures[i])
                {
                    index = i;
                    break;
                }
            }


            if (photo == null)
                return null;

            return new ImageViewModel(index, photo);
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

                if (photo.Width > 480 && photo.Height > 800 &&
                    photo.Album.Name.ToUpper() != "WHATSAPP" && 
                    photo.Album.Name.ToUpper() != "SCREENSHOTS" && 
                    photo.Album.Name.ToUpper() != "PICTURES")
                    break;

                photo = null;
            } while (retryCounter > 0);

            if (photo == null)
                return null;

            return new ImageViewModel(index, photo);
        }

        /// <summary>
        /// Gets an image by its instance id.
        /// </summary>
        /// <param name="instanceId">The instance id</param>
        /// <returns>The image viwe model or NULL.</returns>
        //public ImageViewModel GetByInstanceId(int instanceId)
        //{
        //    foreach (var image in _images)
        //    {
        //        if (image.InstanceId == instanceId)
        //            return image;
        //    }

        //    // ensure data has loaded (in case of return from tombstone) fallback to ID.
        //    var totalImagesCount = MediaLibrary.Pictures.Count;
        //    if (_images.Count == 0 && instanceId < totalImagesCount) // FIXME this is just a hack. we mix index and instanceId here!
        //    {
        //        return new ImageViewModel(MediaLibrary.Pictures[totalImagesCount - instanceId]);
        //    }

        //    return null;
        //}

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

        public ObservableCollection<ImageViewModel> Images
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
