using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NotificationsTestClient
{
    public partial class Page1 : PhoneApplicationPage
    {
        /// <summary>
        /// Contains information about the locations displayed by the application.
        /// </summary>
        public Dictionary<string, LocationInformation> Locations { get; set; }

        public Page1()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Locations = (App.Current as App).Locations;
            base.OnNavigatedTo(e);

            mylist.ItemsSource = Locations.Values;
        }

        private static Uri MakeTileUri(LocationInformation locationInformation)
        {
            return new Uri(Uri.EscapeUriString(String.Format("/CityPage.xaml?location={0}",
               locationInformation.Name)), UriKind.Relative);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                (sender as ListBox).SelectedIndex = -1;
                NavigationService.Navigate(MakeTileUri(e.AddedItems[0] as LocationInformation));
            }

        }

        private void PinItem_Click(object sender, RoutedEventArgs e)
        {
            LocationInformation locationInformation =
         (sender as MenuItem).DataContext as LocationInformation;

            Uri tileUri = MakeTileUri(locationInformation);

            StandardTileData initialData = new StandardTileData()
            {
                BackgroundImage = new Uri("Images/Clear.png", UriKind.Relative),
                Title = locationInformation.Name
            };

            ((sender as MenuItem).Parent as ContextMenu).IsOpen = false;

            try
            {
                ShellTile.Create(tileUri, initialData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating tile", MessageBoxButton.OK);
                return;
            }

        }

        private void UnpinItem_Click(object sender, RoutedEventArgs e)
        {
            LocationInformation locationInformation = (sender as MenuItem).DataContext as LocationInformation;

            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault(
                t => t.NavigationUri.ToString().EndsWith(locationInformation.Name));

            if (tile == null)
            {
                MessageBox.Show("Tile inconsistency detected. It is suggested that you restart the application.");
                return;
            }

            try
            {
                tile.Delete();
                locationInformation.TilePinned = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error deleting tile", MessageBoxButton.OK);
                return;
            }
        }
    }
}