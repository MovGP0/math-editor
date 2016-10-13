using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace Editor
{ 
    public class FontFactory
    {
        private FontFactory() { }
        private static readonly Dictionary<FontType, FontFamily> FontFamilies = new Dictionary<FontType, FontFamily>();

        static FontFactory()
        {
            foreach (FontType ft in Enum.GetValues(typeof(FontType)))
            {
                FontFamilies.Add(ft, CreateFontFamily(ft));
            }
        }
        
        public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize)
        {
            return GetFormattedText(textToFormat, fontType, fontSize, FontStyles.Normal, FontWeights.Normal);
        }

        public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontWeight fontWeight)
        {
            return GetFormattedText(textToFormat, fontType, fontSize, FontStyles.Normal, fontWeight);
        }
        
        public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontStyle fontStyle, FontWeight fontWeight)
        {
            return GetFormattedText(textToFormat, fontType, fontSize, fontStyle, fontWeight, Brushes.Black);
        }

        public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, Brush brush)
        {
            return GetFormattedText(textToFormat, fontType, fontSize, FontStyles.Normal, FontWeights.Normal, brush);
        }


        public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontStyle fontStyle, FontWeight fontWeight, Brush brush)
        {
            var typeface = GetTypeface(fontType, fontStyle, fontWeight);
            return new FormattedText(textToFormat, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, typeface, fontSize, brush);
        }

        public static FontFamily GetFontFamily(FontType fontType)
        {
            if (FontFamilies.Keys.Contains(fontType))
            {
                return FontFamilies[fontType];
            }
            else
            {
                return new FontFamily("Segoe UI");
            }
        }

        private static FontFamily CreateFontFamily(FontType ft)
        {
            switch (ft)
            {
                case FontType.StixGeneral:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXGeneral");
                case FontType.StixIntegralsD:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXIntegralsD");
                case FontType.StixIntegralsSm:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXIntegralsSm");
                case FontType.StixIntegralsUp:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXIntegralsUp");
                case FontType.StixIntegralsUpD:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXIntegralsUpD");
                case FontType.StixIntegralsUpSm:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXIntegralsUpSm");
                case FontType.StixNonUnicode:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXNonUnicode");
                case FontType.StixSizeFiveSym:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXSizeFiveSym");
                case FontType.StixSizeFourSym:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXSizeFourSym");
                case FontType.StixSizeOneSym:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXSizeOneSym");
                case FontType.StixSizeThreeSym:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXSizeThreeSym");
                case FontType.StixSizeTwoSym:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXSizeTwoSym");
                case FontType.StixVariants:
                    return new FontFamily(new Uri("pack://application:,,,/STIX/"), "./#STIXVariants");
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
            }
            return new FontFamily("Segoe UI");
        }

        public static Typeface GetTypeface(FontType fontType, FontStyle fontStyle, FontWeight fontWeight)
        {
            return new Typeface(GetFontFamily(fontType), fontStyle, fontWeight, FontStretches.Normal, GetFontFamily(FontType.StixGeneral));
        }        
    }
}