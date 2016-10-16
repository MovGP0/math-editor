using System;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    class SignBottomTop : EquationContainer
    {
        RowContainer mainEquation;
        RowContainer topEquation;
        RowContainer bottomEquation;
        StaticSign sign;
        double HGap { get { return FontSize * .02; } }
        double VGap { get { return FontSize * .05; } }

        public SignBottomTop(EquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(parent)
        {
            ActiveChild = mainEquation = new RowContainer(this);
            SubLevel++;
            bottomEquation = new RowContainer(this);
            topEquation = new RowContainer(this);
            bottomEquation.ApplySymbolGap = false;
            topEquation.ApplySymbolGap = false;
            sign = new StaticSign(this, symbol, useUpright);
            topEquation.FontFactor = SubFontFactor;
            bottomEquation.FontFactor = SubFontFactor;
            childEquations.AddRange(new EquationBase[] { mainEquation, topEquation, bottomEquation, sign });
        }
        
        protected override void CalculateWidth()
        {
            double maxLeft = Math.Max(sign.Width, Math.Max(bottomEquation.Width, topEquation.Width));
            Width = maxLeft + mainEquation.Width + HGap;
            sign.MidX = Left + maxLeft / 2;
            topEquation.MidX = sign.MidX;
            bottomEquation.MidX = sign.MidX;
            mainEquation.Left = Math.Max(Math.Max(topEquation.Right, bottomEquation.Right), sign.Right) + HGap;
        }

        protected override void CalculateHeight()
        {
            double upperHalf = Math.Max(sign.RefY + VGap + topEquation.Height, mainEquation.RefY);
            double lowerHalf = Math.Max(sign.RefY + VGap + bottomEquation.Height, mainEquation.Height - mainEquation.RefY);
            Height = upperHalf + lowerHalf;
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                double upperHalf = Math.Max(sign.RefY + VGap + topEquation.Height, mainEquation.RefY);
                double lowerHalf = Math.Max(sign.RefY + VGap + bottomEquation.Height, mainEquation.Height - mainEquation.RefY);
                Height = upperHalf + lowerHalf;
                if (mainEquation.RefY > sign.RefY + VGap + topEquation.Height)
                {
                    mainEquation.MidY = MidY;
                    sign.MidY = MidY;
                    topEquation.Bottom = sign.Top - VGap;
                    bottomEquation.Top = sign.Bottom + VGap;
                }
                else
                {
                    topEquation.Top = Top;
                    sign.Top = topEquation.Bottom + VGap;
                    bottomEquation.Top = sign.Bottom + VGap;
                    mainEquation.MidY = sign.MidY;
                }
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (topEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = topEquation;
            }
            else if (bottomEquation.Bounds.Contains(mousePoint))
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
                double maxLeft = Math.Max(sign.Width, Math.Max(bottomEquation.Width, topEquation.Width));
                sign.MidX = value + maxLeft / 2;
                topEquation.MidX = sign.MidX;
                bottomEquation.MidX = sign.MidX;
                mainEquation.Left = Math.Max(Math.Max(topEquation.Right, bottomEquation.Right), sign.Right) + HGap;
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
                return Math.Max(sign.RefY + topEquation.Height + VGap, mainEquation.RefY);
            }
        }

        public override bool ConsumeKey(VirtualKey key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }

            switch (key)
            {
                case VirtualKey.Down:
                    if (ActiveChild == topEquation)
                    {
                        ActiveChild = mainEquation;
                        return true;
                    }

                    if (ActiveChild == mainEquation)
                    {
                        ActiveChild = bottomEquation;
                        return true;
                    }
                    break;
                case VirtualKey.Up:
                    if (ActiveChild == bottomEquation)
                    {
                        ActiveChild = mainEquation;
                        return true;
                    }

                    if (ActiveChild == mainEquation)
                    {
                        ActiveChild = topEquation;
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}