namespace Editor
{
    public class RightBracket : Bracket
    {
        public RightBracket(EquationContainer parent, BracketSignType bracketType)
            : base(parent)
        {
            BracketSign = new BracketSign(this, bracketType);
            childEquations.AddRange(new EquationBase[] { InsideEq, BracketSign });
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                InsideEq.Left = value;
                BracketSign.Left = InsideEq.Right;
            }
        }
    }
}
