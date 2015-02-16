using System;
using System.Collections.Generic;
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
using ImageInfoTool.App.Resources;
using PhoneKit.Framework.Advertising;
using PhoneKit.Framework.Core.Collections;
using PhoneKit.Framework.Storage;
using PhoneKit.Framework.InAppPurchase;
using Microsoft.Phone.Maps.Services;

namespace ImageInfoTool.App.Pages
{
    public partial class ImageInfoPage : PhoneApplicationPage
    {
        private enum InfoPageViewState
        {
            Info,
            Map,
            Image
        }
        
        /// <summary>
        /// The current view state.
        /// </summary>
        private InfoPageViewState _viewState = InfoPageViewState.Info;

        /// <summary>
        /// Indicates whether the map animation has already been played.
        /// </summary>
        private bool _infoMapAnimationPlayed = false;

        /// <summary>
        /// Indicates whether the tutorial popup is visible.
        /// </summary>
        private bool _isTutorialPopupVisible = false;

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

                        if (lat == GeoLocationHelper.NO_COORDINATE || lng == GeoLocationHelper.NO_COORDINATE)
                            return;

                        MapControl.CartographicMode = AppSettings.MapType.Value;
                        var photoPosition = new GeoCoordinate(lat, lng);

                        var latCenter = lat + 0.0275;
                        latCenter = (latCenter > 90) ? 90 : latCenter;

                        var centerPosition = new GeoCoordinate(latCenter, lng);
                        MapControl.Center = centerPosition;
                        UpdateOverlayAtCenter(MapControl, photoPosition);
                        MapControl.Visibility = System.Windows.Visibility.Visible;

                        if (_viewState == InfoPageViewState.Info)
                        {
                            ShowMapAnimation.Begin();
                            _infoMapAnimationPlayed = true;
                        }


                        // init full screen map
                        FullMapControl.CartographicMode = AppSettings.MapType.Value;
                        FullMapControl.Center = photoPosition;
                        UpdateOverlayAtCenter(FullMapControl, photoPosition);
                    }
                }

                // load scroll state
                // note: can not be loaded in the OnNavigatedTo, because the scroller is not populated at that point
                if (PhoneStateHelper.ValueExists(AppConstants.STATE_SCROLL_KEY))
                {
                    var scrollOffset = PhoneStateHelper.LoadValue<double>(AppConstants.STATE_SCROLL_KEY);
                    ImageInfoScroller.ScrollToVerticalOffset(scrollOffset);
                    PhoneStateHelper.DeleteValue(AppConstants.STATE_SCROLL_KEY);
                }

                ImageInfoSlideIn.Begin();

                if (!AppSettings.HasDoneSwipeTutorial.Value)
                {
                    AppSettings.HasDoneSwipeTutorial.Value = true;
                    ShowTutorialAnimation.Begin();
                    _isTutorialPopupVisible = true;
                }
            };

            RemoveAdButton.Tap += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/Pages/InAppStorePage.xaml", UriKind.Relative));
            };

            InfoToMapAnimation.Completed += (s, e) =>
            {
                // show address when animation is over:
                ShowAddressOfLocation();
            };

            InitializeMapApiKey();

            BuildLocalizedApplicationBar();
        }

        private void ShowAddressOfLocation()
        {
            var vm = DataContext as ImageViewModel;
            if (vm == null)
                return;

            if (vm.HasGPS)
            {
                List<MapLocation> locations;
                ReverseGeocodeQuery query = new ReverseGeocodeQuery();
                var exif = vm.ExifData;

                var lat = GeoLocationHelper.ToDouble(exif.GPSLatitude, exif.GPSLatitudeRef);
                var lng = GeoLocationHelper.ToDouble(exif.GPSLongitude, exif.GPSLongitudeRef);
                if (lat == GeoLocationHelper.NO_COORDINATE || lng == GeoLocationHelper.NO_COORDINATE)
                    return;

                query.GeoCoordinate = new GeoCoordinate(
                    lat,
                    lng);
                query.QueryCompleted += (s, e) =>
                {
                    if (e.Error == null && e.Result.Count > 0)
                    {
                        locations = e.Result as List<MapLocation>;
                        var first = locations[0];
                        Dispatcher.BeginInvoke(() =>
                        {
                            // title
                            string txt1 = "";
                            if (first.Information.Address.City != "")
                                txt1 += string.Format("{0}, ", first.Information.Address.City);
                            if (first.Information.Address.StateCode != "")
                                txt1 += string.Format("{0}, ", first.Information.Address.StateCode);
                            if (first.Information.Address.Country != "")
                                txt1 += string.Format("{0}", first.Information.Address.Country);
                            AddressInfoTitle.Text = txt1;

                            // line 2
                            AddressInfoLine2.Visibility = Visibility.Visible;
                            if (first.Information.Address.District != "" && first.Information.Address.Neighborhood != "")
                            {
                                AddressInfoLine2.Text = string.Format("{0}, {1}", first.Information.Address.District, first.Information.Address.Neighborhood);
                            }
                            else if (first.Information.Address.District != "")
                            {
                                AddressInfoLine2.Text = first.Information.Address.District;
                            }
                            else if (first.Information.Address.Neighborhood != "")
                            {
                                AddressInfoLine2.Text = first.Information.Address.Neighborhood;
                            }
                            else
                            {
                                AddressInfoLine2.Visibility = Visibility.Collapsed;
                            }

                            // line 3
                            if (first.Information.Address.Street != "")
                            {
                                AddressInfoLine3.Text = string.Format("{0} {1}", first.Information.Address.Street, first.Information.Address.HouseNumber);
                                AddressInfoLine3.Visibility = Visibility.Visible;
                            } 
                            else
                            {
                                AddressInfoLine3.Visibility = Visibility.Collapsed;
                            } 

                            // line 4
                            if (first.Information.Address.PostalCode != "")
                            {
                                AddressInfoLine4.Text = string.Format("{0} {1}", first.Information.Address.PostalCode, first.Information.Address.City);
                                AddressInfoLine4.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                AddressInfoLine4.Visibility = Visibility.Collapsed;
                            }

                            ShowAddressInfoAnimation.Begin();
                        });
                    }
                };
                query.QueryAsync();
            }
            
        }

        /// <summary>
        /// Initializes the maps API key.
        /// </summary>
        private void InitializeMapApiKey()
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "ac39aa30-c9b1-4dc6-af2d-1cc17d9807cc";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "QTKCtAtxfOx_XsQs4Ox1Rg";
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
        /// Updates the banner visiblilty.
        /// </summary>
        private void UpdateBannerVisibility()
        {
            if (InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
            {
                BannerControl.Visibility = System.Windows.Visibility.Collapsed;
                BannerContainer.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                BannerControl.Visibility = System.Windows.Visibility.Visible;
                BannerContainer.Visibility = System.Windows.Visibility.Visible;
                ShowBannerAnimation.Begin();
            }
        }

        /// <summary>
        /// Updates the image translation.
        /// </summary>
        /// <param name="image">The image.</param>
        private void UpdateImageTranslation(ImageViewModel image)
        {
            if (image != null && image.Width != 0)
            {
                // transform
                var transform = (CompositeTransform)ImageControl.RenderTransform;

                double imageRatio = (double)image.Height / image.Width;

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
        /// <param name="mapControl">The map control.</param>
        /// <param name="center">The center POI.</param>
        private void UpdateOverlayAtCenter(Map mapControl, GeoCoordinate center)
        {
            mapControl.Layers.Clear();
            
            var overlay = new MapOverlay();
            overlay.Content = CreateMarker();
            overlay.GeoCoordinate = center;
            overlay.PositionOrigin = new Point(0.5, 0.5);
            var layer = new MapLayer();
            layer.Add(overlay);
            mapControl.Layers.Add(layer);
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
                Height = 30,
                Width = 30
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
            bool success = false;
            if (NavigationContext.QueryString != null)
            {
                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_MEDIA_LIB_INDEX))
                {
                    var indexString = NavigationContext.QueryString[AppConstants.PARAM_MEDIA_LIB_INDEX];

                    int index;
                    if (int.TryParse(indexString, out index))
                    {
                        var vm = ImageLibraryViewModel.Instance.GetByLibIndex(index);
                        vm.LoadExifData();
                        UpdateImageTranslation(vm);
                        DataContext = vm;
                        success = true;
                    }
                }

                else if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_FILE_TOKEN))
                {
                    var token = NavigationContext.QueryString[AppConstants.PARAM_FILE_TOKEN];

                    var vm = ImageLibraryViewModel.Instance.GetFromToken(token);
                    if (vm != null)
                    {
                        vm.LoadExifData();
                        UpdateImageTranslation(vm);
                        DataContext = vm;
                        success = true;
                    }
                }

                // error handling - warning and go back or exit
                if (!success)
                {
                    MessageBox.Show(AppResources.MessageBoxNoImageFound, AppResources.MessageBoxWarning, MessageBoxButton.OK);
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    else
                        App.Current.Terminate();

                    return;
                }
            }

            // load state
            if (PhoneStateHelper.ValueExists(AppConstants.STATE_INFO_VIEW_STATE))
            {
                var viewState = PhoneStateHelper.LoadValue<InfoPageViewState>(AppConstants.STATE_INFO_VIEW_STATE);
                GoToStartupViewState(viewState);
                PhoneStateHelper.DeleteValue(AppConstants.STATE_INFO_VIEW_STATE);
            }
            if (PhoneStateHelper.ValueExists(AppConstants.STATE_FULL_MAP_ZOOM))
            {
                var zoomLevel = PhoneStateHelper.LoadValue<double>(AppConstants.STATE_FULL_MAP_ZOOM);
                FullMapControl.ZoomLevel = zoomLevel;
                PhoneStateHelper.DeleteValue(AppConstants.STATE_FULL_MAP_ZOOM);
            }
        }

        

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // save state
            if (e.NavigationMode != NavigationMode.Back || e.Uri.OriginalString == "app://external/")
            {
                PhoneStateHelper.SaveValue(AppConstants.STATE_SCROLL_KEY, ImageInfoScroller.VerticalOffset);
                PhoneStateHelper.SaveValue(AppConstants.STATE_INFO_VIEW_STATE, _viewState);
                PhoneStateHelper.SaveValue(AppConstants.STATE_FULL_MAP_ZOOM, FullMapControl.ZoomLevel);
            }
        }

        /// <summary>
        /// Goes directrly to the given view state.
        /// </summary>
        private void GoToStartupViewState(InfoPageViewState viewState)
        {
            _viewState = viewState;

            switch (_viewState)
            {
                case InfoPageViewState.Info:
                    ContentPanel.Visibility = System.Windows.Visibility.Visible;
                    FullMapPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FullImagePanel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case InfoPageViewState.Map:
                    ContentPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FullMapPanel.Visibility = System.Windows.Visibility.Visible;
                    FullImagePanel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case InfoPageViewState.Image:
                    ContentPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FullMapPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FullImagePanel.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Checks and plays unplayed animations.
        /// </summary>
        private void CheckForSwitchStateAnimations()
        {
            if (!_infoMapAnimationPlayed)
            {
                if (_viewState == InfoPageViewState.Info)
                {
                    ShowMapAnimation.Begin();
                    _infoMapAnimationPlayed = true;
                }
            }
        }

        /// <summary>
        /// The event handler for the SKIP tutorial button.
        /// </summary>
        private void SkipTutorialClick(object sender, RoutedEventArgs e)
        {
            SkipTutorial();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (_isTutorialPopupVisible)
            {
                SkipTutorial();
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Skips the swipte tutorial-
        /// </summary>
        private void SkipTutorial()
        {
            HideTutorialAnimation.Begin();
            _isTutorialPopupVisible = false;
        }

        #region Swipe transition

        /// <summary>
        /// The swipe sensitivity, where less is more sensitive.
        /// </summary>
        private const int SWIPE_SENSITIVITY_VALUE = 1500;

        /// <summary>
        /// Represents no scale.
        /// </summary>
        private readonly Point ZERO_SCALE = new Point();

        /// <summary>
        /// The swipe gesture listener event for the content panel.
        /// </summary>
        private void ContentPanelManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (IsRotationAnimationActive)
                return;

            double flickX = e.FinalVelocities.LinearVelocity.X;
            var vm = DataContext as ImageViewModel;
            if (vm == null)
                return;

            // right
            if (flickX > SWIPE_SENSITIVITY_VALUE)
            {
                if (vm.HasExifData && vm.ExifData.HasGPSLatitude && vm.ExifData.HasGPSLongitude)
                {
                    ResetAddressInfoAnimation.Begin();
                    InfoToMapAnimation.Begin();
                    _viewState = InfoPageViewState.Map;
                    CheckForSwitchStateAnimations();
                }

                else
                {
                    InfoToImageSkipMapAnimation.Begin();
                    _viewState = InfoPageViewState.Image;
                    CheckForSwitchStateAnimations();
                }
            }
            // left
            else if (flickX < -SWIPE_SENSITIVITY_VALUE)
            {
                InfoToImageAnimation.Begin();
                _viewState = InfoPageViewState.Image;
                CheckForSwitchStateAnimations();
            }
        }

        /// <summary>
        /// The swipe gesture listener event for the full image panel.
        /// </summary>
        private void FullImagePanelManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (IsRotationAnimationActive)
                return;

            double flickX = e.FinalVelocities.LinearVelocity.X;
            var vm = DataContext as ImageViewModel;
            if (vm == null)
                return;

            // right
            if (flickX > SWIPE_SENSITIVITY_VALUE)
            {
                ImageToInfoAnimation.Begin();
                _viewState = InfoPageViewState.Info;
                CheckForSwitchStateAnimations();
            }
            // left
            else if (flickX < -SWIPE_SENSITIVITY_VALUE)
            {
                if (vm.HasGPS)
                {
                    ResetAddressInfoAnimation.Begin();
                    ImageToMapAnimation.Begin();
                    _viewState = InfoPageViewState.Map;
                    CheckForSwitchStateAnimations();
                }
                else
                {
                    ImageToInfoSkipMapAnimation.Begin();
                    _viewState = InfoPageViewState.Info;
                    CheckForSwitchStateAnimations();
                }
            }
        }

        /// <summary>
        /// The swipe gesture listener event for the full map panel.
        /// </summary>
        private void FullMapPanelManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (IsRotationAnimationActive)
                return;

            // make sure ther was no pinch on the map
            if (e.TotalManipulation.Scale != ZERO_SCALE)
                return;

            double flickX = e.FinalVelocities.LinearVelocity.X;
            double flickY = e.FinalVelocities.LinearVelocity.Y;

            // right
            if (flickX > SWIPE_SENSITIVITY_VALUE)
            {
                MapToImageAnimation.Begin();
                _viewState = InfoPageViewState.Image;
                CheckForSwitchStateAnimations();
            }
            // left
            else if (flickX < -SWIPE_SENSITIVITY_VALUE)
            {
                MapToInfoAnimation.Begin();
                _viewState = InfoPageViewState.Info;
                CheckForSwitchStateAnimations();
            }
        }

        private void FullMapPanelManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            // verify there was a pinch geasture
            if (e.PinchManipulation == null)
                return;

            var scaleFactor = e.PinchManipulation.DeltaScale;

            var newScaleFactor = FullMapControl.ZoomLevel * (scaleFactor + ((1 - scaleFactor) * 0.85));

            newScaleFactor = Math.Max(4, newScaleFactor);
            newScaleFactor = Math.Min(19, newScaleFactor);

            FullMapControl.ZoomLevel = newScaleFactor;
        }

        private bool IsRotationAnimationActive
        {
            get
            {
                return InfoToImageAnimation.GetCurrentState() == System.Windows.Media.Animation.ClockState.Active ||
                    ImageToInfoAnimation.GetCurrentState() == System.Windows.Media.Animation.ClockState.Active ||
                    InfoToMapAnimation.GetCurrentState() == System.Windows.Media.Animation.ClockState.Active ||
                    MapToInfoAnimation.GetCurrentState() == System.Windows.Media.Animation.ClockState.Active ||
                    ImageToMapAnimation.GetCurrentState() == System.Windows.Media.Animation.ClockState.Active ||
                    MapToImageAnimation.GetCurrentState() == System.Windows.Media.Animation.ClockState.Active;

            }
        }

        #endregion

        private void ShowPhotoOnMapClicked(object sender, RoutedEventArgs e)
        {
            TryChangeStateToFullMap();
        }

        private void AddressInfoContainerDoubleTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            HideAddressInfoAnimation.Begin();
        }

        private void ScreenDoubleTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (pos.Y < 350)
            {
                InfoToImageAnimation.Begin();
                _viewState = InfoPageViewState.Image;
                CheckForSwitchStateAnimations();
            } 
            else if (pos.Y > 450)
            {
                TryChangeStateToFullMap();
            }
        }

        private void TryChangeStateToFullMap()
        {
            var vm = DataContext as ImageViewModel;
            if (vm == null)
                return;

            if (vm.HasGPS)
            {
                ResetAddressInfoAnimation.Begin();
                InfoToMapAnimation.Begin();
                _viewState = InfoPageViewState.Map;
                CheckForSwitchStateAnimations();
            }
        }
    }
}