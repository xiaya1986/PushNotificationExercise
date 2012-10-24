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
using System.Windows.Shapes;
using Notifications_Server.Service;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using WindowsPhone.Recipes.Push.Messasges;
using System.Timers;
using System.Windows.Threading;
using System.Threading;

namespace Notifications_Server
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
                RegistrationService.Subscribed += new EventHandler<RegistrationService.SubscriptionEventArgs>(RegistrationService_Subscribed);
            RegistrationService.DataRequested += new EventHandler<RegistrationService.DataRequestEventArgs>(RegistrationService_DataRequested);
        
        }

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
        private const int sleepTime = 6000;
        #endregion

        

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
            txtStatus.Text = isReady ? "Ready" : "Waiting for connection...";
        }

        private void sendHttp(TestData data)
        {
            UpdateSentData(data);

            //Get the list of subscribed WP7 clients
            List<Uri> subscribers = RegistrationService.GetSubscribers();
            //Prepare payload
            byte[] payload = prepareRAWPayload(
                data.Name,
                data.PassRate.ToString(),
                data.ImageName,
                data.TestProgress.ToString(),
                data.TestCoverage.ToString(),
                data.CodeCoverage.ToString());

            rawPushNotificationMessage.RawData = payload;
            subscribers.ForEach(uri => rawPushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Raw, result),
                (result) => { }));
        }

        private void sendToast(TestData data)
        {
            UpdateSentData(data);
            //TODO - Add TOAST notifications sending logic here
            string msg = "Test completed";
            List<Uri> subscribers = RegistrationService.GetSubscribers();

            toastPushNotificationMessage.Title = String.Format("Test ALERT({0})", data.Name);
            toastPushNotificationMessage.SubTitle = msg;
            toastPushNotificationMessage.TargetPage = MakeTileUri(data.Name).ToString();


            subscribers.ForEach(uri =>
                toastPushNotificationMessage.SendAsync(uri,
                (result) => OnMessageSent(NotificationType.Toast, result),
                (result) => { }));
        }

        private void sendTile(TestData data)
        {
            UpdateSentData(data);

            //TODO - Add TILE notifications sending logic here
            string picType = data.ImageName;
            int passRate = data.PassRate;
            string project = data.Name;
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

        private static Uri MakeTileUri(string locationName)
        {
            return new Uri(Uri.EscapeUriString(String.Format(
                "/ComponentPage.xaml?component={0}",
                        locationName)), UriKind.Relative);
        }

        private void UpdateSentData(TestData data)
        {
            componentsSentData[data.Name] = new SentData
            {
                PassRate = data.PassRate.ToString(),
                ImageType = data.ImageName,
                TestProgress = data.TestProgress.ToString(),
                TestCoverage = data.TestCoverage.ToString(),
                CodeCoverage = data.CodeCoverage.ToString()
            };
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
    

        /// <summary>
        /// Initializes the Test data.
        /// </summary>
        private Dictionary<string,TestData> InitializeTestData()
        {
            Dictionary<string, TestData> Datas;

            List<TestData> testDataList = new List<TestData>(new[] { 
        new TestData { Name = "ATT", 
            ImageName = "In_Progress", PassRate = 0, TestProgress = 0, TestCoverage = 100, CodeCoverage = 100},

        new TestData { Name = "Designer", 
            ImageName = "In_Progress", PassRate = 0, TestProgress = 0, TestCoverage = 100, CodeCoverage = 95},

        new TestData { Name = "Import", 
            ImageName = "In_Progress", PassRate = 0, TestProgress = 0, TestCoverage = 100, CodeCoverage = 96},

        new TestData { Name = "Language", 
            ImageName = "In_Progress", PassRate = 0, TestProgress = 0, TestCoverage = 100, CodeCoverage = 94},

        new TestData { Name = "Model", 
            ImageName = "In_Progress", PassRate = 0, TestProgress = 0, TestCoverage = 100, CodeCoverage = 98}});

            Datas = testDataList.ToDictionary(l => l.Name);

            return Datas;
        }

         
        private void start_Click(object sender, RoutedEventArgs e)
        {
            progressBar1.IsIndeterminate = true;
            this.btnStart.IsEnabled = false;
            this.tbxMessage.Focus();
            Thread demo = new Thread(new ThreadStart(DemoNotificaiton));
            demo.Start();
        }

        private void DemoNotificaiton()
        {
            Dictionary<string, TestData> Datas;
            Datas = InitializeTestData();

            PlayDemo(Datas["ATT"], 100, 25, 50);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["ATT"], 100, 30, 60);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["ATT"], 100, 50, 70);
            PlayDemo(Datas["Designer"], 100, 10, 70);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["ATT"], 100, 70, 80);
            PlayDemo(Datas["Designer"], 100, 20, 70);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["ATT"], 100, 80, 90);
            PlayDemo(Datas["Designer"], 100, 50, 70);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["ATT"], 100, 100, 98);
            PlayDemo(Datas["Designer"], 98, 60, 80);
            PlayDemo(Datas["Model"], 100, 10, 20);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Designer"], 98, 70, 85);
            PlayDemo(Datas["Model"], 100, 10, 30);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Designer"], 97, 80, 85);
            PlayDemo(Datas["Model"], 100, 20, 40);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Designer"], 97, 90, 90);
            PlayDemo(Datas["Model"], 100, 30, 50);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Designer"], 96, 100, 95);
            PlayDemo(Datas["Model"], 100, 40, 30);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Model"], 100, 60, 35);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Model"], 100, 70, 35);
            PlayDemo(Datas["Import"], 100, 12, 35);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Model"], 100, 90, 35);
            PlayDemo(Datas["Import"], 100, 25, 45);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Model"], 100, 100, 45);
            PlayDemo(Datas["Import"], 100, 36, 45);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Import"], 100, 48, 50);
            PlayDemo(Datas["Language"], 100, 5, 20);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Import"], 100, 68, 50);
            PlayDemo(Datas["Language"], 100, 15, 20);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Import"], 100, 78, 65);
            PlayDemo(Datas["Language"], 100, 28, 20);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Import"], 100, 86, 50);
            PlayDemo(Datas["Language"], 100, 45, 30);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Import"], 100, 95, 60);
            PlayDemo(Datas["Language"], 100, 58, 50);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Import"], 100, 100, 80);
            PlayDemo(Datas["Language"], 100, 69, 60);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Language"], 100, 78, 74);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Language"], 100, 94, 74);
            Thread.Sleep(sleepTime);
            PlayDemo(Datas["Language"], 100, 100, 85);
            Thread.Sleep(sleepTime);
            EndTest();
        }

        private void PlayDemo(TestData data, int passRate, int testProgress, int testCoverage)
        {
            data.PassRate = passRate;
            data.TestProgress = testProgress;
            data.TestCoverage = testCoverage;
            if (testProgress == 100)
            {
                if (passRate == 100)
                {
                    data.ImageName = "Passed";
                }
                else
                {
                    data.ImageName = "Failed";
                }
                sendToast(data);
            }

            sendTile(data);
            sendHttp(data);
            LogMessage(String.Format("{0}: Run Test Suite '{1}', {2}% Completed, {3}% Passed. \n",
                            DateTime.Now.ToString(),
                            data.Name,
                            data.TestProgress,
                            data.PassRate
                            ));


        }

        private delegate void SetMessageCallback(string message);
        private void LogMessage(string message)
        {
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new SetMessageCallback(DisplayLogMessage),
                message);
        }
        private void DisplayLogMessage(string message)
        {
            this.tbxMessage.Text += message;
        }

        private delegate void EndTestCallback();
        private void EndTest()
        {
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new EndTestCallback(EndTestSettings));
        }
        private void EndTestSettings()
        {
            this.progressBar1.IsIndeterminate = false;
            this.btnStart.IsEnabled = true;
        }

        private void tbxMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbxMessage.SelectionStart = tbxMessage.Text.Length;
        }
    }

    public class TestData
    {
        public string Name { get; set; }
        public string ImageName { get; set; }
        public int PassRate { get; set; }
        public int TestProgress { get; set; }
        public int TestCoverage { get; set; }
        public int CodeCoverage { get; set; }
    }
}
