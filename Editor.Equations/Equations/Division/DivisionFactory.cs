namespace Editor
{
    public static class DivisionFactory
    {
        public static IEquationBase CreateEquation(EquationContainer equationParent, DivisionType divType)
        {
            switch (divType)
            {
                case DivisionType.DivRegular:
                    return new DivRegular(equationParent);
                case DivisionType.DivRegularSmall:
                    return new DivRegularSmall(equationParent);
                case DivisionType.DivDoubleBar:
                    return new DivDoubleBar(equationParent);
                case DivisionType.DivTripleBar:
                    return new DivTripleBar(equationParent);
                case DivisionType.DivHoriz:
                    return new DivHorizontal(equationParent);
                case DivisionType.DivHorizSmall:
                    return new DivHorizSmall(equationParent);
                case DivisionType.DivMath:
                    return new DivMath(equationParent);
                case DivisionType.DivMathWithTop:
                    return new DivMathWithTop(equationParent);
                case DivisionType.DivSlanted:
                    return new DivSlanted(equationParent); 
                case DivisionType.DivSlantedSmall:
                    return new DivSlantedSmall(equationParent);
                case DivisionType.DivMathInverted:
                    return new DivMathInverted(equationParent);
                case DivisionType.DivInvertedWithBottom:
                    return new DivMathWithBottom(equationParent);
                case DivisionType.DivTriangleFixed:
                    return new DivTriangle(equationParent, true);
                case DivisionType.DivTriangleExpanding:
                    return new DivTriangle(equationParent, false);
                default:
                    return null;
            }
        }
    }
}
