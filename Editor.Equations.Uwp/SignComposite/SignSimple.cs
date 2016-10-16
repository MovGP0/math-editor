using System;

namespace Editor
{
    class SignSimple : EquationContainer
    {
        protected RowContainer mainEquation;
        protected StaticSign sign;            

        public SignSimple(EquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(parent)
        {
            ActiveChild = mainEquation = new RowContainer(this);
            sign = new StaticSign(this, symbol, useUpright);  
            childEquations.AddRange(new EquationBase[] {mainEquation, sign});            
        }
        
        protected override void CalculateWidth()
        {
            Width = sign.Width + mainEquation.Width;
        }

        protected override void CalculateHeight()
        {
            Height = Math.Max(sign.RefY, mainEquation.RefY) + Math.Max(sign.RefY, mainEquation.Height - mainEquation.RefY);
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                sign.MidY = MidY;
                mainEquation.MidY = MidY;
            }
        }
        
        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                sign.Left = value;
                mainEquation.Left = sign.Right;
            }
        }

        public override double RefY => Math.Max(sign.RefY, mainEquation.RefY);
    }
}