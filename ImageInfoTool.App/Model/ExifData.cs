using ExifLib;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInfoTool.App.Model
{
    /// <summary>
    /// Extracts the EXIF data of an image.
    /// </summary>
    public class ExifData
    {
        private Picture _image;

        public bool IsLoaded { private set; get; }

        public bool HasData { private set; get; }

        public ExifData(Picture image)
        {
            _image = image;
            HasData = false;
            IsLoaded = false;
        }

        public bool Load()
        {
            if (IsLoaded == true)
                return HasData;

            IsLoaded = true;

            try
            {
                using (ExifReader exifReader = new ExifReader(_image.GetImage()))
                {
                    bool resExifVersion = exifReader.GetTagValue<byte[]>(ExifTags.ExifVersion, out _exifVersion);
                    bool resDigitalZoom = exifReader.GetTagValue<double>(ExifTags.DigitalZoomRatio, out _digitalZoom);
                    bool resFlash = exifReader.GetTagValue<ushort>(ExifTags.Flash, out _flash);
                    bool resGPSAltitude = exifReader.GetTagValue<double>(ExifTags.GPSAltitude, out _gpsAltitude);
                    bool resGPSAreaInformation = exifReader.GetTagValue<string>(ExifTags.GPSAreaInformation, out _gpsAreaInformation);
                    bool resGPSLatidude = exifReader.GetTagValue<double[]>(ExifTags.GPSLatitude, out _gpsLatitude); // TODO: ExifTags.GPSLatitude.Select(x=>x[0]+x[1]/60+x[2]/3600)
                    bool resGPSLongitude = exifReader.GetTagValue<double[]>(ExifTags.GPSLongitude, out _gpsLongitude);
                    bool resExposureTime = exifReader.GetTagValue<double>(ExifTags.ExposureTime, out _exposureTime);
                    bool resISOSpeedRatings = exifReader.GetTagValue<ushort>(ExifTags.ISOSpeedRatings, out _isoSpeedRatings);
                    bool resModel = exifReader.GetTagValue<string>(ExifTags.Model, out _model);
                    bool resOridentation = exifReader.GetTagValue<ushort>(ExifTags.Orientation, out _orientation);
                    bool resSharpness = exifReader.GetTagValue<string>(ExifTags.Sharpness, out _sharpness);
                    bool resShutterSpeedValue = exifReader.GetTagValue<double>(ExifTags.ShutterSpeedValue, out _shutterSpeedValue);
                    bool resWhiteBalance = exifReader.GetTagValue<ushort>(ExifTags.WhiteBalance, out _whiteBalance);
                }
            }
            catch (ExifLibException)
            {
                // no exif data
                HasData = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exif extraction error: " + e.Message);
                HasData = false;
            }

            HasData = true;
            return HasData;
        }

        private byte[] _exifVersion;

        public byte[] ExifVersion
        {
            get { return _exifVersion; }
            set { _exifVersion = value; }
        }

        private double _digitalZoom;

        public double DigitalZoom
        {
            get { return _digitalZoom; }
            set { _digitalZoom = value; }
        }

        private ushort _flash;

        public ushort Flash
        {
            get { return _flash; }
            set { _flash = value; }
        }

        private double _gpsAltitude;

        public double GPSAltitude
        {
            get { return _gpsAltitude; }
            set { _gpsAltitude = value; }
        }

        private string _gpsAreaInformation;

        public string GPSAreaInformation
        {
            get { return _gpsAreaInformation; }
            set { _gpsAreaInformation = value; }
        }

        private double[] _gpsLatitude;

        public double[] GPSLatitude
        {
            get { return _gpsLatitude; }
            set { _gpsLatitude = value; }
        }

        private double[] _gpsLongitude;

        public double[] GPSLongitude
        {
            get { return _gpsLongitude; }
            set { _gpsLongitude = value; }
        }

        private double _exposureTime;

        public double ExposureTime
        {
            get { return _exposureTime; }
            set { _exposureTime = value; }
        }

        private ushort _isoSpeedRatings;

        public ushort ISOSpeedRatings
        {
            get { return _isoSpeedRatings; }
            set { _isoSpeedRatings = value; }
        }

        private string _model;

        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private ushort _orientation;

        public ushort Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        private string _sharpness;

        public string Sharpness
        {
            get { return _sharpness; }
            set { _sharpness = value; }
        }

        private double _shutterSpeedValue;

        public double ShutterSpeedValue
        {
            get { return _shutterSpeedValue; }
            set { _shutterSpeedValue = value; }
        }

        private ushort _whiteBalance;

        public ushort WhiteBalance
        {
            get { return _whiteBalance; }
            set { _whiteBalance = value; }
        }
    }
}
