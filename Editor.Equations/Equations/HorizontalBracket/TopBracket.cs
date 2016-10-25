namespace Editor
{
    public class TopBracket : HorizontalBracket
    {
        public TopBracket(IEquationContainer parent, HorizontalBracketSignType signType)
             :base (parent, signType)
        {
            topEquation.FontFactor = SubFontFactor;
            ActiveChild = bottomEquation;
        }
    }
}
