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
                    HasExifVersion = exifReader.GetTagValue<byte[]>(ExifTags.ExifVersion, out _exifVersion);
                    HasDigitalZoom = exifReader.GetTagValue<double>(ExifTags.DigitalZoomRatio, out _digitalZoom);
                    HasFlash = exifReader.GetTagValue<ushort>(ExifTags.Flash, out _flash);
                    HasFNumber = exifReader.GetTagValue<double>(ExifTags.FNumber, out _fNumber);
                    HasGPSAltitude = exifReader.GetTagValue<double>(ExifTags.GPSAltitude, out _gpsAltitude);
                    HasGPSLatitude = exifReader.GetTagValue<double[]>(ExifTags.GPSLatitude, out _gpsLatitude);
                    HasGPSLongitude = exifReader.GetTagValue<double[]>(ExifTags.GPSLongitude, out _gpsLongitude);
                    HasExposureTime = exifReader.GetTagValue<double>(ExifTags.ExposureTime, out _exposureTime);
                    HasISOSpeedRatings = exifReader.GetTagValue<ushort>(ExifTags.ISOSpeedRatings, out _isoSpeedRatings);
                    HasModel = exifReader.GetTagValue<string>(ExifTags.Model, out _model);
                    HasWhiteBalance = exifReader.GetTagValue<ushort>(ExifTags.WhiteBalance, out _whiteBalance);

                    /* black-list - these are tested and not from interest:
                     * ExifTags.Sharpness (no value)
                     * ExifTags.Orientation (we get the orientation from the image resolution)
                     * ExifTags.ShutterSpeedValue (we use exposure time)
                     */
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

            // update data flag
            HasData = HasExifVersion || HasDigitalZoom || HasFlash || 
                HasFNumber || HasGPS || HasExposureTime || HasISOSpeedRatings || 
                HasModel || HasWhiteBalance;

            return HasData;
        }

        private byte[] _exifVersion;

        public byte[] ExifVersion
        {
            get { return _exifVersion; }
            set { _exifVersion = value; }
        }

        public bool HasExifVersion { get; private set; }

        private double _digitalZoom;

        public double DigitalZoom
        {
            get { return _digitalZoom; }
            set { _digitalZoom = value; }
        }

        public bool HasDigitalZoom { get; private set; }

        private ushort _flash;

        public ushort Flash
        {
            get { return _flash; }
            set { _flash = value; }
        }

        public bool HasFlash { get; private set; }

        private double _fNumber;

        public double FNumber
        {
            get { return _fNumber; }
            set { _fNumber = value; }
        }

        public bool HasFNumber { get; private set; }

        private double _gpsAltitude;

        public double GPSAltitude
        {
            get { return _gpsAltitude; }
            set { _gpsAltitude = value; }
        }

        public bool HasGPSAltitude { get; private set; }

        private double[] _gpsLatitude;

        public double[] GPSLatitude
        {
            get { return _gpsLatitude; }
            set { _gpsLatitude = value; }
        }

        public bool HasGPSLatitude { get; private set; }

        private double[] _gpsLongitude;

        public double[] GPSLongitude
        {
            get { return _gpsLongitude; }
            set { _gpsLongitude = value; }
        }

        public bool HasGPSLongitude { get; private set; }

        public bool HasGPS
        {
            get
            {
                return HasGPSAltitude || HasGPSLatitude || HasGPSLongitude;
            }
        }

        private double _exposureTime;

        public double ExposureTime
        {
            get { return _exposureTime; }
            set { _exposureTime = value; }
        }

        public bool HasExposureTime { get; private set; }

        private ushort _isoSpeedRatings;

        public ushort ISOSpeedRatings
        {
            get { return _isoSpeedRatings; }
            set { _isoSpeedRatings = value; }
        }

        public bool HasISOSpeedRatings { get; private set; }

        private string _model;

        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public bool HasModel { get; private set; }

        private ushort _whiteBalance;

        public ushort WhiteBalance
        {
            get { return _whiteBalance; }
            set { _whiteBalance = value; }
        }

        public bool HasWhiteBalance { get; private set; }
    }
}
