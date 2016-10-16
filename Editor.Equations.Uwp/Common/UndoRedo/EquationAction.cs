namespace Editor
{
    public abstract class EquationAction
    {
        public bool UndoFlag { get; set; }
        public ISupportsUndo Executor { get; set; }
        public int FurtherUndoCount { get; set; }

        protected EquationAction(ISupportsUndo executor)
        {
            Executor = executor;
            UndoFlag = true;
        }
    }
}

