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
            XElement[] formatElements = rootXE.Element(typeof(TextManager).Name).Elements("Formats").Elements().ToArray();
            IEnumerable<XElement> formats = rootXE.Descendants(typeof(TextEquation).Name).Descendants("Formats");
            Dictionary<int, int> allFormatIds = new Dictionary<int, int>();
            foreach (XElement xe in formats)
            {
                if (xe.Value.Length > 0)
                {
                    string[] formatStrings = xe.Value.Split(',');
                    foreach (string s in formatStrings)
                    {
                        int id = int.Parse(s);
                        if (!allFormatIds.Keys.Contains(id))
                        {
                            allFormatIds.Add(id, id);
                        }
                    }
                }
            }
            for (int i=0;i < allFormatIds.Count;i++)
            {
                int key = allFormatIds.ElementAt(i).Key;
                TextFormat tf = TextFormat.DeSerialize(formatElements[key]);
                TextFormat match = manager.formattingList.Where(x =>
                {
                    return x.FontSize == Math.Round(tf.FontSize, 1) &&
                           x.FontType == tf.FontType &&
                           x.FontStyle == tf.FontStyle &&
                           x.UseUnderline == tf.UseUnderline &&
                           Color.AreClose(x.TextBrush.Color, tf.TextBrush.Color) &&
                           x.FontWeight == tf.FontWeight;

                }).FirstOrDefault();

                int newValue = 0;
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
            IEnumerable<XElement> textElements = rootXE.Descendants(typeof(TextEquation).Name);
            foreach (XElement xe in textElements)
            {
                XElement formatsElement = xe.Elements("Formats").FirstOrDefault();
                if (formatsElement != null)
                {
                    StringBuilder strBuilder = new StringBuilder();
                    string[] formatStrings = formatsElement.Value.Split(',');
                    foreach (string s in formatStrings)
                    {
                        if (s.Length > 0)
                        {
                            int id = int.Parse(s);
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