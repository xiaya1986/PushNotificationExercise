﻿using System;
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
        #region SentData internal type
        /// <summary>
        /// A data type used to store the most recent data sent to each location in order to support clients
        /// actively requesting the latest data.
        /// </summary>
        public class SentData
        {
            public string PassRate { get; set; }
            public string ImageType { get; set; }
            public string TestProgress { get; set; }
            public string TestCoverage { get; set; }
            public string CodeCoverage { get; set; }
        }
        #endregion

        #region Private variables
        private ObservableCollection<PushNotificationsLogMsg> trace = new ObservableCollection<PushNotificationsLogMsg>();
        private RawPushNotificationMessage rawPushNotificationMessage = new RawPushNotificationMessage(MessageSendPriority.High);
        private TilePushNotificationMessage tilePushNotificationMessage = new TilePushNotificationMessage(MessageSendPriority.High);
        private ToastPushNotificationMessage toastPushNotificationMessage = new ToastPushNotificationMessage(MessageSendPriority.High);
        private Dictionary<string, SentData> componentsSentData = new Dictionary<string, SentData>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitializePic();
            InitializeComponents();
            //Log.ItemsSource = trace;
            RegistrationService.Subscribed += new EventHandler<RegistrationService.SubscriptionEventArgs>(RegistrationService_Subscribed);
            RegistrationService.DataRequested += new EventHandler<RegistrationService.DataRequestEventArgs>(RegistrationService_DataRequested);
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
            UpdateSentData();

            //Get the list of subscribed WP7 clients
            List<Uri> subscribers = RegistrationService.GetSubscribers();
            //Prepare payload
            byte[] payload = prepareRAWPayload(
                cmbProject.SelectedValue as string,
                sld.Value.ToString("F1"),
                cmbPic.SelectedValue as string,
                sldTestProgress.Value.ToString("F1"),
                sldTestCoverage.Value.ToString("F1"),
                sldCodeCoverage.Value.ToString("F1"));

            rawPushNotificationMessage.RawData = payload;
            subscribers.ForEach(uri => rawPushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Raw, result),
                (result) => { }));
        }

        private void UpdateSentData()
        {
            componentsSentData[cmbProject.SelectedValue as string] = new SentData
            {
                PassRate = sld.Value.ToString("F1"),
                ImageType = cmbPic.SelectedValue as string,
                TestProgress = sldTestProgress.Value.ToString("F1"),
                TestCoverage = sldTestCoverage.Value.ToString("F1"),
                CodeCoverage = sldCodeCoverage.Value.ToString("F1")
            };
        }

        private void sendToast1()
        {
            //TODO - Add TOAST notifications sending logic here
            string msg = txtToastMessage.Text;
            txtToastMessage.Text = "";
            List<Uri> subscribers = RegistrationService.GetSubscribers();

            toastPushNotificationMessage.Title = "Test ALERT";
            toastPushNotificationMessage.SubTitle = msg;

            subscribers.ForEach(uri =>
                toastPushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Toast, result),
                (result) => { }));
        }

        private void sendToast()
        {
            UpdateSentData();
            //TODO - Add TOAST notifications sending logic here
            string msg = txtToastMessage.Text;
            txtToastMessage.Text = "";
            List<Uri> subscribers = RegistrationService.GetSubscribers();

            toastPushNotificationMessage.Title = String.Format("Test ALERT({0})", cmbProject.SelectedValue);
            toastPushNotificationMessage.SubTitle = msg;
            toastPushNotificationMessage.TargetPage = MakeTileUri(cmbProject.SelectedValue.ToString()).ToString();


            subscribers.ForEach(uri =>
                toastPushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Toast, result),
                (result) => { }));
        }

        private void sendTile1()
        {
            //TODO - Add TILE notifications sending logic here
            string picType = cmbPic.SelectedValue as string;
            int passRate = (int)(sld.Value);
            string project = cmbProject.SelectedValue as string;
            List<Uri> subscribers = RegistrationService.GetSubscribers();

            tilePushNotificationMessage.BackgroundImageUri = new Uri("Images/" + picType + ".png", UriKind.Relative);
            if (passRate <= 99)
            {
                tilePushNotificationMessage.Count = passRate;
            }
            else
            {
                tilePushNotificationMessage.Count = 0;
            }
            tilePushNotificationMessage.Title = project;

            subscribers.ForEach(uri => tilePushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Token, result),
                (result) => { }));
        }

        private void sendTile()
        {
            UpdateSentData();

            //TODO - Add TILE notifications sending logic here
            string picType = cmbPic.SelectedValue as string;
            int passRate = (int)(sld.Value);
            string project = cmbProject.SelectedValue as string;
            List<Uri> subscribers = RegistrationService.GetSubscribers();

            tilePushNotificationMessage.BackgroundImageUri = new Uri("Images/" + picType + ".png", UriKind.Relative);
            if (passRate <= 99)
                tilePushNotificationMessage.Count = passRate;
            else
            {
                tilePushNotificationMessage.Count = 0;
            }
            tilePushNotificationMessage.Title = project;
            tilePushNotificationMessage.SecondaryTile = MakeTileUri(project).ToString();


            subscribers.ForEach(uri => tilePushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Token, result),
                (result) => { }));
        }

        private void sendRemoteTile()
        {
            //TODO - Add TILE with remote image URI logic here
        }

        private static Uri MakeTileUri(string locationName)
        {
            return new Uri(Uri.EscapeUriString(String.Format(
                "/ComponentPage.xaml?component={0}",
                        locationName)), UriKind.Relative);
        }

        private void btnSendRAW_Click(object sender, RoutedEventArgs e)
        {
            sendHttp();
            sendTile();
        }

        private void btnSendToast_Click(object sender, RoutedEventArgs e)
        {
            sendToast();
            sendHttp();
            sendTile();
        }

        private void btnSendTile_Click(object sender, RoutedEventArgs e)
        {
            sendTile();
            sendHttp();
        }

        private void InitializePic()
        {
            Dictionary<string, string> pictures = new Dictionary<string, string>();
            pictures.Add("In_Progress", "In_Progress");
            pictures.Add("Passed", "Passed");
            pictures.Add("Failed", "Failed");

            cmbPic.ItemsSource = pictures;
            cmbPic.DisplayMemberPath = "Value";
            cmbPic.SelectedValuePath = "Key";
            cmbPic.SelectedIndex = 0;
        }

        private void InitializeComponents()
        {
            List<string> projects = new List<string>();
            projects.Add("ATT");
            projects.Add("Import");
            projects.Add("Model");
            projects.Add("Language");
            projects.Add("Designer");

            cmbProject.ItemsSource = projects;
            cmbProject.SelectedIndex = 0;
        }

        void RegistrationService_DataRequested(object sender, RegistrationService.DataRequestEventArgs e)
        {
            if (!componentsSentData.ContainsKey(e.ComponentName))
            {
                return;
            }

            SentData latestData = componentsSentData[e.ComponentName];

            // Send raw message
            byte[] payload = prepareRAWPayload(e.ComponentName, latestData.PassRate, latestData.ImageType, latestData.TestProgress, latestData.TestCoverage, latestData.CodeCoverage);

            rawPushNotificationMessage.RawData = payload;

            rawPushNotificationMessage.SendAsync(e.ChannelUri,
                (result) => OnMessageSent(NotificationType.Raw, result),
                (result) => { });

            // send tile message
            tilePushNotificationMessage.BackgroundImageUri = new Uri("/Images/" + latestData.ImageType + ".png", UriKind.Relative);
            if (double.Parse(latestData.PassRate) <= 99)
                tilePushNotificationMessage.Count = Convert.ToInt32(double.Parse(latestData.PassRate));
            else
            {
                tilePushNotificationMessage.Count = 0;
            }
            tilePushNotificationMessage.Title = e.ComponentName;
            tilePushNotificationMessage.SecondaryTile = MakeTileUri(e.ComponentName).ToString();
        }

        private static byte[] prepareRAWPayload(string content, string component, string passRate, string picType)
        {
            MemoryStream stream = new MemoryStream();

            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 };
            XmlWriter writer = XmlTextWriter.Create(stream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("MessageUpdate");

            writer.WriteStartElement("Message");
            writer.WriteValue(content);
            writer.WriteEndElement();

            writer.WriteStartElement("Component");
            writer.WriteValue(component);
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

        private static byte[] prepareRAWPayload(string component, string passRate, string picType, string testProgress, string testCoverage, string codeCoverage)
        {
            MemoryStream stream = new MemoryStream();

            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 };
            XmlWriter writer = XmlTextWriter.Create(stream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("MessageUpdate");

            writer.WriteStartElement("Component");
            writer.WriteValue(component);
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

            writer.WriteStartElement("TestProgress");
            writer.WriteValue(testProgress);
            writer.WriteEndElement();

            writer.WriteStartElement("TestCoverage");
            writer.WriteValue(testCoverage);
            writer.WriteEndElement();

            writer.WriteStartElement("CodeCoverage");
            writer.WriteValue(codeCoverage);
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            byte[] payload = stream.ToArray();
            return payload;
        }
    }
}
