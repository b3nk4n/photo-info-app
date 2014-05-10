using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ImageInfoTool.App.Resources;
using PhoneKit.Framework.Support;
using ImageInfoTool.App.ViewModels;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Diagnostics;

namespace ImageInfoTool.App
{
    /// <summary>
    /// The apps initial page.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        private const int BOTTOM_OF_LIST = 9999999;

        private const int INITIAL_LOADED_IMAGES_ = 99999;//40;
        private const int LOADED_IMAGES_PER_INTERVAL = 120;

        private bool _doStartupAnimation;

        /// <summary>
        /// Creates the MainPage instance.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                if (!ImageLibraryViewModel.Instance.HasLoadedAllImages)
                {
                    
                    await ImageLibraryViewModel.Instance.LoadAll();
                    DataContext = ImageLibraryViewModel.Instance;
                    ScrollListToBottom();
                }

                if (_doStartupAnimation)
                {
                    HideSplashScreenAnimation.Begin();
                    ImagesSlideIn.Begin();
                }
            };

            // register startup actions
            StartupActionManager.Instance.Register(5, ActionExecutionRule.Equals, () =>
            {
                if (!AppSettings.HasReviewed.Value)
                    FeedbackManager.Instance.StartFirst();
            });
            StartupActionManager.Instance.Register(10, ActionExecutionRule.Equals, () =>
            {
                if (!AppSettings.HasReviewed.Value)
                    FeedbackManager.Instance.StartSecond();
            }); 

            InitializeBackgroundImage();

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
        /// Initializes the background image.
        /// </summary>
        private void InitializeBackgroundImage()
        {
            var backImage = ImageLibraryViewModel.Instance.GetRandomFromLibrary();

            if (backImage != null)
            {
                BackgroundImage.Source = backImage.Image;
            }
            else
            {
                // use a fallback image
                BackgroundImage.Source = new BitmapImage(new Uri("/Assets/Images/fallback_back.jpg", UriKind.Relative));
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
            StartupActionManager.Instance.Fire();

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

            // refresh
            ApplicationBarIconButton appBarRefreshIconButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/refresh.png", UriKind.Relative));
            appBarRefreshIconButton.Text = AppResources.AppBarRefresh;
            appBarRefreshIconButton.Click += async (s, e) =>
            {
                await ImageLibraryViewModel.Instance.LoadAll();
                ScrollListToBottom();
            };
            ApplicationBar.Buttons.Add(appBarRefreshIconButton);

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
    }
}