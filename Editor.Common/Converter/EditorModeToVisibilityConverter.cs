using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Editor.Converter
{
    public sealed class EditorModeToVisibilityConverter : IValueConverter
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
