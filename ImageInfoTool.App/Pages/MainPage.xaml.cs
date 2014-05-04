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

        private const int INITIAL_LOADED_IMAGES_ = 40;
        private const int LOADED_IMAGES_PER_INTERVAL = 120;

        /// <summary>
        /// Creates the MainPage instance.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                // load data
                await ImageLibraryViewModel.Instance.LoadNext(INITIAL_LOADED_IMAGES_);
                await Task.Delay(200);
                ImageScrollViewer.ScrollToVerticalOffset(BOTTOM_OF_LIST);

                InitializeEndlessScrolling();

                HideSplashScreenAnimation.Begin();
                ShowBackgroundImageAnimation.Begin();
                ImagesSlideIn.Begin();
            };

            // register startup actions
            StartupActionManager.Instance.Register(5, ActionExecutionRule.Equals, () =>
            {
                FeedbackManager.Instance.StartFirst();
            });
            StartupActionManager.Instance.Register(10, ActionExecutionRule.Equals, () =>
            {
                FeedbackManager.Instance.StartSecond();
            }); 

            InitializeBackgroundImage();

            BuildLocalizedApplicationBar();
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

        //private void NavigateToImageInfoPageByLibraryIndex(int index)
        //{
        //    string uriString = string.Format("/Pages/ImageInfoPage.xaml?{0}={1}", AppConstants.PARAM_MEDIA_LIB_INDEX, index);
        //    NavigationService.Navigate(new Uri(uriString, UriKind.Relative));
        //}

        private void NavigateToImageInfoPageByInstanceId(int id)
        {
            string uriString = string.Format("/Pages/ImageInfoPage.xaml?{0}={1}", AppConstants.PARAM_INSTANCE_ID, id);
            NavigationService.Navigate(new Uri(uriString, UriKind.Relative));
        }

        /// <summary>
        /// When the page is navigated to.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // fire startup events
            StartupActionManager.Instance.Fire();

            //await ImageLibraryViewModel.Instance.LoadPart(0, 50);
            DataContext = ImageLibraryViewModel.Instance;
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
                ImageLibraryViewModel.Instance.Clear();

                ProgressBar.IsIndeterminate = true;
                ProgressBar.Visibility = System.Windows.Visibility.Visible;
                int loadedCount = await ImageLibraryViewModel.Instance.LoadNext(LOADED_IMAGES_PER_INTERVAL);
                await Task.Delay(500);
                ImageScrollViewer.ScrollToVerticalOffset(Math.Round(loadedCount / 4.0, 0) * (102 + 12));
                ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
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

        private void ImageClicked(object sender, RoutedEventArgs e)
       {
            var button = sender as Button;

            if (button != null)
            {
                var vm = button.DataContext as ImageViewModel;

                if (vm != null)
                {
                    NavigateToImageInfoPageByInstanceId(vm.InstanceId);
                }
            }
        }

        #region ENDLESS SCROLLING

        private ScrollViewer sv = null;
        private bool alreadyHookedScrollEvents = false;

        private void InitializeEndlessScrolling()
        {
            if (alreadyHookedScrollEvents)
                return;

            alreadyHookedScrollEvents = true;
            sv = ImageScrollViewer;
            if (sv != null)
            {
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup group = FindVisualState(element, "ScrollStates");
                    if (group != null)
                    {
                        group.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(group_CurrentStateChanging);
                    }
                    VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");
                    if (vgroup != null)
                    {
                        vgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(vgroup_CurrentStateChanging);
                    }
                }
            }
        }

        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        return element as UIElement;
                    }
                    else
                    {
                        returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType);
                    }
                }
            }
            return returnElement;
        }


        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        private async void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionTop")
            {
                if (!ImageLibraryViewModel.Instance.CanLoadNext)
                    return;

                ProgressBar.IsIndeterminate = true;
                ProgressBar.Visibility = System.Windows.Visibility.Visible;
                int loadedCount = await ImageLibraryViewModel.Instance.LoadNext(LOADED_IMAGES_PER_INTERVAL);
                await Task.Delay(500);
                ImageScrollViewer.ScrollToVerticalOffset(Math.Round(loadedCount / 4.0, 0) * (102 + 12));
                ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
            }

            if (e.NewState.Name == "CompressionBottom")
            {
            }
            if (e.NewState.Name == "NoVerticalCompression")
            {
            }
        }
        private void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "Scrolling")
            {
            }
            else
            {
            }
        }

        #endregion
    }
}