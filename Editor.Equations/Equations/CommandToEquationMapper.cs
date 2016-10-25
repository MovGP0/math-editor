namespace Editor
{
    public sealed class CommandToEquationMapper
    {
        public static IEquationBase Execute(IEquationRow container, CommandType commandType, object data)
        {
            switch (commandType)
            {
                case CommandType.Composite:
                    return CompositeFactory.CreateEquation(container, (Position) data);
                case CommandType.CompositeBig:
                    return BigCompositeFactory.CreateEquation(container, (Position) data);
                case CommandType.Division:
                    return DivisionFactory.CreateEquation(container, (DivisionType) data);
                case CommandType.SquareRoot:
                    return new SquareRoot(container);
                case CommandType.NRoot:
                    return new NRoot(container);
                case CommandType.LeftBracket:
                    return new LeftBracket(container, (BracketSignType) data);
                case CommandType.RightBracket:
                    return new RightBracket(container, (BracketSignType) data);
                case CommandType.LeftRightBracket:
                    return new LeftRightBracket(container, ((BracketSignType[]) data)[0], ((BracketSignType[]) data)[1]);
                case CommandType.Sub:
                    return new Sub(container, (Position) data);
                case CommandType.Super:
                    return new Super(container, (Position) data);
                case CommandType.SubAndSuper:
                    return new SubAndSuper(container, (Position) data);
                case CommandType.TopBracket:
                    return new TopBracket(container, (HorizontalBracketSignType) data);
                case CommandType.BottomBracket:
                    return new BottomBracket(container, (HorizontalBracketSignType) data);
                case CommandType.DoubleArrowBarBracket:
                    return new DoubleArrowBarBracket(container);
                case CommandType.SignComposite:
                    return SignCompositeFactory.CreateEquation(container, (Position) (((object[]) data)[0]), (SignCompositeSymbol) (((object[]) data)[1]), EquationRowShared.UseItalicIntergalOnNew);
                case CommandType.Decorated:
                    return new Decorated(container, (DecorationType) (((object[]) data)[0]), (Position) (((object[]) data)[1]));
                case CommandType.Arrow:
                    return new Arrow(container, (ArrowType) (((object[]) data)[0]), (Position) (((object[]) data)[1]));
                case CommandType.Box:
                    return new Box(container, (BoxType) data);
                case CommandType.Matrix:
                    return new MatrixEquation(container, ((int[]) data)[0], ((int[]) data)[1]);
                default:
                    return null;
            }
        }
    }
}