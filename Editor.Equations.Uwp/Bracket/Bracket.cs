using System;

namespace Editor
{
    public abstract class Bracket : EquationContainer
    {
        protected RowContainer InsideEq;
        protected BracketSign BracketSign;
        protected double ExtraHeight { get; set; }

        protected Bracket(EquationContainer parent)
            : base(parent)
        {
            ExtraHeight = FontSize * 0.2;
            ActiveChild = InsideEq = new RowContainer(this);
        }

        public sealed override double FontSize
        {
            get { return base.FontSize; }
            set { base.FontSize = value; }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                InsideEq.MidY = MidY;
                BracketSign.MidY = MidY;
            }
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            Width = InsideEq.Width + BracketSign.Width;
        }

        protected override void CalculateHeight()
        {
            ExtraHeight = FontSize * 0.2;
            var upperMax = InsideEq.RefY;
            var lowerMax = InsideEq.RefYReverse;
            Height = Math.Max(upperMax, lowerMax) * 2 + ExtraHeight;
            BracketSign.Height = Height;
        }
    }
}
