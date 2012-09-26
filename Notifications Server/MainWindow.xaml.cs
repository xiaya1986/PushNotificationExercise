using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using WindowsPhone.Recipes.Push.Messasges;
using Notifications_Server.Service;
using System.IO;
using System.Xml;

namespace Notifications_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private variables
        private ObservableCollection<PushNotificationsLogMsg> trace = new ObservableCollection<PushNotificationsLogMsg>();
        private RawPushNotificationMessage rawPushNotificationMessage = new RawPushNotificationMessage(MessageSendPriority.High);
        private TilePushNotificationMessage tilePushNotificationMessage = new TilePushNotificationMessage(MessageSendPriority.High);
        private ToastPushNotificationMessage toastPushNotificationMessage = new ToastPushNotificationMessage(MessageSendPriority.High);

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitializePic();
            InitializeProjects();
            //Log.ItemsSource = trace;
            RegistrationService.Subscribed += new EventHandler<RegistrationService.SubscriptionEventArgs>(RegistrationService_Subscribed);
        }

        void RegistrationService_Subscribed(object sender, RegistrationService.SubscriptionEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            { UpdateStatus(); })
            );
        }

        private void OnMessageSent(NotificationType type, MessageSendResult result)
        {
            PushNotificationsLogMsg msg =
                new PushNotificationsLogMsg(type, result);
            Dispatcher.BeginInvoke((Action)(() =>
            { trace.Add(msg); }));
        }

        private void UpdateStatus()
        {
            int activeSubscribers = RegistrationService.GetSubscribers().Count;
            bool isReady = (activeSubscribers > 0);
            //txtActiveConnections.Text = activeSubscribers.ToString();
            txtStatus.Text = isReady ? "Ready" : "Waiting for connection...";
        }

        private void sendHttp()
        {
            //Get the list of subscribed WP7 clients
            List<Uri> subscribers = RegistrationService.GetSubscribers();
            //Prepare payload
            byte[] payload = prepareRAWPayload(textBox1.Text,
                cmbProject.SelectedValue as string,
                sld.Value.ToString("F1"),
                cmbPic.SelectedValue as string);

            rawPushNotificationMessage.RawData = payload;
            subscribers.ForEach(uri => rawPushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Raw, result),
                (result) => { }));
        }

        private void sendToast()
        {
            //TODO - Add TOAST notifications sending logic here
            string msg = txtToastMessage.Text;
            txtToastMessage.Text = "";
            List<Uri> subscribers = RegistrationService.GetSubscribers();

            toastPushNotificationMessage.Title = "WEATHER ALERT";
            toastPushNotificationMessage.SubTitle = msg;

            subscribers.ForEach(uri =>
                toastPushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Toast, result),
                (result) => { }));
        }

        private void sendTile()
        {
            //TODO - Add TILE notifications sending logic here
            string picType = cmbPic.SelectedValue as string;
            int passRate = (int)(sld.Value);
            string project = cmbProject.SelectedValue as string;
            List<Uri> subscribers = RegistrationService.GetSubscribers();

            tilePushNotificationMessage.BackgroundImageUri = new Uri("Images/" + picType + ".png", UriKind.Relative);
            tilePushNotificationMessage.Count = passRate;
            tilePushNotificationMessage.Title = project;

            subscribers.ForEach(uri => tilePushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Token, result),
                (result) => { }));
        }

        private void sendRemoteTile()
        {
            //TODO - Add TILE with remote image URI logic here
        }

        private static byte[] prepareRAWPayload(string content, string project, string passRate, string picType)
        {
            MemoryStream stream = new MemoryStream();

            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 };
            XmlWriter writer = XmlTextWriter.Create(stream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("MessageUpdate");

            writer.WriteStartElement("Message");
            writer.WriteValue(content);
            writer.WriteEndElement();

            writer.WriteStartElement("Project");
            writer.WriteValue(project);
            writer.WriteEndElement();

            writer.WriteStartElement("PassRate");
            writer.WriteValue(passRate);
            writer.WriteEndElement();

            writer.WriteStartElement("PicType");
            writer.WriteValue(picType);
            writer.WriteEndElement();

            writer.WriteStartElement("LastUpdated");
            writer.WriteValue(DateTime.Now.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            byte[] payload = stream.ToArray();
            return payload;
        }

        private void btnSendRAW_Click(object sender, RoutedEventArgs e)
        {
            sendHttp();
        }

        private void btnSendToast_Click(object sender, RoutedEventArgs e)
        {
            sendToast();
        }

        private void btnSendTile_Click(object sender, RoutedEventArgs e)
        {
            sendTile();
        }

        private void InitializePic()
        {
            Dictionary<string, string> weather = new Dictionary<string, string>();
            weather.Add("Chance_Of_Showers", "Chance Of Showers");
            weather.Add("Clear", "Clear");
            weather.Add("Cloudy", "Cloudy");
            weather.Add("Cloudy_Period", "Cloudy Period");
            weather.Add("Cloudy_With_Drizzle", "Cloudy With Drizzle");
            weather.Add("Few_Flurries", "Few Flurries");
            weather.Add("Few_Flurries_Night", "Few Flurries Night");
            weather.Add("Few_Showers", "Few Showers");
            weather.Add("Flurries", "Flurries");
            weather.Add("Fog", "Fog");
            weather.Add("Freezing_Rain", "Freezing Rain");
            weather.Add("Mostly_Cloudy", "Mostly Cloudy");
            weather.Add("Mostly_Sunny", "Mostly Sunny");
            weather.Add("Rain", "Rain");
            weather.Add("Rain_Or_Snow", "Rain Or Snow");
            weather.Add("Risk_Of_Thundershowers", "Risk Of Thundershowers");
            weather.Add("Snow", "Snow");
            weather.Add("Sunny", "Sunny");
            weather.Add("Thunder_Showers", "Thunder Showers");
            weather.Add("Thunderstorms", "Thunderstorms");
            weather.Add("Wet_Flurries", "Wet Flurries");
            weather.Add("Wet_Flurries_Night", "Wet Flurries Night");

            cmbPic.ItemsSource = weather;
            cmbPic.DisplayMemberPath = "Value";
            cmbPic.SelectedValuePath = "Key";
            cmbPic.SelectedIndex = 0;
        }

        private void InitializeProjects()
        {
            List<string> projects = new List<string>();
            projects.Add("ATT");
            projects.Add("Import/Export");
            projects.Add("Model");
            projects.Add("Language");
            projects.Add("Designer");

            cmbProject.ItemsSource = projects;
            cmbProject.SelectedIndex = 0;
        }
    }
}
