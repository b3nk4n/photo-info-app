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
            MediaLibrary mediaLib = new MediaLibrary();

            foreach (var image in mediaLib.Pictures)
            {
                _images.Add(new ImageViewModel(image));
            }
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

        public ICollection<ImageViewModel> Images
        {
            get
            {
                return _images;
            }
        }
    }
}
