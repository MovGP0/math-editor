namespace Editor
{
    public class DivMathWithBottom : DivMathWithOuterBase
    {
        public DivMathWithBottom(EquationContainer parent)
            : base(parent)
        {
            divMathSign.IsInverted = true; 
        }       

        protected override void AdjustVertical()
        {
            OuterEquation.Bottom = Bottom;
            insideEquation.Top = Top;
            divMathSign.Bottom = OuterEquation.Top - VerticalGap;
        } 
      
        public override double RefY => insideEquation.Height - (insideEquation.FirstRow.Height - insideEquation.FirstRow.RefY);
    }
}
