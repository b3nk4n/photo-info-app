using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ImageInfoTool.App.ViewModels;
using System.Device.Location;
using ImageInfoTool.App.GeoLocation;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Phone.Maps.Controls;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using ImageInfoTool.App.Resources;

namespace ImageInfoTool.App.Pages
{
    public partial class ImageInfoPage : PhoneApplicationPage
    {
        public ImageInfoPage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var vm = DataContext as ImageViewModel;
                
                if (vm != null)
                {
                    MapControl.Visibility = System.Windows.Visibility.Visible;

                    var exif = vm.ExifData;
                    if (vm.HasExifData && exif.HasGPSLatitude && exif.HasGPSLongitude)
                    {
                        var lat = GeoLocationHelper.ToDouble(exif.GPSLatitude);
                        var lng = GeoLocationHelper.ToDouble(exif.GPSLongitude);
                        MapControl.ZoomLevel = 11;
                        MapControl.CartographicMode = AppSettings.MapType.Value;
                        var photoPosition = new GeoCoordinate(lat, lng);
                        var centerPosition = new GeoCoordinate(photoPosition.Latitude + 0.0175, photoPosition.Longitude);
                        MapControl.Center = centerPosition;
                        UpdateOverlayAtCenter(photoPosition);
                        ShowMapAnimation.Begin();
                    }
                }

                ShowImageAnimation.Begin();
                ImageInfoSlideIn.Begin();
            };

            MapControl.Loaded += (s, e) =>
            {
                Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "ac39aa30-c9b1-4dc6-af2d-1cc17d9807cc";
                Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "QTKCtAtxfOx_XsQs4Ox1Rg";
            };
        }

        private void UpdateImageTranslation(ImageViewModel image)
        {
            if (image != null)
            {
                // transform
                var transform = (CompositeTransform)ImageControl.RenderTransform;

                double imageRatio = (double)image.Heigth / image.Width;

                if (imageRatio != 1)
                {
                    // portrait
                    if (imageRatio > 1)
                    {
                        transform.TranslateY = ((imageRatio - 1) * ImageControl.Height) / -2;
                    }
                    // landscape
                    else
                    {
                        transform.TranslateX = ((imageRatio - 1) * ImageControl.Width) / 2;
                    }
                }
            }
        }

        private void UpdateOverlayAtCenter(GeoCoordinate center)
        {
            MapControl.Layers.Clear();
            
            var overlay = new MapOverlay();
            overlay.Content = GetMarker();
            overlay.GeoCoordinate = center;
            overlay.PositionOrigin = new Point(0.5, 0.5);
            var layer = new MapLayer();
            layer.Add(overlay);
            MapControl.Layers.Add(layer);
        }

        private UIElement GetMarker()
        {
            var innerCircle = new Ellipse
            {
                Fill = new SolidColorBrush((Color)App.Current.Resources["PhoneAccentColor"]),
                Height = 14,
                Width = 14,
                Opacity = 0.8f
            };
            var outerCircle = new Ellipse
            {
                Stroke = new SolidColorBrush((Color)App.Current.Resources["PhoneForegroundColor"]),
                StrokeThickness = 3,
                Height = 28,
                Width = 28,
                Opacity = 0.66f
            };

            var container = new Grid
            {
                Height = 28,
                Width = 28
            };
            container.Children.Add(innerCircle);
            container.Children.Add(outerCircle);
            return container;
        }

        /// <summary>
        /// When the page is navigated to, make a query string lookup and load the image and its information.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // hide map
            MapControl.Opacity = 0.0f;

            // query string lookup
            if (NavigationContext.QueryString != null)
            {
                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_MEDIA_LIB_INDEX))
                {
                    var totalImages = ImageLibraryViewModel.Instance.Images.Count;
                    if (totalImages > 0)
                    {
                        var indexString = NavigationContext.QueryString[AppConstants.PARAM_MEDIA_LIB_INDEX];

                        int index;
                        if (int.TryParse(indexString, out index))
                        {
                            // ensure not out of range
                            index = Math.Min(index, totalImages - 1);

                            var vm = ImageLibraryViewModel.Instance.Images[index];
                            UpdateImageTranslation(vm);
                            vm.LoadExifData();
                            DataContext = vm;
                            return;
                        }

                    }
                }
                else if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_INSTANCE_ID))
                {
                    var totalImages = ImageLibraryViewModel.Instance.Images.Count;
                    if (totalImages > 0)
                    {
                        var idString = NavigationContext.QueryString[AppConstants.PARAM_INSTANCE_ID];

                        int id;
                        if (int.TryParse(idString, out id))
                        {
                            var vm = ImageLibraryViewModel.Instance.GetByInstanceId(id);

                            if (vm != null)
                            {
                                UpdateImageTranslation(vm);
                                vm.LoadExifData();
                                DataContext = vm;
                                return;
                            }
                            
                        }

                    }
                }

                else if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_FILE_TOKEN))
                {
                    var token = NavigationContext.QueryString[AppConstants.PARAM_FILE_TOKEN];

                    var vm = ImageLibraryViewModel.Instance.GetFromToken(token);
                    if (vm != null)
                    {
                        UpdateImageTranslation(vm);
                        vm.LoadExifData();
                        DataContext = vm;
                        return;
                    }
                }

                // error handling - warning and go back or exit
                MessageBox.Show(AppResources.MessageBoxNoImageFound, AppResources.MessageBoxWarning, MessageBoxButton.OK);
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    App.Current.Terminate();
            }
        }
    }
}