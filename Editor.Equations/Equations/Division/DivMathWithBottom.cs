namespace Editor
{
    public class DivMathWithBottom : DivMathWithOuterBase
    {
        public DivMathWithBottom(IEquationContainer parent)
            : base(parent)
        {
            divMathSign.IsInverted = true; 
        }       

        protected override void AdjustVertical()
        {
            outerEquation.Bottom = Bottom;
            insideEquation.Top = Top;
            divMathSign.Bottom = outerEquation.Top - VerticalGap;
        } 
      
        public override double RefY
        {
            get
            {
                return insideEquation.Height - (insideEquation.FirstRow.Height - insideEquation.FirstRow.RefY);
            }
        }
    }
}
