using System;
using Windows.UI.Text;
using Windows.UI.Xaml.Media;

namespace Editor
{   
    public sealed class TextFormat
    {
        public double FontSize { get; }
        public FontType FontType { get; }
        public FontFamily FontFamily { get; }
        public FontStyle FontStyle { get; }
        public FontWeight FontWeight { get; }
        public SolidColorBrush TextBrush { get; }
        public bool UseUnderline { get; set; }
        public int Index { get; set; }

        public TextFormat(double size, FontType ft, FontStyle fs, FontWeight fw, SolidColorBrush brush, bool useUnderline)
        {
            FontSize = Math.Round(size, 1);
            FontType = ft;
            FontFamily = FontFactory.GetFontFamily(ft);
            FontStyle = fs;
            UseUnderline = useUnderline;
            FontWeight = fw;
            TextBrush = brush;
        }
    }
}
