using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Editor
{ 
    public static class  FontFactory
    {
        private static readonly Dictionary<FontType, FontFamily> FontFamilies = new Dictionary<FontType, FontFamily>();

        static FontFactory()
        {
            foreach (FontType ft in Enum.GetValues(typeof(FontType)))
            {
                FontFamilies.Add(ft, CreateFontFamily(ft));
            }
        }
        
        public static TextBlock GetFormattedText(string textToFormat, FontType fontType, double fontSize)
        {
            return GetFormattedText(textToFormat, fontType, fontSize, FontStyle.Normal, FontWeights.Normal);
        }

        public static TextBlock GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontWeight fontWeight)
        {
            return GetFormattedText(textToFormat, fontType, fontSize, FontStyle.Normal, fontWeight);
        }
        
        public static TextBlock GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontStyle fontStyle, FontWeight fontWeight)
        {
            return GetFormattedText(textToFormat, fontType, fontSize, fontStyle, fontWeight, new SolidColorBrush(Colors.Black));
        }
        
        public static TextBlock GetFormattedText(string textToFormat, FontType fontType, double fontSize, Brush brush)
        {
            return GetFormattedText(textToFormat, fontType, fontSize, FontStyle.Normal, FontWeights.Normal, brush);
        }
        
        public static TextBlock GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontStyle fontStyle, FontWeight fontWeight, Brush brush)
        {
            return new TextBlock
            {
                FontFamily = CreateFontFamily(fontType), 
                Text = textToFormat, 
                FontSize = fontSize,
                FontStyle = fontStyle, 
                FontWeight = fontWeight, 
                Foreground = brush
            };
        }

        public static FontFamily GetFontFamily(FontType fontType)
        {
            return FontFamilies.Keys.Contains(fontType) 
                ? FontFamilies[fontType] 
                : new FontFamily("Segoe UI");
        }

        private static FontFamily CreateFontFamily(FontType ft)
        {
            switch (ft)
            {
                case FontType.StixGeneral:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXGeneral#STIXGeneral"));
                case FontType.StixIntegralsD:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXIntegralsD#STIXIntegralsD"));
                case FontType.StixIntegralsSm:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXIntegralsSm#STIXIntegralsSm"));
                case FontType.StixIntegralsUp:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXIntegralsUp#STIXIntegralsUp"));
                case FontType.StixIntegralsUpD:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXIntegralsUpD#STIXIntegralsUpD"));
                case FontType.StixIntegralsUpSm:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXIntegralsUpSm#STIXIntegralsUpSm"));
                case FontType.StixNonUnicode:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXNonUnicode#STIXNonUnicode"));
                case FontType.StixSizeFiveSym:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXSizeFiveSym#STIXSizeFiveSym"));
                case FontType.StixSizeFourSym:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXSizeFourSym#STIXSizeFourSym"));
                case FontType.StixSizeOneSym:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXSizeOneSym#STIXSizeOneSym"));
                case FontType.StixSizeThreeSym:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXSizeThreeSym#STIXSizeThreeSym"));
                case FontType.StixSizeTwoSym:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXSizeTwoSym#STIXSizeTwoSym"));
                case FontType.StixVariants:
                    return new FontFamily(Global.GetPackUriString("STIX/STIXVariants#STIXVariants"));
                case FontType.Arial:
                    return new FontFamily("Arial");
                case FontType.ArialBlack:
                    return new FontFamily("Arial Black");
                case FontType.ComicSansMs:
                    return new FontFamily("Comic Sans MS");
                case FontType.Courier:
                    return new FontFamily("Courier");      
                case FontType.CourierNew:
                    return new FontFamily("Courier New");   
                case FontType.Georgia:
                    return new FontFamily("Georgia");
                case FontType.Impact:
                    return new FontFamily("Impact");
                case FontType.LucidaConsole:
                    return new FontFamily("Lucida Console");
                case FontType.LucidaSansUnicode:
                    return new FontFamily("Lucida Sans Unicode");
                case FontType.MsSerif:
                    return new FontFamily("MS Serif");
                case FontType.MsSansSerif:
                    return new FontFamily("MS Sans Serif");
                case FontType.PalatinoLinotype:
                    return new FontFamily("Palatino Linotype");
                case FontType.Segoe:
                    return new FontFamily("Segoe UI");
                case FontType.Symbol:
                    return new FontFamily("Symbol");
                case FontType.Tahoma:
                    return new FontFamily("Tahoma");
                case FontType.TimesNewRoman:
                    return new FontFamily("Times New Roman");
                case FontType.TrebuchetMs:
                    return new FontFamily("Trebuchet MS");
                case FontType.Verdana:
                    return new FontFamily("Verdana");
                case FontType.Webdings:
                    return new FontFamily("Webdings");
                case FontType.Wingdings:
                    return new FontFamily("Wingdings");
                case FontType.SystemDefault:
                    return new FontFamily("Segoe UI");
                default:
                    throw new ArgumentOutOfRangeException(nameof(ft), ft, null);
            }
        }        
    }
}