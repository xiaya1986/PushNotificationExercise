using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace NotificationsTestClient
{
    public class ComponentInformation:INotifyPropertyChanged
    {
        private bool tilePinned;
        private string name;
        private string passRate;
        private string imageName;
        private string testProgress;
        private string testCoverage;
        private string codeCoverage;

        public bool TilePinned 
        {
            get 
            {
                return tilePinned;
            }
            set 
            {
                if (value != tilePinned)
                {
                    tilePinned = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TilePinned"));
                    }
                }
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value != name)
                {
                    name = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                    }
                }
            }
        }

        public string ImageName
        {
            get
            {
                return imageName;
            }
            set
            {
                if (value != imageName)
                {
                    imageName = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ImageName"));
                    }
                }
            }
        }

        public string PassRate
        {
            get
            {
                return passRate;
            }
            set
            {
                if (value != passRate)
                {
                    passRate = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PassRate"));
                    }
                }
            }
        }

        public string TestProgress {
            get 
            {
                return testProgress;
            }
            set 
            {
                if (value != testProgress)
                {
                    testProgress = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TestProgress"));
                    }
                }
            }
        }

        public string TestCoverage {
            get 
            {
                return testCoverage;
            }
            set 
            {
                if (value != testCoverage)
                {
                    testCoverage = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TestCoverage"));
                    }
                }
            }
        }

        public string CodeCoverage
        {
            get 
            {
                return codeCoverage;
            }
            set 
            {
                if (value != codeCoverage)
                {
                    codeCoverage = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("CodeCoverage"));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
