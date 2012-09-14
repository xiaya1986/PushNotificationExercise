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

        private void ParseRAWPayload(Stream e, out string content)
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
        #endregion

        #region Channel event handlers
        void httpChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            Trace("Channel opened. Got Uri:\n" + httpChannel.ChannelUri.ToString());
            Trace("Subscribing to channel events");
            SubscribeToService();

            Dispatcher.BeginInvoke(() => UpdateStatus("Channel created successfully"));
        }

        void httpChannel_ExceptionOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(() => UpdateStatus(e.ErrorType + " occurred: " + e.Message));
        }

        void httpChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {

            Trace("===============================================");
            Trace("RAW notification arrived:");

            string content;
            ParseRAWPayload(e.Notification.Body, out content);

            Dispatcher.BeginInvoke(() => this.textBlock2.Text = content);
            Trace(string.Format("Got message: {0}", content));

            Trace("===============================================");
        }
        #endregion
    }
}