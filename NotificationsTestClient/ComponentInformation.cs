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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
