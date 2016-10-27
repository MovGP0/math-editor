using System.Windows.Media;

namespace Editor
{
    public static class EquationBaseCommon
    {
        private const double DefaultFontSize = 20;
        public static TextManager TextManager = new TextManager();
        public static double LineFactor = 0.06;
        public static double LineThickness => DefaultFontSize * LineFactor;
        public static double ThinLineThickness => DefaultFontSize * LineFactor * 0.7;
        public static Pen StandardPen => PenManager.GetPen(LineThickness);
        public static Pen ThinPen => PenManager.GetPen(ThinLineThickness);
        public static Pen StandardMiterPen => PenManager.GetPen(LineThickness, PenLineJoin.Miter);
        public static Pen ThinMiterPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Miter);
        public static Pen StandardRoundPen => PenManager.GetPen(LineThickness, PenLineJoin.Round);
        public static Pen ThinRoundPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Round);
        public static double SubFontFactor = 0.6;
        public static double SubSubFontFactor = 0.7;
        public static Brush DebugBrush;
    }
}