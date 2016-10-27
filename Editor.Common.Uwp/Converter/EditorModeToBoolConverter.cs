using System;
using Windows.UI.Xaml.Data;

namespace Editor.Converter
{
    public sealed class EditorModeToBoolConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            var mode = (EditorMode)value;
            return mode != EditorMode.Math;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
