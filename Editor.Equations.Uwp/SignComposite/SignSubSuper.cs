using System;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;

namespace Editor
{
    class SignSubSuper : EquationContainer
    {
        RowContainer mainEquation;
        StaticSign sign;
        RowContainer superEquation;
        RowContainer subEquation;        
        double HGap { get { return FontSize * .06; } }
        double SubMinus = 0;
        double SuperMinus = 0;

        public SignSubSuper(EquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(parent)
        {
            ActiveChild = mainEquation = new RowContainer(this);
            SubLevel++;
            subEquation = new RowContainer(this);
            superEquation = new RowContainer(this);
            subEquation.ApplySymbolGap = false;
            superEquation.ApplySymbolGap = false;
            sign = new StaticSign(this, symbol, useUpright);
            subEquation.FontFactor = SubFontFactor;
            superEquation.FontFactor = SubFontFactor;
            childEquations.AddRange(new EquationBase[] { mainEquation, sign, superEquation, subEquation });
        }
        
        protected override void CalculateWidth()
        {            
            if (sign.Symbol.ToString().ToLower().Contains("integral"))
            {
                SubMinus = sign.OverhangTrailing;
                SuperMinus = sign.OverhangLeading + (sign.UseItalicIntegralSign ? FontSize * .1 : 0);
            }
            Width = sign.Width + Math.Max(subEquation.Width + SubMinus, superEquation.Width + SuperMinus) + mainEquation.Width + HGap;
        }

        protected override void CalculateHeight()
        {
            Height = Math.Max(sign.Height * .5 + subEquation.Height + superEquation.Height, mainEquation.Height);
        }
        
        public override double RefY => Math.Max(superEquation.Height + sign.Height * .3, mainEquation.RefY);

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                sign.MidY = MidY;
                mainEquation.MidY = MidY;
                subEquation.Top = sign.Bottom - sign.Height * .3;
                superEquation.Bottom = sign.Top + sign.Height * .2;
            }
        }
        
        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (mainEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = mainEquation;
            }
            else if (superEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = superEquation;
            }
            else 
            {
                ActiveChild = subEquation;
            }
            return ActiveChild.ConsumeMouseClick(mousePoint);
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                sign.Left = value;
                subEquation.Left = sign.Right + SubMinus;
                superEquation.Left = sign.Right + SuperMinus;
                mainEquation.Left = Math.Max(subEquation.Right, superEquation.Right) + HGap;
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
                    if (ActiveChild == superEquation)
                    {
                        ActiveChild = mainEquation;
                        return true;    
                    }

                    if (ActiveChild == mainEquation)
                    {
                        ActiveChild = subEquation;
                        return true;
                    }
                    break;
                case VirtualKey.Up:
                    if (ActiveChild == subEquation)
                    {
                        ActiveChild = mainEquation;
                        return true;
                    }

                    if (ActiveChild == mainEquation)
                    {
                        ActiveChild = superEquation;
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}