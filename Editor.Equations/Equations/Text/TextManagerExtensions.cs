using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;

namespace Editor
{
    public static class TextManagerExtensions
    {
        public static void ProcessPastedXML(this TextManager manager, XElement rootXE)
        {
            var formatElements = rootXE.Element(typeof(TextManager).Name).Elements("Formats").Elements().ToArray();
            var formats = rootXE.Descendants(typeof(TextEquation).Name).Descendants("Formats");
            var allFormatIds = new Dictionary<int, int>();

            foreach (var xe in formats)
            {
                if (xe.Value.Length > 0)
                {
                    var formatStrings = xe.Value.Split(',');
                    foreach (var s in formatStrings)
                    {
                        var id = int.Parse(s);
                        if (!allFormatIds.Keys.Contains(id))
                        {
                            allFormatIds.Add(id, id);
                        }
                    }
                }
            }
            for (var i=0;i < allFormatIds.Count;i++)
            {
                var key = allFormatIds.ElementAt(i).Key;
                var tf = TextFormat.DeSerialize(formatElements[key]);
                var match = manager.formattingList
                    .Where(x => Math.Abs(x.FontSize - Math.Round(tf.FontSize, 1)) < double.Epsilon )
                    .Where(x => x.FontType == tf.FontType )
                    .Where(x => x.FontStyle == tf.FontStyle )
                    .Where(x => x.UseUnderline == tf.UseUnderline )
                    .Where(x => Color.AreClose(x.TextBrush.Color, tf.TextBrush.Color))
                    .FirstOrDefault(x => x.FontWeight == tf.FontWeight);

                int newValue;
                if (match == null)
                {
                    manager.AddToList(tf);
                    newValue = tf.Index;
                }
                else
                {
                    newValue = match.Index;
                }
                allFormatIds[key] = newValue;
            }
            var textElements = rootXE.Descendants(typeof(TextEquation).Name);
            foreach (var xe in textElements)
            {
                var formatsElement = xe.Elements("Formats").FirstOrDefault();
                if (formatsElement != null)
                {
                    var strBuilder = new StringBuilder();
                    var formatStrings = formatsElement.Value.Split(',');
                    foreach (var s in formatStrings)
                    {
                        if (s.Length > 0)
                        {
                            var id = int.Parse(s);
                            strBuilder.Append(allFormatIds[id] + ",");
                        }
                    }
                    if (strBuilder.Length > 0)
                    {
                        strBuilder.Remove(strBuilder.Length - 1, 1);
                    }
                    formatsElement.Value = strBuilder.ToString();
                }                
            }
        }

    }
}