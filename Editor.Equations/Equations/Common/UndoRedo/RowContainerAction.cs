namespace Editor
{   
    public sealed class RowContainerAction : EquationAction
    {
        public int Index { get; set; }
        public int ChildIndexInRow { get; set; }
        public int CaretIndex { get; set; }
        public IEquationRow Equation { get; set; }

        public RowContainerAction(ISupportsUndo executor, int index, int childIndexInRow, int caretIndex, IEquationRow equation)
            : base(executor)
        {
            Index = index;
            ChildIndexInRow = childIndexInRow;
            CaretIndex = caretIndex;
            Equation = equation;
        }
    }    
}

