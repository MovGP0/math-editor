namespace Editor
{
    public class DivMathWithTop : DivMathWithOuterBase
    {
        public DivMathWithTop(EquationContainer parent)
            : base(parent)
        {             
        }       

        protected override void AdjustVertical()
        {
            OuterEquation.Top = Top;
            insideEquation.Top = OuterEquation.Bottom + VerticalGap;
            divMathSign.Top = OuterEquation.Bottom + VerticalGap;
        } 
      
        public override double RefY => OuterEquation.Height + insideEquation.FirstRow.RefY + VerticalGap;
    }
}
