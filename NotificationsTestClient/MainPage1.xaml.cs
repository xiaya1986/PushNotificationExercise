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
    public partial class MainPage1 : PhoneApplicationPage
    {
        public MainPage1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Contains information about the components displayed by the application.
        /// </summary>
        public Dictionary<string, ComponentInformation> Components { get; set; }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Components = (App.Current as App).Components;
            tileList.ItemsSource = Components.Values;
            base.OnNavigatedTo(e);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                (sender as ListBox).SelectedIndex = -1;
                NavigationService.Navigate(MakeTileUri(e.AddedItems[0] as ComponentInformation));
            }

        }

        private void PinItem_Click(object sender, RoutedEventArgs e)
        {
            ComponentInformation componentInformation =
         (sender as MenuItem).DataContext as ComponentInformation;

            Uri tileUri = MakeTileUri(componentInformation);

            StandardTileData initialData = new StandardTileData()
            {
                BackgroundImage = new Uri("Images/Clear.png", UriKind.Relative),
                Title = componentInformation.Name
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
            ComponentInformation componentInformation =
        (sender as MenuItem).DataContext as ComponentInformation;

            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault(
                t => t.NavigationUri.ToString().EndsWith(
                componentInformation.Name));

            if (tile == null)
            {
                MessageBox.Show("Tile inconsistency detected. It is suggested that you restart the application.");
                return;
            }

            try
            {
                tile.Delete();
                componentInformation.TilePinned = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error deleting tile",
                MessageBoxButton.OK);
                return;
            }
        }

        /// <summary>
        /// Creates a Uri leading to the location specified by the location information to be bound to a tile.
        /// </summary>
        /// <param name="locationInformation">The location information for which to generate the Uri.</param>
        /// <returns>Uri for the page displaying information about the provided location.</returns>
        private static Uri MakeTileUri(ComponentInformation componentInformation)
        {
            return new Uri(Uri.EscapeUriString(String.Format("/ComponentPage.xaml?component={0}",
               componentInformation.Name)), UriKind.Relative);
        }
    }
}