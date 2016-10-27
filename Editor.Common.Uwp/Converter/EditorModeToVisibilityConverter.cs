using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Editor.Converter
{
    public sealed class EditorModeToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            var mode = (EditorMode)value;
            return mode == EditorMode.Math 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
