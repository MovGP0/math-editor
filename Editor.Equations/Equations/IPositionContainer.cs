using System.Windows;

namespace Editor
{
    public interface IPositionContainer
    {
        double Width { get; set; }
        double Height { get; set; }
        double Left { get; set; }
        double Top { get; set; }
        double Right { get; set; }
        double Bottom { get; set; }
        double MidY { get; set; }
        double MidX { get; set; }
        Point Location { get; set; }
        Size Size { get; }
    }
}