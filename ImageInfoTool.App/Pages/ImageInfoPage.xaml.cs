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
using PhoneKit.Framework.Advertising;
using PhoneKit.Framework.InAppPurchase;
using PhoneKit.Framework.Core.Collections;
using Microsoft.Phone.Tasks;
using ImageInfoTool.App.Helpers;

namespace ImageInfoTool.App.Pages
{
    public partial class ImageInfoPage : PhoneApplicationPage
    {
        /// <summary>
        /// Creates a ImageInfoPage instance.
        /// </summary>
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

                ImageInfoSlideIn.Begin();
            };

            MapControl.Loaded += (s, e) =>
            {
                Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "ac39aa30-c9b1-4dc6-af2d-1cc17d9807cc";
                Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "QTKCtAtxfOx_XsQs4Ox1Rg";
            };

            RemoveAdButton.Tap += (s, e) =>
            {
                if (AppSettings.HasReviewed.Value)
                {
                    NavigationService.Navigate(new Uri("/Pages/InAppStorePage.xaml", UriKind.Relative));
                }
                else
                {
                    if (MessageBox.Show(AppResources.MessageBoxRemoveAdContent, AppResources.MessageBoxRemoveAdTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        AppSettings.HasReviewed.Value = true;

                        AppSettings.AddFreeDateDeadline.Value = DateTime.Now.AddDays(AppConstants.AD_FREE_TRIAL_TIME_IN_DAYS);

                        var reviewTask = new MarketplaceReviewTask();
                        reviewTask.Show();
                    }
                }
            };

            InitializeBanner();

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

        /// <summary>
        /// Initializes the banner.
        /// </summary>
        private void InitializeBanner()
        {
            if (PurchaseHelper.IsFreeTrialOrProVersion())
                return;

            if (BannerControl.AdvertsCount > 0)
                return;

            List<AdvertData> advertsList = new List<AdvertData>();
            advertsList.Add(new AdvertData(new Uri("/Assets/Banners/pocketBRAIN_adduplex.png", UriKind.Relative), AdvertData.ActionTypes.AppId, "ad1227e4-9f80-4967-957f-6db140dc0c90"));
            advertsList.Add(new AdvertData(new Uri("/Assets/Banners/SpaceScribble_adduplex.png", UriKind.Relative), AdvertData.ActionTypes.AppId, "71fc4a5b-de12-4b28-88ec-8ac573ce9708"));
            advertsList.Add(new AdvertData(new Uri("/Assets/Banners/SpacepiXX_adduplex.png", UriKind.Relative), AdvertData.ActionTypes.AppId, "cbe0dfa7-2879-4c2c-b7c6-3798781fba16"));
            advertsList.Add(new AdvertData(new Uri("/Assets/Banners/ScribbleHunter_adduplex.png", UriKind.Relative), AdvertData.ActionTypes.AppId, "ed250596-e670-4d22-aee1-8ed0a08c411f"));
            advertsList.Add(new AdvertData(new Uri("/Assets/Banners/GeoPhoto_adduplex.png", UriKind.Relative), AdvertData.ActionTypes.AppId, "f10991b2-3e1a-4fb0-99bc-833338a33502"));

            //shuffle
            advertsList.ShuffleList();

            foreach (var advert in advertsList)
            {
                BannerControl.AddAdvert(advert);
            }
        }

        /// <summary>
        /// Updates the banner visiblilty.
        /// </summary>
        private void UpdateBannerVisibility()
        {
            if (PurchaseHelper.IsFreeTrialOrProVersion())
            {
                BannerControl.Visibility = System.Windows.Visibility.Collapsed;
                BannerContainer.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                BannerControl.Visibility = System.Windows.Visibility.Visible;
                BannerContainer.Visibility = System.Windows.Visibility.Visible;
                BannerControl.Start();
                ShowBannerAnimation.Begin();
            }
        }

        /// <summary>
        /// Updates the image translation.
        /// </summary>
        /// <param name="image">The image.</param>
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

        /// <summary>
        /// Updates the marker to the POI.
        /// </summary>
        /// <param name="center">The center POI.</param>
        private void UpdateOverlayAtCenter(GeoCoordinate center)
        {
            MapControl.Layers.Clear();
            
            var overlay = new MapOverlay();
            overlay.Content = CreateMarker();
            overlay.GeoCoordinate = center;
            overlay.PositionOrigin = new Point(0.5, 0.5);
            var layer = new MapLayer();
            layer.Add(overlay);
            MapControl.Layers.Add(layer);
        }

        /// <summary>
        /// Creates a marker.
        /// </summary>
        /// <returns>The marker.</returns>
        private UIElement CreateMarker()
        {
            var innerCircle = new Ellipse
            {
                Fill = new SolidColorBrush((Color)App.Current.Resources["ThemeColor"]),
                Height = 14,
                Width = 14,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Opacity = 0.9f
            };
            var outerCircle = new Ellipse
            {
                Stroke = new SolidColorBrush((Color)App.Current.Resources["PhoneBackgroundColor"]),
                StrokeThickness = 3,
                Height = 22,
                Width = 22,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Opacity = 0.66f
            };
            var outer2Circle = new Ellipse
            {
                Stroke = new SolidColorBrush((Color)App.Current.Resources["PhoneForegroundColor"]),
                StrokeThickness = 3,
                Height = 30,
                Width = 30,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateBannerVisibility();

            // query string lookup
            if (NavigationContext.QueryString != null)
            {
                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_MEDIA_LIB_INDEX))
                {
                    var indexString = NavigationContext.QueryString[AppConstants.PARAM_MEDIA_LIB_INDEX];

                    int index;
                    if (int.TryParse(indexString, out index))
                    {
                        var vm = ImageLibraryViewModel.Instance.GetByLibIndex(index);
                        UpdateImageTranslation(vm);
                        vm.LoadExifData();
                        DataContext = vm;
                        return;
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