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
using Microsoft.Phone.Notification;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;


namespace NotificationsTestClient
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ConnectToMSPN();
        }

        private HttpNotificationChannel httpChannel;
        const string channelName = "NotificationChannel";
        const string fileName = "PushNotificationsSettings.dat";
        const int pushConnectTimeout = 30;
        const string toastMessage = "Toast";
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication();

        #region Tracing and Status Updates
        private void UpdateStatus(string message)
        {
            txtStatus.Text = message;
        }

        private void Trace(string message)
        {
#if DEBUG
            Debug.WriteLine(message);
#endif
        }
        #endregion

        private void ConnectToMSPN()
        {
            try
            {
                //First, try to pick up existing channel
                httpChannel = HttpNotificationChannel.Find(channelName);

                if (null != httpChannel)
                {
                    Trace("Channel Exists - no need to create a new one");
                    SubscribeToChannelEvents();

                    Trace("Register the URI with 3rd party web service");
                    SubscribeToService();

                    //TODO: Place Notification
                    Trace("Subscribe to the channel to Tile and Toast notifications");
                    SubscribeToNotifications();

                    Dispatcher.BeginInvoke(() => UpdateStatus("Channel recovered"));
                }
                else
                {
                    Trace("Trying to create a new channel...");
                    //Create the channel
                    httpChannel = new HttpNotificationChannel(channelName, "HOLWeatherService");
                    Trace("New Push Notification channel created successfully");

                    SubscribeToChannelEvents();

                    Trace("Trying to open the channel");
                    httpChannel.Open();
                    Dispatcher.BeginInvoke(() => UpdateStatus("Channel open requested"));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(() => UpdateStatus("Channel error: " + ex.Message));
            }
        }

        private void ParseRAWPayload(Stream e, out string content, out string project, out string passRate, out string picType)
        {
            XDocument document;

            using (var reader = new StreamReader(e))
            {
                string payload = reader.ReadToEnd().Replace('\0',
                  ' ');
                document = XDocument.Parse(payload);
            }

            content = (from c in document.Descendants("MessageUpdate")
                       select c.Element("Message").Value).FirstOrDefault();
            Trace("Got content: " + content);

            project = (from c in document.Descendants("MessageUpdate")
                           select c.Element("Project").Value).FirstOrDefault();
            Trace("Got Project: " + project);

            passRate = (from c in document.Descendants("MessageUpdate")
                        select c.Element("PassRate").Value).FirstOrDefault();
            Trace("Got PassRate: " + passRate);

            picType = (from c in document.Descendants("MessageUpdate")
                        select c.Element("PicType").Value).FirstOrDefault();
            Trace("Got PicType: " + picType);
        }

        #region Subscriptions
        private void SubscribeToChannelEvents()
        {
            //Register to UriUpdated event - occurs when channel successfully opens
            httpChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(httpChannel_ChannelUriUpdated);

            //Subscribed to Raw Notification
            httpChannel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(httpChannel_HttpNotificationReceived);

            //general error handling for push channel
            httpChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(httpChannel_ExceptionOccurred);

            //////////////////////////////////////////
            // Toast Notification 
            //////////////////////////////////////////
            //subscrive to toast notification when running app
            httpChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(httpChannel_ShellToastNotificationReceived);
        }

        private void SubscribeToService()
        {
            //Hardcode for solution - need to be updated in case the REST WCF service address change
            string baseUri = "http://localhost:8000/RegirstatorService/Register?uri={0}";
            string theUri = String.Format(baseUri, httpChannel.ChannelUri.ToString());
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (s, e) =>
            {
                if (null == e.Error)
                    Dispatcher.BeginInvoke(() => UpdateStatus("Registration succeeded"));
                else
                    Dispatcher.BeginInvoke(() => UpdateStatus("Registration failed: " + e.Error.Message));
            };
            client.DownloadStringAsync(new Uri(theUri));
        }

        private void SubscribeToNotifications()
        {
            //////////////////////////////////////////
            // Bind to Toast Notification 
            //////////////////////////////////////////
            try
            {
                if (httpChannel.IsShellToastBound == true)
                {
                    Trace("Already bounded (register) to Toast notification");
                }
                else
                {
                    Trace("Registering to Toast Notifications");
                    httpChannel.BindToShellToast();
                }
            }
            catch (Exception ex)
            {
                // handle error here
            }

            //////////////////////////////////////////
            // Bind to Tile Notification 
            //////////////////////////////////////////
            try
            {
                if (httpChannel.IsShellTileBound == true)
                {
                    Trace("Already bounded (register) to Tile Notifications");
                }
                else
                {
                    Trace("Registering to Tile Notifications");

                    // you can register the phone application to receive tile images from remote servers [this is optional]
                    Collection<Uri> uris = new Collection<Uri>();
                    uris.Add(new Uri("http://www.larvalabs.com"));

                    httpChannel.BindToShellTile(uris);
                }
            }
            catch (Exception ex)
            {
                //handle error here
            }
        }

        #endregion

        #region Channel event handlers
        void httpChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            Trace("Channel opened. Got Uri:\n" + httpChannel.ChannelUri.ToString());
            Trace("Subscribing to channel events");
            SubscribeToService();

            SubscribeToNotifications();

            Dispatcher.BeginInvoke(() => UpdateStatus("Channel created successfully"));
        }

        void httpChannel_ExceptionOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(() => UpdateStatus(e.ErrorType + " occurred: " + e.Message));
        }

        // RAW notification
        void httpChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            Trace("===============================================");
            Trace("RAW notification arrived:");

            string content, project, passRate, picType;
            ParseRAWPayload(e.Notification.Body, out content, out project, out passRate, out picType);

            Dispatcher.BeginInvoke(() => this.textBlock2.Text = content);
            Dispatcher.BeginInvoke(() => this.textBlockListTitle.Text = project);
            Dispatcher.BeginInvoke(() => this.textBlockPassrate.Text = passRate);
            Dispatcher.BeginInvoke(() => this.imgPics.Source = new BitmapImage(new Uri(@"Images/" + picType + ".png", UriKind.Relative)));
            Trace(string.Format("Got message: {0}", content));

            Trace("===============================================");
        }

        void httpChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            Trace("===============================================");
            Trace("Toast/Tile notification arrived:");
            foreach (var key in e.Collection.Keys)
            {
                string msg = e.Collection[key];

                Trace(msg);
                Dispatcher.BeginInvoke(() => this.textBlock5.Text = "Toast/Tile message: " + msg);

                using (var file = appStorage.OpenFile(toastMessage, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(file))
                    {
                        sw.WriteLine(msg);
                    }
                }
            }

            Trace("===============================================");
        }

        #endregion

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (appStorage.FileExists(toastMessage))
                {
                    using (var file = appStorage.OpenFile(toastMessage, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            textBlock5.Text = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (IsolatedStorageException ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            // Reset Tile
            var appTile = ShellTile.ActiveTiles.FirstOrDefault();
            if (appTile == null)
            {
                return;
                //Don't create...just update
            }
            else
            {
                appTile.Update(new StandardTileData()
                {
                    Count = 0,
                    Title = GetWinPhoneAttribute("Title"),
                    BackgroundImage = new Uri("ApplicationIcon.png", UriKind.Relative),
                });
            }
        }

        private static string GetWinPhoneAttribute(string attributeName)
        {
            string ret = string.Empty;

            try
            {
                XElement xe = XElement.Load("WMAppManifest.xml");
                var attr = (from manifest in xe.Descendants("App")
                            select manifest).SingleOrDefault();
                if (attr != null)
                    ret = attr.Attribute(attributeName).Value;
            }
            catch
            {
                // Ignore errors in case this method is called
                // from design time in VS.NET
            }

            return ret;
        }
    }
}