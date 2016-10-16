using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Windows.System;

namespace Editor
{
    public class DivHorizontal : DivBase
    {
        double ExtraWidth { get { return FontSize * .3; } }

        public DivHorizontal(EquationContainer parent)
            : base(parent, false)
        {
        }

        public DivHorizontal(EquationContainer parent, bool isSmall)
            : base (parent, isSmall)
        {  
        }
        
        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            dc.DrawLine(StandardPen, new Point(BottomEquation.Left - ExtraWidth/10, Top), new Point(TopEquation.Right + ExtraWidth/10, Bottom));   
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

            TopEquation.Left = this.Left;
            BottomEquation.Left = TopEquation.Right + ExtraWidth;
        }
        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                TopEquation.MidY = MidY;
                BottomEquation.MidY = MidY;
            }
        }

        public override double RefY
        {
            get
            {
                return Math.Max(TopEquation.RefY, BottomEquation.RefY);
            }
        }


        protected override void CalculateWidth()
        {            
            Width = TopEquation.Width + BottomEquation.Width + ExtraWidth;
            AdjustHorizontal();
        }

        protected override void CalculateHeight()
        {            
            Height = Math.Max(TopEquation.Height , BottomEquation.Height);            
        }

        public override bool ConsumeKey(VirtualKey key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            if (key == VirtualKey.Right)
            {
                if (ActiveChild == TopEquation)
                {
                    ActiveChild = BottomEquation;
                    return true;
                }
            }
            else if (key == VirtualKey.Left)
            {
                if (ActiveChild == BottomEquation)
                {
                    ActiveChild = TopEquation;
                    return true;
                }
            }
            return false;
        }
    }
}
