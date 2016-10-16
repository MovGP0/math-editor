using System;
using System.Windows.Data;
using System.Globalization;

namespace Editor
{
    public sealed class EditorModeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = (EditorMode)value;
            return mode != EditorMode.Math;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
