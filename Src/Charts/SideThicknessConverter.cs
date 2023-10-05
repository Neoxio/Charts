using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Neoxio.Charts
{
    public sealed class SideThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Thickness thickness)
            {
                return new Thickness(thickness.Left, 0, thickness.Right, 0);
            }
            else
            {
                return new Thickness();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness)
            {
                return value;
            }
            else
            {
                return new Thickness();
            }
        }
    }
}
