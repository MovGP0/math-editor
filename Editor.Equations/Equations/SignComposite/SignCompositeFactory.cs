namespace Editor
{
    public static class SignCompositeFactory
    {
        public static IEquationBase CreateEquation(EquationContainer equationParent, Position position, SignCompositeSymbol symbol, bool useUpright)
        {
            switch (position)
            {
                case Position.None:
                    return new SignSimple(equationParent, symbol, useUpright);
                case Position.Bottom:
                    return new SignBottom(equationParent, symbol, useUpright);
                case Position.BottomAndTop:
                    return new SignBottomTop(equationParent, symbol, useUpright);
                case Position.Sub:
                    return new SignSub(equationParent, symbol, useUpright);
                case Position.SubAndSuper:
                    return new SignSubSuper(equationParent, symbol, useUpright);
                default:
                    return null;
            }
        }
    }
}
