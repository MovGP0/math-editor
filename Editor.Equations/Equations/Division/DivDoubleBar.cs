namespace Editor
{
    public sealed class DivDoubleBar : DivRegular
    {
        public DivDoubleBar(IEquationContainer parent)
            : base(parent)
        {
            barCount = 2;
        }
    }
}