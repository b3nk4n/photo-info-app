using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ImageInfoTool.App.Resources;
using PhoneKit.Framework.Support;
using ImageInfoTool.App.ViewModels;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using PhoneKit.Framework.InAppPurchase;

namespace ImageInfoTool.App
{
    /// <summary>
    /// The apps initial page.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        private bool _doStartupAnimation;

        ApplicationBarIconButton _appBarFilterIconButton;

        private bool _isGpsFilterActive = false;

        private bool _isBusy = false;

        /// <summary>
        /// Creates the MainPage instance.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                //if (!ImageLibraryViewModel.Instance.HasLoadedAllImages)
                if (!ImageLibraryViewModel.Instance.HasLoadedImages)
                {

                    if (!_isBusy)
                    {
                        _isBusy = true;
                        await ImageLibraryViewModel.Instance.LoadAllAsync(this, _isGpsFilterActive);
                        DataContext = ImageLibraryViewModel.Instance;
                        ScrollListToBottom();
                        _isBusy = false;
                    }
                }

                if (_doStartupAnimation)
                {
                    HideSplashScreenAnimation.Begin();
                    ImagesSlideIn.Begin();
                }
            };

            // register startup actions
            StartupActionManager.Instance.Register(10, ActionExecutionRule.Equals, () =>
            {
                if (!AppSettings.HasReviewed.Value && !InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
                    FeedbackManager.Instance.StartFirst();
            });
            StartupActionManager.Instance.Register(20, ActionExecutionRule.Equals, () =>
            {
                if (!AppSettings.HasReviewed.Value && !InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
                    FeedbackManager.Instance.StartSecond();
            }); 

            BuildLocalizedApplicationBar();
            
        }

        /// <summary>
        /// Scrolls to the bottom of the page.
        /// </summary>
        private void ScrollListToBottom()
        {
            var index = ImageList.ItemsSource.Count - 1;

            if (index > 0)
            {
                // scroll to the last group
                ImageList.ScrollTo(ImageList.ItemsSource[ImageList.ItemsSource.Count - 1]);
            }
        }

        /// <summary>
        /// Navigates to the image with the given lib index position.
        /// </summary>
        /// <param name="index">The library index.</param>
        private void NavigateToImageInfoPageByLibraryIndex(int index)
        {
            string uriString = string.Format("/Pages/ImageInfoPage.xaml?{0}={1}", AppConstants.PARAM_MEDIA_LIB_INDEX, index);
            NavigationService.Navigate(new Uri(uriString, UriKind.Relative));
        }

        /// <summary>
        /// When the page is navigated to.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // fire startup events
            StartupActionManager.Instance.Fire(e);

            if (e.IsNavigationInitiator)
            {
                _doStartupAnimation = false;
                SplashImage.Visibility = System.Windows.Visibility.Collapsed;
                ImageList.RenderTransform = new TranslateTransform();
            }
            else
            {
                _doStartupAnimation = true;
            }

            // make sure the list is hit-test visible
            ImageList.IsHitTestVisible = true;
        }

        /// <summary>
        /// Builds the localized application bar.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            // ApplicationBar der Seite einer neuen Instanz von ApplicationBar zuweisen
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.99f;

            // refresh
            ApplicationBarIconButton appBarRefreshIconButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/refresh.png", UriKind.Relative));
            appBarRefreshIconButton.Text = AppResources.AppBarRefresh;
            appBarRefreshIconButton.Click += async (s, e) =>
            {
                if (!_isBusy)
                {
                    _isBusy = true;
                    ShowLoadingPopup();
                    await ImageLibraryViewModel.Instance.LoadAllAsync(this, _isGpsFilterActive);
                    ScrollListToBottom();
                    HideLoadingPopup();
                    _isBusy = false;
                }
            };
            ApplicationBar.Buttons.Add(appBarRefreshIconButton);

            // filter GPS images
            _appBarFilterIconButton = new ApplicationBarIconButton();
            ChangeAppBarToNonFilteredState();
            _appBarFilterIconButton.Click += async (s, e) =>
            {
                if (!_isBusy)
                {
                    _isBusy = true;
                    ShowLoadingPopup();

                    if (_isGpsFilterActive)
                    {
                        _isGpsFilterActive = false;
                        await ImageLibraryViewModel.Instance.LoadAllAsync(this, _isGpsFilterActive);
                        ChangeAppBarToNonFilteredState();
                    }
                    else
                    {
                        _isGpsFilterActive = true;
                        await ImageLibraryViewModel.Instance.LoadAllAsync(this, _isGpsFilterActive);
                        ChangeAppBarToFilteredState();
                    }
                    ScrollListToBottom();
                    HideLoadingPopup();
                    _isBusy = false;
                }
            };
            ApplicationBar.Buttons.Add(_appBarFilterIconButton);

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

        private void ChangeAppBarToNonFilteredState()
        {
            _appBarFilterIconButton.IconUri = new Uri("/Assets/AppBar/appbar.map.gps.png", UriKind.Relative);
            _appBarFilterIconButton.Text = AppResources.FilterImagesGPS;
        }

        private void ChangeAppBarToFilteredState()
        {
            _appBarFilterIconButton.IconUri = new Uri("/Assets/AppBar/appbar.image.multiple.png", UriKind.Relative);
            _appBarFilterIconButton.Text = AppResources.FilterImagesAll;
        }

        /// <summary>
        /// Called when an image got selected in the long list.
        /// </summary>
        private void ImageListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = ImageList.SelectedItem as ImageViewModel;

            if (vm == null)
                return;

            NavigateToImageInfoPageByLibraryIndex(vm.LibIndex);
            ImageList.SelectedItem = null;
        }

        private void ShowLoadingPopup()
        {
            ImageList.IsHitTestVisible = false;
            FilteringLoadingComponent.Visibility = Visibility.Visible;
            FilteringAnimation.Begin();
        }

        private void HideLoadingPopup()
        {
            ImageList.IsHitTestVisible = true;
            FilteringLoadingComponent.Visibility = Visibility.Collapsed;
            FilteringAnimation.Stop();
        }
    }
}