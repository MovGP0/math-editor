namespace Editor
{
    public class TopBracket : HorizontalBracket
    {
        public TopBracket(EquationContainer parent, HorizontalBracketSignType signType)
             :base (parent, signType)
        {
            TopEquation.FontFactor = SubFontFactor;
            ActiveChild = BottomEquation;
        }
    }
}
