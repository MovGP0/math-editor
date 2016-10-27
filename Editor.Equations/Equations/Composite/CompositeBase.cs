namespace Editor
{
    public abstract class CompositeBase : EquationContainer
    {
        protected RowContainer mainRowContainer;
        protected double bottomGap;
        protected double SubOverlap => FontSize * .4;
        protected double SuperOverlap => FontSize * 0.32;

        protected CompositeBase(IEquationContainer parent)
            : base(parent)
        {
            ActiveChild = mainRowContainer = new RowContainer(this);
            DetermineBottomGap();           
        }

        private void DetermineBottomGap()
        {
            bottomGap = FontSize / 20;
        }

        public override double FontSize
        {
            get
            {
                return base.FontSize;
            }
            set
            {
                base.FontSize = value;
                DetermineBottomGap();
            }
        }

        public void ChangeMainEquationSize(double newFontSizeFactor)
        {
            mainRowContainer.FontFactor = newFontSizeFactor;
        }
    }
}
