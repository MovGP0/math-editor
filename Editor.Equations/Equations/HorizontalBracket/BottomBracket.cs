namespace Editor
{
    public sealed class BottomBracket : HorizontalBracket
    {
        public BottomBracket(IEquationContainer parent, HorizontalBracketSignType signType)
             :base (parent, signType)
        {
            bottomEquation.FontFactor = SubFontFactor;
            ActiveChild = topEquation;
        }
    }
}
