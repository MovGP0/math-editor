using System;
using System.Windows.Media;
using System.Windows;
using System.Xml.Linq;

namespace Editor
{   
    public class TextFormat
    {
        public double FontSize { get; }
        public FontType FontType { get; }
        public FontFamily FontFamily { get; }
        public FontStyle FontStyle { get; }
        public FontWeight FontWeight { get; }
        public SolidColorBrush TextBrush { get; private set; }
        public Typeface TypeFace { get; private set; }
        public string TextBrushString { get; }
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
            TypeFace = new Typeface(FontFamily, fs, fw, FontStretches.Normal, FontFactory.GetFontFamily(FontType.StixGeneral));
            var bc = new BrushConverter();
            TextBrushString = bc.ConvertToString(brush);
        }

        public XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(new XElement("FontSize", FontSize),
                             new XElement("FontType", FontType),
                             new XElement("FontStyle", FontStyle),
                             new XElement("Underline", UseUnderline),
                             new XElement("FontWeight", FontWeight),
                             new XElement("Brush", TextBrushString));                             
            return thisElement;
        }

        public static TextFormat DeSerialize(XElement xe)
        {
            var fontSize = double.Parse(xe.Element("FontSize").Value);
            var fontType = (FontType)Enum.Parse(typeof(FontType), xe.Element("FontType").Value);
            var fontStyle = xe.Element("FontStyle").Value == "Italic" ? FontStyles.Italic : FontStyles.Normal;
            var fontWeight = xe.Element("FontWeight").Value == "Bold" ? FontWeights.Bold : FontWeights.Normal;                      
            var bc = new BrushConverter();
            var brush = (SolidColorBrush)bc.ConvertFrom(xe.Element("Brush").Value);
            var useUnderline = Convert.ToBoolean(xe.Element("Underline").Value);
            return new TextFormat(fontSize, fontType, fontStyle, fontWeight, brush, useUnderline);
        }
    }
}
