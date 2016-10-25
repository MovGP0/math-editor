using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Xml.Linq;

namespace Editor
{
    //Only those EquationBase classes should use it which are able to remeber the formats (as of May 15, 2013 only TextEquation)!!
    public class TextManager
    {
        public List<TextFormat> formattingList = new List<TextFormat>();
        List<TextDecorationCollection> decorations = new List<TextDecorationCollection>();
        Dictionary<int, int> mapping = new Dictionary<int, int>();
        List<TextFormat> formattingListBeforeSave;

        public TextManager()
        {
            var tdc = new TextDecorationCollection {TextDecorations.Underline};
            decorations.Add(tdc);
        }

        public void OptimizeForSave(EquationRoot root)
        {
            mapping.Clear();
            var newList = new List<TextFormat>();
            var usedOnes = root.GetUsedTextFormats();                 
            foreach (var i in usedOnes)
            {
                var tf = formattingList[i];
                tf.Index = newList.Count;
                newList.Add(tf);
                mapping.Add(i, tf.Index);
            }
            root.ResetTextFormats(mapping);
            formattingListBeforeSave = formattingList;
            formattingList = newList;
        }

        public void RestoreAfterSave(EquationRoot root)
        {   
            var oldMapping = new Dictionary<int, int>();
            foreach (var i in mapping.Keys)
            {
                oldMapping.Add(mapping[i], i);
                var tf = formattingListBeforeSave[i];
                tf.Index = i;
            }            
            root.ResetTextFormats(oldMapping);
            formattingList = formattingListBeforeSave;
        }

        public XElement Serialize()
        {            
            var thisElement = new XElement(GetType().Name);
            var children = new XElement("Formats");
            foreach (var tf in formattingList)
            {
                children.Add(tf.Serialize());
            }
            thisElement.Add(children);
            return thisElement;
        }

        public void AddToList(TextFormat tf)
        {
            tf.Index = formattingList.Count;
            formattingList.Add(tf);
        }

        public void DeSerialize(XElement xElement)
        {
            formattingList.Clear();
            var children = xElement.Element("Formats");
            foreach (var xe in children.Elements())
            {                
                AddToList(TextFormat.DeSerialize(xe));
            }
        }

        public int GetFormatId(double fontSize, FontType fontType, FontStyle fontStyle, FontWeight fontWeight, SolidColorBrush textBrush, bool useUnderline)
        {
            var num = Math.Round(fontSize, 1);
            var tf = formattingList.Where(x =>
            {
                return x.FontSize == Math.Round(fontSize, 1) &&
                       x.FontType == fontType &&
                       x.FontStyle == fontStyle &&
                       x.UseUnderline == useUnderline &&
                       Color.AreClose(x.TextBrush.Color, textBrush.Color) &&
                       x.FontWeight == fontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(fontSize, fontType, fontStyle, fontWeight, textBrush, useUnderline);
                AddToList(tf);
            } 
            return tf.Index;
        }

        public int GetFormatIdForNewFont(int oldId, FontType fontType)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == oldFormat.FontSize &&
                       x.FontType == fontType &&
                       x.FontStyle == oldFormat.FontStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == oldFormat.FontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(oldFormat.FontSize, fontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }
        
        public int GetFormatIdForNewSize(int oldId, double newSize)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == Math.Round(newSize, 1) &&
                       x.FontType == oldFormat.FontType &&
                       x.FontStyle == oldFormat.FontStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == oldFormat.FontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(newSize, oldFormat.FontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewStyle(int oldId, FontStyle newStyle)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == oldFormat.FontSize &&
                       x.FontType == oldFormat.FontType &&
                       x.FontStyle == newStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == oldFormat.FontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, newStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewWeight(int oldId, FontWeight newWeight)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == oldFormat.FontSize &&
                       x.FontType == oldFormat.FontType &&
                       x.FontStyle == oldFormat.FontStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == newWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle, newWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewUnderline(int oldId, bool newUnderline)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList
                .Where(x => Math.Abs(x.FontSize - oldFormat.FontSize) < double.Epsilon )
                .Where(x => x.FontType == oldFormat.FontType )
                .Where(x => x.FontStyle == oldFormat.FontStyle )
                .Where(x => x.UseUnderline == newUnderline )
                .Where(x => Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color))
                .FirstOrDefault(x => x.FontWeight == oldFormat.FontWeight);

            if (tf != null) return tf.Index;

            tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, newUnderline);
            AddToList(tf);
            return tf.Index;
        }

        public FormattedText GetFormattedText(string text, List<int> formats)
        {
            var formattedText = new FormattedText(text,
                                    CultureInfo.InvariantCulture,
                                    FlowDirection.LeftToRight,
                                    formattingList[formats[0]].TypeFace,
                                    formattingList[formats[0]].FontSize,
                                    formattingList[formats[0]].TextBrush);
            for (var i = 0; i < formats.Count; i++)
            {
                FormatText(formats, formattedText, i);
            }
            return formattedText;
        }

        private void FormatText(IList<int> formats, FormattedText formattedText, int i)
        {
            formattedText.SetFontFamily(formattingList[formats[i]].FontFamily, i, 1);
            formattedText.SetFontSize(formattingList[formats[i]].FontSize, i, 1);
            formattedText.SetFontStyle(formattingList[formats[i]].FontStyle, i, 1);
            formattedText.SetFontWeight(formattingList[formats[i]].FontWeight, i, 1);
            formattedText.SetForegroundBrush(formattingList[formats[i]].TextBrush, i, 1);
            if (formattingList[formats[i]].UseUnderline)
            {
                formattedText.SetTextDecorations(decorations[0], i, 1);
            }
        }

        public bool IsBold(int formatId)
        {
            return formattingList[formatId].FontWeight == FontWeights.Bold;
        }

        public bool IsItalic(int formatId)
        {
            return formattingList[formatId].FontStyle == FontStyles.Italic;
        }

        public bool IsUnderline(int formatId)
        {
            return formattingList[formatId].UseUnderline;
        }

        public FontType GetFontType(int formatId)
        {
            return formattingList[formatId].FontType;
        }

        public FormattedText GetFormattedText(string text, int format)
        {
            var formattedText = new FormattedText(text,
                                        CultureInfo.InvariantCulture,
                                        FlowDirection.LeftToRight,
                                        formattingList[format].TypeFace,
                                        formattingList[format].FontSize,
                                        formattingList[format].TextBrush);
            
            formattedText.SetFontStyle(formattingList[format].FontStyle);
            formattedText.SetFontWeight(formattingList[format].FontWeight);            
            return formattedText;
        }

        public double GetBaseline(int formatId)
        {
            return formattingList[formatId].FontFamily.Baseline;
        }

        public double GetLineSpacing(int formatId)
        {
            return formattingList[formatId].FontFamily.LineSpacing;
        }

        public double GetFontHeight(int formatId)
        {
            double fontDpiSize = 16;
            return fontDpiSize * formattingList[formatId].FontFamily.LineSpacing;
        }
    }
}
