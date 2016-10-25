using System.Windows.Media;

namespace Editor
{
    public interface IRowContainer : ISupportsUndo, IEquationContainer
    {
        void DrawVisibleRows(DrawingContext dc, double top, double bottom);
    }
}