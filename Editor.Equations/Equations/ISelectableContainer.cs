namespace Editor
{
    public interface ISelectableContainer
    {
        int SelectionStartIndex { get; set; }
        int SelectedItems { get; set; }
    }
}