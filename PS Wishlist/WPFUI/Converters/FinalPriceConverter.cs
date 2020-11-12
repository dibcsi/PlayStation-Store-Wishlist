using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPFUI
{
    public class FinalPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty((string)value) ? "NO PRICE! - ERROR" : (string)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
