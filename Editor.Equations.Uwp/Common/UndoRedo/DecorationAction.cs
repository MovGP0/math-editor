namespace Editor
{
    public class DecorationAction : EquationAction
    {
        public CharacterDecorationInfo [] CharacterDecorations { get; set; }
        public DecorationAction(ISupportsUndo executor, CharacterDecorationInfo [] cdi)
            : base(executor)
        {
            CharacterDecorations = cdi;
        }
    }
}