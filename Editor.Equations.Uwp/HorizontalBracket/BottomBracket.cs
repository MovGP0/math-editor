namespace Editor
{
    public class BottomBracket : HorizontalBracket
    {
        public BottomBracket(EquationContainer parent, HorizontalBracketSignType signType)
             :base (parent, signType)
        {
            BottomEquation.FontFactor = SubFontFactor;
            ActiveChild = TopEquation;
        }
    }
}
