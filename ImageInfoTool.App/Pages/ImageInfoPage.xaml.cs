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

namespace ImageInfoTool.App.Pages
{
    public partial class ImageInfoPage : PhoneApplicationPage
    {
        public ImageInfoPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the page is navigated to, make a query string lookup and load the image and its information.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString != null &&
                NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_MEDIA_LIB_INDEX))
            {
                var indexString = NavigationContext.QueryString[AppConstants.PARAM_MEDIA_LIB_INDEX];

                int index;
                if (int.TryParse(indexString, out index))
                {
                    var vm = ImageLibraryViewModel.Instance.Images[index];
                    vm.LoadExifData();
                    DataContext = vm;
                }
            }
        }
    }
}