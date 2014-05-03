using ExifLib;
using ImageInfoTool.App.Model;
using Microsoft.Phone;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImageInfoTool.App.ViewModels
{
    class ImageViewModel
    {
        private Picture _image;
        private ExifData _exifData;

        public ImageViewModel(Picture image)
        {
            _image = image;
            _exifData = new ExifData(image);
        }

        /// <summary>
        /// Lazy load of exif data.
        /// </summary>
        /// <returns></returns>
        public bool LoadExifData()
        {
            return _exifData.Load();
        }

        public ImageSource ThumbnailImage
        {
            get
            {
                return PictureDecoder.DecodeJpeg(_image.GetThumbnail());
            }
        }

        public ImageSource Image
        {
            get
            {
                return PictureDecoder.DecodeJpeg(_image.GetImage());
            }
        }

        public string Name
        {
            get
            {
                return _image.Name;
            }
        }

        public DateTime Date
        {
            get
            {
                return _image.Date;
            }
        }

        public string Album
        {
            get
            {
                return _image.Album.Name;
            }
        }

        /// <summary>
        /// Gets whether the image has exif data. This can only be TRUE when <code>LoadExifData()</code> was called.
        /// </summary>
        public bool HasExifData
        {
            get
            {
                return _exifData.HasData;
            }
        }

        /// <summary>
        /// Gets the EXIF data.
        /// </summary>
        public ExifData ExifData
        {
            get
            {
                return _exifData;
            }
        }
    }
}
