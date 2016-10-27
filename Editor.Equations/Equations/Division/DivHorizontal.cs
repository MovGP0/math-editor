using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace Editor
{
    public class DivHorizontal : DivBase
    {
        private double ExtraWidth => FontSize * .3;

        public DivHorizontal(IEquationContainer parent)
            : base(parent)
        {
        }

        public DivHorizontal(IEquationContainer parent, bool isSmall)
            : base (parent, isSmall)
        {  
        }
        
        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            dc.DrawLine(StandardPen, new Point(bottomEquation.Left - ExtraWidth/10, Top), new Point(topEquation.Right + ExtraWidth/10, Bottom));   
        }     

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                AdjustHorizontal();
            }
        }

        private void AdjustHorizontal()
        {

            topEquation.Left = Left;
            bottomEquation.Left = topEquation.Right + ExtraWidth;
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                topEquation.MidY = MidY;
                bottomEquation.MidY = MidY;
            }
        }

        public override double RefY => Math.Max(topEquation.RefY, bottomEquation.RefY);
        
        public override void CalculateWidth()
        {            
            Width = topEquation.Width + bottomEquation.Width + ExtraWidth;
            AdjustHorizontal();
        }

        public override void CalculateHeight()
        {            
            Height = Math.Max(topEquation.Height , bottomEquation.Height);            
        }

        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }

            switch (key)
            {
                case Key.Right:
                    if (ActiveChild == topEquation)
                    {
                        ActiveChild = bottomEquation;
                        return true;
                    }
                    return false;
                case Key.Left:
                    if (ActiveChild == bottomEquation)
                    {
                        ActiveChild = topEquation;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
    }
}
