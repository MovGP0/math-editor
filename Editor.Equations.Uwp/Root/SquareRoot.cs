namespace Editor
{
    public class SquareRoot : EquationContainer
    {
        protected RowContainer insideEquation;
        protected RadicalSign radicalSign;
        protected double ExtraHeight => FontSize * .15;
        protected double LeftGap => FontSize * .1;

        public SquareRoot(EquationContainer parent)
            : base(parent)
        {            
            radicalSign = new RadicalSign(this);
            ActiveChild = insideEquation = new RowContainer(this);
            childEquations.Add(insideEquation);
            childEquations.Add(radicalSign);
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
            radicalSign.Top = Top;
        }

        protected override void CalculateWidth()
        {
            Width = insideEquation.Width + radicalSign.Width + LeftGap;
        }

        protected override void CalculateHeight()
        {
            Height = insideEquation.Height + ExtraHeight;
            radicalSign.Height = Height;
            AdjustVertical();
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                radicalSign.Left = value + LeftGap;
                insideEquation.Left = radicalSign.Right;
            }
        }

        public override double RefY
        {
            get
            {
                return insideEquation.RefY + ExtraHeight;
            }
        }
    }
}
