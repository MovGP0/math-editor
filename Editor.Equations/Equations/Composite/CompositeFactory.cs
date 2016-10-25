namespace Editor
{
    public static class CompositeFactory
    {
        public static IEquationBase CreateEquation(IEquationContainer equationParent, Position position)
        {
            switch (position)
            {                
                case Position.Bottom:
                    return new CompositeBottom(equationParent);
                case Position.Top:
                    return new CompositeTop(equationParent);
                case Position.BottomAndTop:
                    return new CompositeBottomTop(equationParent);
                default:
                    return null;
            }
        }
    }
}
