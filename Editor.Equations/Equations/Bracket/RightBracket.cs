namespace Editor
{
    public class RightBracket : Bracket
    {
        public RightBracket(IEquationContainer parent, BracketSignType bracketType)
            : base(parent)
        {
            bracketSign = new BracketSign(this, bracketType);
            ChildEquations.AddRange(new EquationBase[] { insideEq, bracketSign });
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                insideEq.Left = value;
                bracketSign.Left = insideEq.Right;
            }
        }
    }
}
