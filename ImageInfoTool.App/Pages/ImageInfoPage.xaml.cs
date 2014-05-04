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
                    var exif = vm.ExifData;
                    if (vm.HasExifData && exif.HasGPSLatitude && exif.HasGPSLongitude)
                    {
                        var lat = GeoLocationHelper.ToDouble(exif.GPSLatitude, exif.GPSLatitudeRef);
                        var lng = GeoLocationHelper.ToDouble(exif.GPSLongitude, exif.GPSLongitudeRef);
                        MapControl.ZoomLevel = 11;
                        MapControl.CartographicMode = AppSettings.MapType.Value;
                        var photoPosition = new GeoCoordinate(lat, lng);
                        var centerPosition = new GeoCoordinate(photoPosition.Latitude + 0.0175, photoPosition.Longitude);
                        MapControl.Center = centerPosition;
                        UpdateOverlayAtCenter(photoPosition);
                        MapControl.Visibility = System.Windows.Visibility.Visible;
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

            BuildLocalizedApplicationBar();
        }

        /// <summary>
        /// Builds the localized application bar.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            // ApplicationBar der Seite einer neuen Instanz von ApplicationBar zuweisen
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBar.Opacity = 0.99f;

            // settings
            ApplicationBarIconButton appBarSettingsIconButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/feature.settings.png", UriKind.Relative));
            appBarSettingsIconButton.Text = AppResources.AppBarSettings;
            appBarSettingsIconButton.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.Relative));
            };
            ApplicationBar.Buttons.Add(appBarSettingsIconButton);

            // in-app store
            ApplicationBarMenuItem appBarStoreMenuItem = new ApplicationBarMenuItem(AppResources.InAppStoreTitle);
            ApplicationBar.MenuItems.Add(appBarStoreMenuItem);
            appBarStoreMenuItem.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/Pages/InAppStorePage.xaml", UriKind.Relative));
            };

            // about
            ApplicationBarMenuItem appBarAboutMenuItem = new ApplicationBarMenuItem(AppResources.AboutTitle);
            appBarAboutMenuItem.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
            };
            ApplicationBar.MenuItems.Add(appBarAboutMenuItem);
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
                Fill = new SolidColorBrush((Color)App.Current.Resources["ThemeColor"]),
                Height = 14,
                Width = 14,
                Opacity = 0.9f
            };
            var outerCircle = new Ellipse
            {
                Stroke = new SolidColorBrush((Color)App.Current.Resources["PhoneBackgroundColor"]),
                StrokeThickness = 3,
                Height = 21,
                Width = 21,
                Opacity = 0.66f
            };
            var outer2Circle = new Ellipse
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
            container.Children.Add(outer2Circle);
            return container;
        }

        /// <summary>
        /// When the page is navigated to, make a query string lookup and load the image and its information.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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