using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Editor
{
    public class EditorModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = (EditorMode)value;
            return mode == EditorMode.Math 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
