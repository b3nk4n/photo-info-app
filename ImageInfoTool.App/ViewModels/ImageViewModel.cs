using ImageInfoTool.App.GeoLocation;
using ImageInfoTool.App.Model;
using ImageInfoTool.App.Resources;
using Microsoft.Phone;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.OS;
using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using Windows.System;

namespace ImageInfoTool.App.ViewModels
{
    public class ImageViewModel
    {
        private Picture _image;
        private ExifData _exifData;

        ICommand _openInGeoPhotoCommand;

        private static int instanceCounter = 0;

        public int InstanceId { get; private set; }

        public ImageViewModel(Picture image)
        {
            InstanceId = instanceCounter++;
            _image = image;
            _exifData = new ExifData(image);

            _openInGeoPhotoCommand = new DelegateCommand(async () =>
            {
                string imagePath = ImagePath;
                await Launcher.LaunchUriAsync(new Uri("geophoto:ShowPicturePosition?PicturePath=" + imagePath, UriKind.Absolute));
            });
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

        public string ImagePath
        {
            get
            {
                return _image.GetPath();
            }
        }

        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(ImagePath);
            }
        }

        public bool HasName
        {
            get
            {
                return !AppSettings.HideFileName.Value;
            }
        }

        public string FileType
        {
            get
            {
                return Path.GetExtension(_image.GetPath()).Replace(".", string.Empty).ToUpper();
            }
        }

        public string CreationDate
        {
            get
            {
                return string.Format("{0:D}", _image.Date);
            }
        }

        public string CreationTime
        {
            get
            {
                return string.Format("{0:T}", _image.Date);
            }
        }

        public string Album
        {
            get
            {
                return _image.Album.Name;
            }
        }

        public int Heigth
        {
            get
            {
                return _image.Height;
            }
        }

        public int Width
        {
            get
            {
                return _image.Width;
            }
        }

        public string Resolution
        {
            get
            {
                return string.Format("{0} x {1}", _image.Width, _image.Height);
            }
        }

        public string Orientation
        {
            get
            {
                if (_image.Height > _image.Width)
                    return AppResources.OrientationPortrait;
                else if (_image.Height < _image.Width)
                    return AppResources.OrientationLandscape;
                else
                    return AppResources.OrientationSquare;
            }
        }

        public string FileSize
        {
            get
            {
                var bytes = _image.GetImage().Length;
                var megaBytes = bytes / (1024.0 * 1024.0);
                return string.Format("{0:0.00} MB", megaBytes);
            }
        }

        public string ExifVersion
        {
            get
            {
                if (_exifData == null || !_exifData.HasExifVersion)
                    return AppConstants.PLACEHOLDER_STRING;

                int major = (_exifData.ExifVersion[0] - '0') * 10 + _exifData.ExifVersion[1] - '0';
                int minor = (_exifData.ExifVersion[2] - '0') * 10 + _exifData.ExifVersion[3] - '0';
                return string.Format("{0}.{1}", major, minor); 
            }
        }

        public string DigitalZoom
        {
            get
            {
                if (_exifData == null || !_exifData.HasDigitalZoom)
                    return AppConstants.PLACEHOLDER_STRING;

                return string.Format("{0}x", _exifData.DigitalZoom);
            }
        }

        public string ExposureTime
        {
            get
            {
                if (_exifData == null || !_exifData.HasExposureTime)
                    return AppConstants.PLACEHOLDER_STRING;

                var time = _exifData.ExposureTime;
                if (time == 0)
                    return "0";
                if (time <= 0.5)
                {
                    var val = (int)(1 / time);
                    return string.Format("1/{0}", val);
                }
                else if (time < 1)
                {
                    var val = (int)(time * 10);
                    return string.Format("0\"{0}", val);
                }
                else
                {
                    var sec = (int)time;
                    var mic = (int)((time - sec) * 10);

                    return string.Format("{0}\"{1}", sec, mic);
                }
            }
        }

        public string Flash
        {
            get
            {
                if (_exifData == null || !_exifData.HasFlash)
                    return AppConstants.PLACEHOLDER_STRING;

                // FIXME: there are more bits availible:
                // http://www.awaresystems.be/imaging/tiff/tifftags/privateifd/exif/flash.html
                switch(_exifData.Flash)
                {
                    case 0x00:
                        return AppResources.FlashNotFired;
                    case 0x18:
                        return AppResources.FlashNotFiredAuto;
                    case 0x19:
                        return AppResources.FlashFiredAuto;
                    default:
                        return AppResources.FlashFired;
                }
            }
        }

        public string FNumber
        {
            get
            {
                if (_exifData == null || !_exifData.HasFNumber)
                    return AppConstants.PLACEHOLDER_STRING;

                return string.Format("{0:0.0}", _exifData.FNumber);
            }
        }

        public string GPSLatitude
        {
            get
            {
                if (_exifData == null || !_exifData.HasGPSLatitude)
                    return AppConstants.PLACEHOLDER_STRING;

                return GeoLocationHelper.ToDegreeString(_exifData.GPSLatitude);
            }
        }

        public string GPSLongitude
        {
            get
            {
                if (_exifData == null || !_exifData.HasGPSLongitude)
                    return AppConstants.PLACEHOLDER_STRING;

                return GeoLocationHelper.ToDegreeString(_exifData.GPSLongitude);
            }
        }

        public string GPSAltitude
        {
            get
            {
                if (_exifData == null || !_exifData.HasGPSAltitude)
                    return AppConstants.PLACEHOLDER_STRING;

                return string.Format("{0:0.0}m", _exifData.GPSAltitude);
            }
        }

        public string WhiteBalance
        {
            get
            {
                if (_exifData == null || !_exifData.HasWhiteBalance)
                    return AppConstants.PLACEHOLDER_STRING;

                return (_exifData.WhiteBalance == 0) ? AppResources.WhiteBalanceAuto : AppResources.WhiteBalanceManual;
            }
        }

        /// <summary>
        /// Gets whether the image has exif data. This can only be TRUE when <code>LoadExifData()</code> was called.
        /// </summary>
        public bool HasExifData
        {
            get
            {
                return _exifData != null && _exifData.HasData;
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

        public ICommand OpenInGeoPhotoCommand
        {
            get
            {
                return _openInGeoPhotoCommand;
            }
        }


        /// <summary>
        /// Gets the email button image path.
        /// </summary>
        public string AddWithGeoPhotoImagePath
        {
            get
            {
                const string imageName = "add.png";
                if (PhoneThemeHelper.IsLightThemeActive)
                    return AppConstants.THEME_LIGHT_BASEPATH + imageName;
                else
                    return AppConstants.THEME_DARK_BASEPATH + imageName;
            }
        }
    }
}
