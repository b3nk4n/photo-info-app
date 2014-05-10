using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Controls;

namespace ImageInfoTool.App.Pages
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ToggleHideFileName.IsChecked = AppSettings.HideFileName.Value;
            ToggleHideScreenshotAlbum.IsChecked = AppSettings.HideScreenshotsAlbum.Value;
            SelectByTag(PickerMapType, AppSettings.MapType.Value.ToString());
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (ToggleHideFileName.IsChecked.HasValue)
                AppSettings.HideFileName.Value = ToggleHideFileName.IsChecked.Value;

            if (ToggleHideScreenshotAlbum.IsChecked.HasValue)
                AppSettings.HideScreenshotsAlbum.Value = ToggleHideScreenshotAlbum.IsChecked.Value;

            AppSettings.MapType.Value = (MapCartographicMode)Enum.Parse(typeof(MapCartographicMode), (string)(PickerMapType.SelectedItem as ListPickerItem).Tag);
        }

        /// <summary>
        /// Selects a item value by tag value.
        /// </summary>
        /// <param name="picker">The list picker.</param>
        /// <param name="tagToSelect">The tag value of the item to select.</param>
        private void SelectByTag(ListPicker picker, string tagToSelect)
        {
            if (picker == null || tagToSelect == null)
                return;

            foreach (var item in picker.Items)
            {
                var pickerItem = item as ListPickerItem;

                if (pickerItem != null)
                {
                    if ((string)pickerItem.Tag == tagToSelect)
                    {
                        picker.SelectedItem = pickerItem;
                        break;
                    }
                }
            }
        }
    }
}