using System;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    public class nRoot : EquationContainer
    {
        protected RowContainer insideEquation;
        RowContainer nthRootEquation;
        protected RadicalSign radicalSign;
        protected double ExtraHeight => FontSize * .15;

        double HGap => FontSize * .5;
        double LeftPadding => FontSize * .1;

        public nRoot(EquationContainer parent)
            : base(parent)
        {
            radicalSign = new RadicalSign(this);
            ActiveChild = insideEquation = new RowContainer(this);
            nthRootEquation = new RowContainer(this);
            nthRootEquation.ApplySymbolGap = false;
            nthRootEquation.FontFactor = SubFontFactor;
            childEquations.AddRange(new EquationBase[] { insideEquation, radicalSign, nthRootEquation });
        }
        
        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (nthRootEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = nthRootEquation;
            }
            else if (insideEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = insideEquation;
            }
            return ActiveChild.ConsumeMouseClick(mousePoint);
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                AdjustVertical();
            }
        }

        private void AdjustVertical()
        {
            insideEquation.Bottom = Bottom;
            radicalSign.Bottom = Bottom;
            nthRootEquation.Bottom = radicalSign.MidY - FontSize * .05;
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(nthRootEquation.Width + HGap, radicalSign.Width) + insideEquation.Width + LeftPadding;            
        }

        protected override void CalculateHeight()
        {
            Height = insideEquation.Height + Math.Max(0, nthRootEquation.Height - insideEquation.Height / 2 + FontSize * .05) + ExtraHeight;
        }

        public override double Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                radicalSign.Height = insideEquation.Height + ExtraHeight;
                AdjustVertical();
            }
        }

        public override double RefY => insideEquation.RefY + Math.Max(0, nthRootEquation.Height - insideEquation.Height / 2 + FontSize * .05) + ExtraHeight;

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;                
                if (nthRootEquation.Width + HGap > radicalSign.Width)
                {
                    nthRootEquation.Left = Left + LeftPadding;
                    radicalSign.Right = nthRootEquation.Right + HGap;
                }
                else
                {
                    radicalSign.Left = Left + LeftPadding;
                    nthRootEquation.Right = radicalSign.Right - HGap;
                }
                insideEquation.Left = radicalSign.Right;
            }
        }
        public override bool ConsumeKey(VirtualKey key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            if (key == VirtualKey.Left)
            {
                if (ActiveChild == insideEquation)
                {
                    ActiveChild = nthRootEquation;
                    return true;
                }
            }
            else if (key == VirtualKey.Right)
            {
                if (ActiveChild == nthRootEquation)
                {
                    ActiveChild = insideEquation;
                    return true;
                }
            }
            return false;
        }
    }
}
