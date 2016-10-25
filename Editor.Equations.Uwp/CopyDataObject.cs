using System.Xml.Linq;
using Windows.UI.Xaml.Media.Imaging;

namespace Editor
{
    public sealed class CopyDataObject
    {
        public BitmapSource Image { get; set; }
        public string Text { get; set; }
        public XElement XElement { get; set; }
    }
}