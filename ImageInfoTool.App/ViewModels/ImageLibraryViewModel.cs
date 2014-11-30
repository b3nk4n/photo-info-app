using Microsoft.Xna.Framework.Media;
using PhoneKit.Framework.Core.Collections;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Themeing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ImageInfoTool.App.ViewModels
{
    class ImageLibraryViewModel : ViewModelBase
    {
        private MediaLibrary _mediaLibrary;

        private const string SCREENSHOTS_ALBUM_NAME = "Screenshots";

        private int _filterProcessCounter = 0;
        private int _imagesToLoad = 0;

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
        public async Task LoadAllAsync(Control c, bool filtered)
        {
            _images.Clear();
            List<ImageViewModel> tempList = new List<ImageViewModel>();
            var totalCount = MediaLibrary.Pictures.Count;

            bool hideScreenshots = AppSettings.HideScreenshotsAlbum.Value;

            ImagesToLoad = totalCount;

            int steps = Math.Max(1, totalCount / 50);

            FilterProcessCounter = 0;

            await Task.Run(() =>
            {
                int j = 0;
                int i = 0;
                foreach (var picture in MediaLibrary.Pictures)
                {
                    j++;

                    if (j % steps == 0)
                    {
                        c.Dispatcher.BeginInvoke(() =>
                        {
                            FilterProcessCounter = j;
                        });
                    }

                    // skip screenshots
                    if (hideScreenshots && picture.Album.Name == SCREENSHOTS_ALBUM_NAME)
                    {
                        ++i;
                        continue;
                    }

                    var imageViewModel = new ImageViewModel(i++, picture);

                    if (filtered)
                    {
                        if (imageViewModel.CheckGPSPreLoading())
                        {
                            tempList.Add(imageViewModel);
                        }
                    }
                    else
                    {
                        tempList.Add(imageViewModel);
                    }      
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

        public bool HasLoadedImages
        {
            get
            {
                return _images.Count > 0;
            }
        }

        public int ImagesToLoad
        {
            get
            {
                return _imagesToLoad;
            }
            set
            {
                if (_imagesToLoad != value)
                {
                    _imagesToLoad = value;
                    NotifyPropertyChanged("ImagesToLoad");
                }
            }
        }

        public int FilterProcessCounter
        {
            get
            {
                return _filterProcessCounter;
            }
            set
            {
                if (_filterProcessCounter != value)
                {
                    _filterProcessCounter = value;
                    NotifyPropertyChanged("FilterProcessCounter");
                }
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
            Picture photo = null;

            // BUGSENSE: "An unexpected error has occured."
            // 13.06.2014
            // 1 time in version 1.3.1
            try
            {
                photo = MediaLibrary.GetPictureFromToken(token);
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine("Could not retrieve photo from library with error: " + ioex.Message);
            }

            if (photo == null)
                return null;

            return new ImageViewModel(0 /* id is not from interest here */, photo);
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
