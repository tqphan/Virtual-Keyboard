using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Data;

namespace Ziyi
{
    public class BoolToVisibility : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return Visibility.Visible;
            bool IsChecked = (bool)value;
            return IsChecked == true ? (object)Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Visibility)) return true;
            Visibility vis = (Visibility)value;
            return vis == Visibility.Visible ? (object)true : (object)false;
        }
    }
}
