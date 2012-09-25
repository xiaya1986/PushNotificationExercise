using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Notifications_Server
{
    class ImageUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string f = parameter.ToString();
            string v = string.Format(f,value.ToString());
            Uri u = new Uri(v, UriKind.RelativeOrAbsolute);

            return u;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
