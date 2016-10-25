namespace Editor
{
    public sealed class TopBracket : HorizontalBracket
    {
        public TopBracket(IEquationContainer parent, HorizontalBracketSignType signType)
             :base (parent, signType)
        {
            topEquation.FontFactor = SubFontFactor;
            ActiveChild = bottomEquation;
        }
    }
}
