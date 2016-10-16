using System;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    class SignBottom : EquationContainer
    {
        RowContainer mainEquation;
        RowContainer bottomEquation;
        StaticSign sign;
        double HGap => FontSize * .02;
        double VGap => FontSize * .05;

        public SignBottom(EquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(parent)
        {
            ActiveChild = mainEquation = new RowContainer(this);
            SubLevel++;
            bottomEquation = new RowContainer(this);
            bottomEquation.ApplySymbolGap = false;
            sign = new StaticSign(this, symbol, useUpright);
            bottomEquation.FontFactor = SubFontFactor;
            childEquations.AddRange(new EquationBase[] { mainEquation, bottomEquation, sign });
        }
        
        protected override void CalculateWidth()
        {
            double maxLeft = Math.Max(sign.Width, bottomEquation.Width);
            Width = maxLeft + mainEquation.Width + HGap;
            sign.MidX = Left + maxLeft / 2;
            bottomEquation.MidX = sign.MidX;
            mainEquation.Left = Math.Max(bottomEquation.Right, sign.Right) + HGap;
        }

        protected override void CalculateHeight()
        {
            double upperHalf = Math.Max(mainEquation.RefY, sign.RefY);
            double lowerHalf = Math.Max(sign.RefY + VGap + bottomEquation.Height, mainEquation.Height - mainEquation.RefY);
            Height = upperHalf + lowerHalf;
            AdjustVertical();
        }

        void AdjustVertical()
        {
            if (mainEquation.RefY > sign.RefY)
            {
                sign.MidY = MidY;
                mainEquation.MidY = MidY;
                bottomEquation.Top = sign.Bottom + VGap;
            }
            else
            {
                bottomEquation.Bottom = Bottom;
                sign.Bottom = bottomEquation.Top - VGap;
                mainEquation.MidY = sign.MidY;
            }
        }

        public override double Top
        {
            get
            {
                return base.Top;
            }
            set
            {
                base.Top = value;
                AdjustVertical();
            }
        }


        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (bottomEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = bottomEquation;
            }
            else
            {
                ActiveChild = mainEquation;
            }
            return ActiveChild.ConsumeMouseClick(mousePoint);
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                double maxLeft = Math.Max(sign.Width, bottomEquation.Width);
                sign.MidX = value + maxLeft / 2;
                bottomEquation.MidX = sign.MidX;
                mainEquation.Left = Math.Max(bottomEquation.Right, sign.Right) + HGap;
            }
        }

        public override double Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;                
            }
        }

        public override double RefY
        {
            get
            {
                return Math.Max(sign.RefY, mainEquation.RefY);
            }
        }

        public override bool ConsumeKey(VirtualKey key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            if (key == VirtualKey.Down)
            {
                if (ActiveChild == mainEquation)
                {
                    ActiveChild = bottomEquation;
                    return true;
                }
            }
            else if (key == VirtualKey.Up)
            {
                if (ActiveChild == bottomEquation)
                {
                    ActiveChild = mainEquation;
                    return true;
                }
            }
            return false;
        }
    }
}