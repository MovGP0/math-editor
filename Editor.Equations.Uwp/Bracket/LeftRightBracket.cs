using System.Xml.Linq;

namespace Editor
{
    public class LeftRightBracket : Bracket
    {
        BracketSign bracketSign2;

        public LeftRightBracket(EquationContainer parent, BracketSignType leftBracketType, BracketSignType rightBracketType)
            : base(parent)
        {
            BracketSign = new BracketSign(this, leftBracketType);
            bracketSign2 = new BracketSign(this, rightBracketType);
            childEquations.AddRange(new EquationBase[] { InsideEq, BracketSign, bracketSign2 });
        }     
        
        protected override void CalculateWidth()
        {
            Width = InsideEq.Width + BracketSign.Width + bracketSign2.Width;
        }

        protected override void CalculateHeight()
        {
            base.CalculateHeight();
            bracketSign2.Height = BracketSign.Height;
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                BracketSign.Left = value;
                InsideEq.Left = BracketSign.Right;
                bracketSign2.Left = InsideEq.Right;
            }
        }

        public override double Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;
                bracketSign2.Top = BracketSign.Top;
            }
        }
    }
}
