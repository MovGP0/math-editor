namespace Editor
{
    public class LeftBracket : Bracket
    {
        public LeftBracket(IEquationContainer parent, BracketSignType bracketType)
            : base(parent)
        {
            bracketSign = new BracketSign(this, bracketType);
            ChildEquations.AddRange(new IEquationBase[] { insideEq, bracketSign });
        }

        public override double Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;
                bracketSign.Left = value;
                insideEq.Left = bracketSign.Right;
            }
        }
    }
}
