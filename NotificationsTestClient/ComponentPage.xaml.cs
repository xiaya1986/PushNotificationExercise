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

namespace NotificationsTestClient
{
    public partial class ComponentPage : PhoneApplicationPage
    {
        public ComponentPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataContext = (App.Current as App).Components[
                NavigationContext.QueryString["component"]];


            // Request the latest data, as the data we have may not be the latest.
            (App.Current as App).RequestLatestData(NavigationContext.QueryString["component"]);      
            base.OnNavigatedTo(e);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}