namespace Editor
{
    public class LeftBracket : Bracket
    {
        public LeftBracket(EquationContainer parent, BracketSignType bracketType)
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
                BracketSign.Left = value;
                InsideEq.Left = BracketSign.Right;
            }
        }
    }
}
