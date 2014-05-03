using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ImageInfoTool.App.Resources;
using PhoneKit.Framework.Support;
using ImageInfoTool.App.ViewModels;

namespace ImageInfoTool.App
{
    /// <summary>
    /// The apps initial page.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Creates the MainPage instance.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            ImageList.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems.Count == 1)
                {
                    //var image = e.AddedItems[0] as ImageViewModel;
                    var index = ImageList.SelectedIndex;
                    NavigateToImageInfoPage(index);
                }
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

            DataContext = ImageLibraryViewModel.Instance; // TODO: load the data not here, because there is a slow start time! move load().

            BuildLocalizedApplicationBar();
        }

        private void NavigateToImageInfoPage(int index)
        {
            string uriString = string.Format("/Pages/ImageInfoPage.xaml?{0}={1}", AppConstants.PARAM_MEDIA_LIB_INDEX, index);
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
            ApplicationBarMenuItem appBarSettingsMenuItem = new ApplicationBarMenuItem(AppResources.SettingsTitle);
            appBarSettingsMenuItem.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.Relative));
            };
            ApplicationBar.MenuItems.Add(appBarSettingsMenuItem);

            // about
            ApplicationBarMenuItem appBarAboutMenuItem = new ApplicationBarMenuItem(AppResources.AboutTitle);
            appBarAboutMenuItem.Click += (s, e) =>
                {
                    NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
                };
            ApplicationBar.MenuItems.Add(appBarAboutMenuItem);
        }
    }
}