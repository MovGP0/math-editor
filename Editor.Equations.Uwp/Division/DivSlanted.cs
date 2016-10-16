using System;
using System.Windows.Media;
using System.Windows;

namespace Editor
{
    public class DivSlanted : DivBase
    {
        double ExtraWidth { get { return FontSize * .1; } }
        double ExtraHeight { get { return FontSize * .1; } }

        double centerX;
        double slantXTop;
        double slantXBottom;

        public DivSlanted(EquationContainer parent)
            : base(parent, false)
        {
        }

        public DivSlanted(EquationContainer parent, bool isSmall)
            : base(parent, isSmall)
        {
        }

        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            dc.DrawLine(StandardPen, new Point(Left + centerX + slantXTop, Top), new Point(Left + centerX - slantXBottom, Bottom));            
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                TopEquation.Right = Left + centerX - ExtraWidth / 2;
                BottomEquation.Left = Left + centerX + ExtraWidth / 2;
            }
        }
        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                TopEquation.Top = this.Top;
                BottomEquation.Bottom = Bottom;
            }
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            double width = TopEquation.Width + BottomEquation.Width + ExtraWidth;
            Rect rect = new Rect(0, 0, width, Height);
            slantXTop = Math.Sin(Math.PI / 5) * (TopEquation.Height + ExtraHeight/2);
            slantXBottom = Math.Sin(Math.PI / 5) * (BottomEquation.Height + ExtraHeight/2);
            rect.Union(new Point(TopEquation.Width + slantXTop + ExtraWidth/2, Top));
            rect.Union(new Point(BottomEquation.Width + slantXBottom + ExtraWidth/2, Bottom));            
            Width = rect.Width;
            centerX = rect.Width - Math.Max(slantXTop, BottomEquation.Width) - ExtraWidth/2;
        }

        protected override void CalculateHeight()
        {
            Height = TopEquation.Height + BottomEquation.Height + ExtraHeight;
        }

        //public override double RefY
        //{
        //    get
        //    {
        //        return topEquation.Height + ExtraHeight / 2;
        //    }
        //}
    }
}
