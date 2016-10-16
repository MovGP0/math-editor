using System;

namespace Editor
{
    public abstract class DivMathWithOuterBase : DivMath
    {
        protected RowContainer OuterEquation;

        protected DivMathWithOuterBase(EquationContainer parent)
            : base(parent)
        {
            OuterEquation = new RowContainer(this)
            {
                HAlignment = HAlignment.Right
            };
            childEquations.Add(OuterEquation);
        }
        
        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(insideEquation.Width, OuterEquation.Width) + divMathSign.Width + LeftGap;
        }

        protected override void CalculateHeight()
        {            
            Height = OuterEquation.Height + insideEquation.Height + ExtraHeight;
            divMathSign.Height = insideEquation.FirstRow.Height + ExtraHeight;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                divMathSign.Left = value + LeftGap;
                insideEquation.Right = Right;
                OuterEquation.Right = Right;
            }
        }
    }
}
