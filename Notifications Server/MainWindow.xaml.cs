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
        #endregion

        public MainWindow()
        {
            InitializeComponent();
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
            byte[] payload = prepareRAWPayload(textBox1.Text);

            rawPushNotificationMessage.RawData = payload;
            subscribers.ForEach(uri => rawPushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Raw, result),
                (result) => { }));

        }

        private void sendToast()
        {
            //TODO - Add TOAST notifications sending logic here
        }

        private void sendTile()
        {
            //TODO - Add TILE notifications sending logic here
        }

        private void sendRemoteTile()
        {
            //TODO - Add TILE with remote image URI logic here
        }

        private static byte[] prepareRAWPayload(string content)
        {
            MemoryStream stream = new MemoryStream();

            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 };
            XmlWriter writer = XmlTextWriter.Create(stream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("MessageUpdate");

            writer.WriteStartElement("Message");
            writer.WriteValue(content);
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

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            sendHttp();
        }

    }
}
