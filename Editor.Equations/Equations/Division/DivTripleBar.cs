namespace Editor
{
    public sealed class DivTripleBar : DivRegular
    {
        public DivTripleBar(IEquationContainer parent)
            : base(parent)
        {
            barCount = 3;
        }
    }
}