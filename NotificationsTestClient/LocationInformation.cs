// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace NotificationsTestClient
{
    /// <summary>
    /// Information about a location monitored by the client.
    /// </summary>
    public class LocationInformation : INotifyPropertyChanged
    {
        private bool tilePinned;
        private string name;
        private string temperature;
        private string imageName;

        /// <summary>
        /// Whether or not the location's secondary tile has been pinned by the user.
        /// </summary>
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

        /// <summary>
        /// The location's name.
        /// </summary>
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

        /// <summary>
        /// The temperature at the location.
        /// </summary>
        public string Temperature
        {
            get
            {
                return temperature;
            }
            set
            {
                if (value != temperature)
                {
                    temperature = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Temperature"));
                    }
                }
            }
        }

        /// <summary>
        /// The name of the image to use for representing the weather at the location.
        /// </summary>
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
