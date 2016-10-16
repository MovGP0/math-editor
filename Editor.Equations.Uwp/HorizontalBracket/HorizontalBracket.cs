using System;

namespace Editor
{
    public abstract class HorizontalBracket : EquationContainer
    {
        protected RowContainer TopEquation;
        protected HorizontalBracketSign BracketSign;
        protected RowContainer BottomEquation;

        protected HorizontalBracket(EquationContainer parent, HorizontalBracketSignType signType)
            : base(parent)
        {
            TopEquation = new RowContainer(this);
            BottomEquation = new RowContainer(this);
            BracketSign = new HorizontalBracketSign(this, signType);
            childEquations.Add(TopEquation);
            childEquations.Add(BracketSign);
            childEquations.Add(BottomEquation);            
        }
        
        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                BracketSign.MidX = MidX;
                TopEquation.MidX = MidX;
                BottomEquation.MidX = MidX;
            }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                AdjustChildrenVertical();
            }
        }

        void AdjustChildrenVertical()
        {
            TopEquation.Top = Top;
            BracketSign.Top = TopEquation.Bottom;
            BottomEquation.Top = BracketSign.Bottom;            
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(TopEquation.Width, BottomEquation.Width) + FontSize * .6;
            BracketSign.Width = Width - FontSize * .2;            
        }

        protected override void CalculateHeight()
        {
            Height = TopEquation.Height + BottomEquation.Height + BracketSign.Height;
            AdjustChildrenVertical();
        }

        public override double RefY => 
            BracketSign.SignType == HorizontalBracketSignType.TopCurly || BracketSign.SignType == HorizontalBracketSignType.ToSquare
            ? Height - BottomEquation.RefY
            : TopEquation.RefY;
    }
}
