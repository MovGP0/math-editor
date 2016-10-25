using System.Windows.Media;

namespace Editor
{
    public static class EquationRowShared
    {
        public static Pen BoxPen { get; }

        static EquationRowShared()
        {
            BoxPen = new Pen(Brushes.Blue, 1.1)
            {
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat,
                DashStyle = DashStyles.Dash
            };
            BoxPen.Freeze();
        }
        
        public static bool UseItalicIntergalOnNew { get; set; }

    }
}