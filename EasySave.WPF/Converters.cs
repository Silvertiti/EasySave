using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace EasySave.WPF.Converters
{
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ProgressToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int progress && parameter is string blockStr && int.TryParse(blockStr, out int blockId))
            {
                // Si le progrès est supérieur ou égal au numéro du bloc, on l'allume
                if (progress >= blockId)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // Vert
                }
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")); // Gris
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
