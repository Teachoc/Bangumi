using Bangumi.Data;
using Bangumi.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Bangumi.ContentDialogs
{
    public sealed partial class SitesContentDialog : ContentDialog
    {
        public ObservableCollection<SiteMetaWithKey> EnabledSites { get; set; } = new ObservableCollection<SiteMetaWithKey>();
        public ObservableCollection<SiteMetaWithKey> NotEnabledSites { get; set; } = new ObservableCollection<SiteMetaWithKey>();

        public SitesContentDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            BangumiData.GetEnabledSites()
                .Select(it => SiteMetaWithKey.FromSiteMeta(it)).ToList()
                .ForEach(it => EnabledSites.Add(it));
            BangumiData.GetDisabledSites()
                .Select(it => SiteMetaWithKey.FromSiteMeta(it)).ToList()
                .ForEach(it => NotEnabledSites.Add(it));
        }

        private void ListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count == 1)
            {
                e.Data.SetText(Newtonsoft.Json.JsonConvert.SerializeObject(e.Items[0]));
                e.Data.RequestedOperation = DataPackageOperation.Move;
            }
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private async void ListView_Drop(object sender, DragEventArgs e)
        {
            ListView target = (ListView)sender;

            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                DragOperationDeferral def = e.GetDeferral();
                string s = await e.DataView.GetTextAsync();
                var site = Newtonsoft.Json.JsonConvert.DeserializeObject<SiteMetaWithKey>(s);

                // Find the insertion index:
                Windows.Foundation.Point pos = e.GetPosition(target.ItemsPanelRoot);

                // Find which ListView is the target, find height of first item
                ListViewItem sampleItem;
                if (target.Name == "EnabledSitesListView")
                {
                    sampleItem = (ListViewItem)NotEnabledSitesListView.ContainerFromIndex(0);
                }
                else
                {
                    sampleItem = (ListViewItem)EnabledSitesListView.ContainerFromIndex(0);
                }

                // Adjust ItemHeight for margins
                double itemHeight = sampleItem.ActualHeight + sampleItem.Margin.Top + sampleItem.Margin.Bottom;

                // Find index based on dividing number of items by height of each item
                int index = Math.Min(target.Items.Count - 1, (int)(pos.Y / itemHeight));

                // Find the item that we want to drop
                ListViewItem targetItem = (ListViewItem)target.ContainerFromIndex(index);

                // Figure out if to insert above or below
                Windows.Foundation.Point positionInItem = e.GetPosition(targetItem);
                if (positionInItem.Y > itemHeight / 2)
                {
                    index++;
                }

                // Don't go out of bounds
                index = Math.Min(target.Items.Count, index);

                // Find correct source list
                if (target.Name == "EnabledSitesListView")
                {
                    EnabledSites.Insert(index, site);
                    foreach (var item in NotEnabledSites)
                    {
                        if (item.Key == site.Key)
                        {
                            NotEnabledSites.Remove(item);
                            break;
                        }
                    }
                }
                else if (target.Name == "NotEnabledSitesListView")
                {
                    NotEnabledSites.Insert(index, site);
                    foreach (var item in EnabledSites)
                    {
                        if (item.Key == site.Key)
                        {
                            EnabledSites.Remove(item);
                            break;
                        }
                    }
                }

                e.AcceptedOperation = DataPackageOperation.Move;
                def.Complete();
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await BangumiData.SetSitesEnabledOrder(EnabledSites.Select(it => it.Key).ToArray());
        }
    }

    public class SiteMetaWithKey : SiteMeta
    {
        public string Key { get; set; }

        public static SiteMetaWithKey FromSiteMeta(KeyValuePair<string, SiteMeta> keyValuePair) => new SiteMetaWithKey
        {
            Key = keyValuePair.Key,
            Title = keyValuePair.Value.Title,
            UrlTemplate = keyValuePair.Value.UrlTemplate,
            Type = keyValuePair.Value.Type
        };
    }
}
